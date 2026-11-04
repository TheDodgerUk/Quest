// GPUBendShaderGenerator.cs
//
// Regenerates Assets/.../GPUBend/GPUBend.shader from GPUBend.template.txt by
// splicing in GPUBend_URP.subshader.txt and/or GPUBend_HDRP.subshader.txt —
// but ONLY when the corresponding render pipeline package is actually
// installed in this project.
//
// Why this exists: Unity's shader compiler parses every SubShader block in a
// .shader file at import time, regardless of its "RenderPipeline" tag or
// which pipeline is active. If a SubShader references
// "Packages/com.unity.render-pipelines.core/ShaderLibrary/Core.hlsl" and that
// package isn't installed, the #include fails to resolve and the shader is
// broken — even on Built-in-only projects that will never use that
// SubShader. Keeping the URP/HDRP source in .txt files (which Unity's shader
// importer never touches) and generating the real .shader only with the
// blocks that are safe to compile avoids that entirely.
//
// Runs automatically:
//   - on every editor load / domain reload ([InitializeOnLoadMethod])
//   - whenever packages are added/removed/updated (PackageManager Events)
// Can also be run manually via ArcCanvas > Rebuild GPUBend Shader.

using System;
using System.IO;
using System.Linq;
using System.Text;
using UnityEditor;
using UnityEditor.PackageManager;
using UnityEngine;

// UnityEditor (legacy Asset Store package info) and UnityEditor.PackageManager
// both declare a type named PackageInfo. Alias explicitly so "PackageInfo"
// always resolves to the Package Manager one we actually want.
using PackageInfo = UnityEditor.PackageManager.PackageInfo;

namespace ArcCanvas.Editor
{
    [InitializeOnLoad]
    public static class GPUBendShaderGenerator
    {
        private const string UrpPackageId = "com.unity.render-pipelines.universal";
        private const string HdrpPackageId = "com.unity.render-pipelines.high-definition";

        private const string SrpMarker = "/*__SRP_SUBSHADERS__*/";

        // Folder containing this script's parent (GPUBend/). Located relative
        // to this file so the tool keeps working if the whole GPUBend folder
        // is moved or renamed inside Assets.
        private static string RootFolder
        {
            get
            {
                var script = AssetDatabase.FindAssets($"t:Script {nameof(GPUBendShaderGenerator)}")
                    .Select(AssetDatabase.GUIDToAssetPath)
                    .FirstOrDefault(p => p.EndsWith(nameof(GPUBendShaderGenerator) + ".cs"));

                if (string.IsNullOrEmpty(script))
                    return null;

                // script is at .../GPUBend/Editor/GPUBendShaderGenerator.cs
                var editorDir = Path.GetDirectoryName(script);
                return Path.GetDirectoryName(editorDir);
            }
        }

        private static string TemplatePath => CombineOrNull(RootFolder, "GPUBend.template.txt");
        private static string UrpBlockPath => CombineOrNull(RootFolder, "GPUBend_URP.subshader.txt");
        private static string HdrpBlockPath => CombineOrNull(RootFolder, "GPUBend_HDRP.subshader.txt");
        private static string OutputShaderPath => CombineOrNull(RootFolder, "GPUBend.shader");

        private static string CombineOrNull(string root, string file) =>
            string.IsNullOrEmpty(root) ? null : (root + "/" + file);

        static GPUBendShaderGenerator()
        {
            // Re-run whenever the Package Manager finishes adding/removing/
            // updating packages, so installing URP/HDRP mid-session upgrades
            // the shader without requiring a manual step.
            Events.registeredPackages += OnPackagesChanged;
        }

        private static void OnPackagesChanged(PackageRegistrationEventArgs args)
        {
            Regenerate();
        }

        [InitializeOnLoadMethod]
        private static void OnLoad()
        {
            // Delay slightly so AssetDatabase is fully ready after a domain reload.
            // (EditorApplication.CallbackFunction takes no args, and method-group
            // conversion doesn't drop optional params, so this needs a lambda.)
            EditorApplication.delayCall += () => Regenerate();
        }

        [MenuItem("Tools/ArcCanvas/Rebuild GPUBend Shader")]
        public static void RebuildFromMenu()
        {
            Regenerate(force: true);
        }

        private static bool IsPackageInstalled(string packageId)
        {
            try
            {
                return PackageInfo.GetAllRegisteredPackages()
                    .Any(p => string.Equals(p.name, packageId, StringComparison.OrdinalIgnoreCase));
            }
            catch (Exception e)
            {
                Debug.LogWarning($"[GPUBendShaderGenerator] Could not query installed packages: {e.Message}");
                return false;
            }
        }

        private static void Regenerate(bool force = false)
        {
            try
            {
                var templatePath = TemplatePath;
                var urpPath = UrpBlockPath;
                var hdrpPath = HdrpBlockPath;
                var outPath = OutputShaderPath;

                if (templatePath == null || !File.Exists(templatePath))
                {
                    // Not an error worth spamming on every load if the folder
                    // simply isn't present in this project.
                    return;
                }

                var template = File.ReadAllText(templatePath);
                if (!template.Contains(SrpMarker))
                {
                    Debug.LogError($"[GPUBendShaderGenerator] {templatePath} is missing the {SrpMarker} marker; cannot splice SRP blocks.");
                    return;
                }

                var sb = new StringBuilder();

                bool urpInstalled = IsPackageInstalled(UrpPackageId);
                bool hdrpInstalled = IsPackageInstalled(HdrpPackageId);

                if (urpInstalled)
                {
                    if (File.Exists(urpPath))
                        sb.Append(File.ReadAllText(urpPath));
                    else
                        Debug.LogWarning($"[GPUBendShaderGenerator] URP detected but {urpPath} not found; skipping URP SubShader.");
                }

                if (hdrpInstalled)
                {
                    if (File.Exists(hdrpPath))
                        sb.Append(File.ReadAllText(hdrpPath));
                    else
                        Debug.LogWarning($"[GPUBendShaderGenerator] HDRP detected but {hdrpPath} not found; skipping HDRP SubShader.");
                }

                var generated = template.Replace(SrpMarker, sb.ToString());

                // Avoid unnecessary reimports/VCS churn if nothing changed.
                if (!force && File.Exists(outPath) && File.ReadAllText(outPath) == generated)
                    return;

                File.WriteAllText(outPath, generated);
                AssetDatabase.ImportAsset(outPath, ImportAssetOptions.ForceUpdate);

                Debug.Log($"[GPUBendShaderGenerator] Regenerated GPUBend.shader " +
                          $"(URP: {(urpInstalled ? "included" : "excluded")}, " +
                          $"HDRP: {(hdrpInstalled ? "included" : "excluded")}).");
            }
            catch (Exception e)
            {
                Debug.LogError($"[GPUBendShaderGenerator] Failed to regenerate GPUBend.shader: {e}");
            }
        }
    }
}