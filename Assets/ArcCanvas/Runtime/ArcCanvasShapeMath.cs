using UnityEngine;

namespace ArcCanvas
{
    public enum ArcShape
    {
        Cylinder,
        Ring,
        Sphere,
        Wave,
        Cone,
        Spiral,
        Ellipse
    }

    public enum ArcCurveDirection
    {
        [InspectorName("Wrap Away From Viewer (concave, default)")]
        Away,
        [InspectorName("Bulge Toward Viewer (convex)")]
        Toward
    }

    /// <summary>
    /// Pure math core of the whole package. Every shape has a Forward function
    /// (flat local point -> curved local point) used by both the mesh bender
    /// and the interaction raycaster, so what you see and what you can click
    /// never disagree.
    ///
    /// Every shape function returns (0,0,0) at the canvas's local (0,0) - i.e.
    /// the CENTER of the canvas rect is always the anchor point that never
    /// moves. Edges bend away from/toward the viewer from there. This is
    /// intentional and shape-independent: it's why bending "starts from the
    /// center" regardless of which shape or radius you pick.
    /// </summary>
    public static class ArcCanvasShapeMath
    {
        public static Vector3 Forward(ArcShape shape, Vector2 flatLocal, ArcCanvasShapeParams p)
        {
            Vector3 result;
            switch (shape)
            {
                case ArcShape.Cylinder: result = Cylinder(flatLocal, p); break;
                case ArcShape.Ring: result = Cylinder(flatLocal, p); break;
                case ArcShape.Sphere: result = Sphere(flatLocal, p); break;
                case ArcShape.Wave: result = Wave(flatLocal, p); break;
                case ArcShape.Cone: result = Cone(flatLocal, p); break;
                case ArcShape.Spiral: result = Spiral(flatLocal, p); break;
                case ArcShape.Ellipse: result = Ellipse(flatLocal, p); break;
                default: result = new Vector3(flatLocal.x, flatLocal.y, 0f); break;
            }

            // Single point of control for concave/convex - flips which side of
            // the canvas the curve's center circle sits on, without touching
            // any per-shape math above.
            if (p.curveDirection == ArcCurveDirection.Toward)
                result.z = -result.z;

            // Optional extra depth control: scales how far the curve pushes
            // into Z independently of radius/arc-spread. Off by default -
            // radius alone is usually enough - but useful when someone wants
            // a shape's silhouette (how wide the curve reads) decoupled from
            // how deep it visually pushes back/forward.
            if (p.useCustomDepth)
                result.z *= p.depthMultiplier;

            return result;
        }

        public static Vector3 Cylinder(Vector2 pt, ArcCanvasShapeParams p)
        {
            float r = Mathf.Max(p.radius, 1f);
            float theta = pt.x / r;
            float x = Mathf.Sin(theta) * r;
            float z = Mathf.Cos(theta) * r - r;
            return new Vector3(x, pt.y, z);
        }

        public static Vector3 Sphere(Vector2 pt, ArcCanvasShapeParams p)
        {
            float r = Mathf.Max(p.radius, 1f);
            float thetaX = pt.x / r;
            float thetaY = pt.y / r;
            float x = Mathf.Sin(thetaX) * r;
            float y = Mathf.Sin(thetaY) * r;
            float z = (Mathf.Cos(thetaX) + Mathf.Cos(thetaY)) * 0.5f * r - r;
            return new Vector3(x, y, z);
        }

        public static Vector3 Wave(Vector2 pt, ArcCanvasShapeParams p)
        {
            float r = Mathf.Max(p.radius, 1f);
            float z = Mathf.Sin((pt.x / r) * p.waveFrequency * Mathf.PI) * p.waveAmplitude;
            return new Vector3(pt.x, pt.y, z);
        }

        public static Vector3 Cone(Vector2 pt, ArcCanvasShapeParams p)
        {
            float r = Mathf.Max(p.radius, 1f);
            float t = Mathf.InverseLerp(-r, r, pt.y);
            float effectiveR = Mathf.Lerp(r, r * (1f - p.coneTaper), t);
            float theta = pt.x / r;
            float x = Mathf.Sin(theta) * effectiveR;
            float z = Mathf.Cos(theta) * effectiveR - r;
            return new Vector3(x, pt.y, z);
        }

        public static Vector3 Spiral(Vector2 pt, ArcCanvasShapeParams p)
        {
            float r = Mathf.Max(p.radius, 1f);
            float theta = (pt.x / r) * p.spiralTurns;
            float x = Mathf.Sin(theta) * r;
            float z = Mathf.Cos(theta) * r - r;
            float yOffset = (theta / (2f * Mathf.PI)) * p.spiralHeightPerTurn;
            return new Vector3(x, pt.y + yOffset, z);
        }

        public static Vector3 Ellipse(Vector2 pt, ArcCanvasShapeParams p)
        {
            float rx = Mathf.Max(p.ellipseRadiusX, 1f);
            float rz = Mathf.Max(p.ellipseRadiusZ, 1f);
            float theta = pt.x / rx;
            float x = Mathf.Sin(theta) * rx;
            float z = Mathf.Cos(theta) * rz - rz;
            return new Vector3(x, pt.y, z);
        }
    }

    [System.Serializable]
    public struct ArcCanvasShapeParams
    {
        public float radius;
        [Tooltip("Concave (wraps around the viewer) or convex (bulges toward the viewer).")]
        public ArcCurveDirection curveDirection;
        [Tooltip("Optional: scale the curve's Z-depth independently of radius. Off by default.")]
        public bool useCustomDepth;
        [Tooltip("Only used when Use Custom Depth is enabled. 1 = normal, >1 = exaggerated depth, <1 = flatter depth.")]
        public float depthMultiplier;
        public float waveAmplitude;
        public float waveFrequency;
        public float coneTaper;
        public float spiralTurns;
        public float spiralHeightPerTurn;
        public float ellipseRadiusX;
        public float ellipseRadiusZ;

        public static ArcCanvasShapeParams Default => new ArcCanvasShapeParams
        {
            radius = 800f,
            curveDirection = ArcCurveDirection.Away,
            useCustomDepth = false,
            depthMultiplier = 1f,
            waveAmplitude = 40f,
            waveFrequency = 2f,
            coneTaper = 0.3f,
            spiralTurns = 1f,
            spiralHeightPerTurn = 200f,
            ellipseRadiusX = 800f,
            ellipseRadiusZ = 400f
        };

        /// <summary>Cheap combined hash so ArcCanvasSettings can detect "did anything change" every frame without per-field diffing.</summary>
        public int ComputeHash()
        {
            unchecked
            {
                int h = radius.GetHashCode();
                h = h * 31 + (int)curveDirection;
                h = h * 31 + useCustomDepth.GetHashCode();
                h = h * 31 + depthMultiplier.GetHashCode();
                h = h * 31 + waveAmplitude.GetHashCode();
                h = h * 31 + waveFrequency.GetHashCode();
                h = h * 31 + coneTaper.GetHashCode();
                h = h * 31 + spiralTurns.GetHashCode();
                h = h * 31 + spiralHeightPerTurn.GetHashCode();
                h = h * 31 + ellipseRadiusX.GetHashCode();
                h = h * 31 + ellipseRadiusZ.GetHashCode();
                return h;
            }
        }
    }

    /// <summary>
    /// Which shapeParams fields are actually meaningful for a given shape, so
    /// both the custom Inspector and the runtime controller can show ONLY
    /// relevant sliders instead of every field for every shape - the single
    /// biggest thing slowing down "get to the shape I want in seconds".
    /// </summary>
    public static class ArcCanvasShapeFieldMap
    {
        public static bool UsesWave(ArcShape s) => s == ArcShape.Wave;
        public static bool UsesConeTaper(ArcShape s) => s == ArcShape.Cone;
        public static bool UsesSpiral(ArcShape s) => s == ArcShape.Spiral;
        public static bool UsesEllipse(ArcShape s) => s == ArcShape.Ellipse;
        public static bool UsesRadius(ArcShape s) => s != ArcShape.Ellipse; // ellipse uses its own X/Z radii instead
    }
}
