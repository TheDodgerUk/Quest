using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Rendering;

namespace HouseBuilder
{
    public class DrawPlotColliders : MonoBehaviour
    {
        private PoolManagerLocalComponent<InteractionGameObjectPlotWallCollider> m_PoolManagerLocal;

        private GameObject m_ColliderHolder;
        public void Initialise()
        {

            GameObject newOb = new GameObject("m_ColliderHolder");
            newOb.AddComponent<InteractionGameObjectPlotWallCollider>();
            m_PoolManagerLocal = new PoolManagerLocalComponent<InteractionGameObjectPlotWallCollider>(newOb, this.transform);
            m_ColliderHolder = new GameObject(nameof(m_ColliderHolder));

            newOb.transform.SetParent(this.transform);
            m_ColliderHolder.transform.SetParent(this.transform);
        }

        public void DrawCollider(int WallIndex, int partIndex, Vector3 start, Vector3 end)
        {

            var obj = m_PoolManagerLocal.SpawnObject();
            obj.transform.position = start;
            obj.transform.LookAt(end);
            obj.transform.position = Vector3.Lerp(start, end, 0.5f);
            obj.transform.localScale = new Vector3(0.3f, 0.1f, Vector3.Distance(start, end));
            obj.gameObject.name = "PlotColliderHolder";
            obj.SetData(WallIndex, partIndex);
        }

        public void ClearColliders()
        {
            m_PoolManagerLocal.DeSpawnAll();

        }
    }
}
