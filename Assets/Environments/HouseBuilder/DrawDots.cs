using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Rendering;

namespace HouseBuilder
{
    public class DrawDots : MonoBehaviour
    {
        private PoolManagerLocalComponent<LineRenderer> m_PoolManagerLocal;

        private GameObject m_DotHolder;
        private string m_TextureName;
        private string m_CloneName;
        private float? m_Offset;

        public void Initialise(string textureName, string cloneName, float? offset)
        {
            m_TextureName = textureName;
            m_CloneName = cloneName;
            m_Offset = offset;
            CreateOneDot((prefabDot) =>
            {
                m_PoolManagerLocal = new PoolManagerLocalComponent<LineRenderer>(prefabDot, this.transform);
                m_DotHolder = new GameObject(nameof(m_DotHolder) + $" {cloneName}");
                m_DotHolder.transform.SetParent(this.transform);
            });
        }

        private void CreateOneDot(Action<GameObject> callback)
        {
            GameObject lineGameobject;
            if (HouseBuilderRoot.Instance != null)
            {
                lineGameobject = CachedObjects.Instance.InstantiateCachedGameObject(m_TextureName);
            }
            else
            {
                lineGameobject = new GameObject("temp");
                lineGameobject.ForceComponent<LineRenderer>();
            }

            lineGameobject.transform.parent = transform;
            lineGameobject.name = m_CloneName;
            lineGameobject.transform.rotation = Quaternion.Euler(90, 0, 0);

            lineGameobject.transform.localScale = new Vector3(0.5f, 0.5f, 0.2f);
            callback?.Invoke(lineGameobject);
        }

        private LineRenderer DrawDottedLine(Vector3 start, Vector3 end)
        {
            if (m_Offset != null)
            {
                start.y = (float)m_Offset;
                end.y = (float)m_Offset;
            }

            var line = m_PoolManagerLocal.SpawnObject();
            line.useWorldSpace = true;
            line.alignment = LineAlignment.View;
            line.textureMode = LineTextureMode.Tile;
            line.receiveShadows = false;
            line.shadowCastingMode = ShadowCastingMode.Off;
            line.positionCount = 2;
            line.SetPosition(0, start);
            line.SetPosition(1, end);
            return line;

        }

        public void ClearLines()
        {
            m_PoolManagerLocal.DeSpawnAll();
        }


        public LineRenderer DrawLines(GameObject start, GameObject end)
        {
            return DrawDottedLine(start.transform.position, end.transform.position);
        }

        public LineRenderer DrawLines(Vector3 start, Vector3 end)
        {
            return DrawDottedLine(start, end);
        }

        public LineRenderer DrawLines(GameObject start, Vector3 end)
        {
            return DrawDottedLine(start.transform.position, end);
        }
    }
}
