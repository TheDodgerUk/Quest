using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Rendering;

namespace HouseBuilder
{
    public class InteractionSoundCollider : Interaction
    {
        public Interaction m_Start;
        public Interaction m_End;

        public override void Awake()
        {
            this.gameObject.ForceComponent<BoxCollider>();
        }

    }

    public class DrawSoundConnectionColliders : MonoBehaviour
    {
        private PoolManagerLocalComponent<InteractionSoundCollider> m_PoolManagerLocal;

        private GameObject m_ColliderHolder;

        public void Initialise()
        {
            GameObject newOb = new GameObject("m_ColliderHolder");
            newOb.AddComponent<InteractionSoundCollider>();
            m_PoolManagerLocal = new PoolManagerLocalComponent<InteractionSoundCollider>(newOb, this.transform);
            m_ColliderHolder = new GameObject(nameof(m_ColliderHolder));

            newOb.transform.SetParent(this.transform);
            m_ColliderHolder.transform.SetParent(this.transform);
        }

        public void DrawCollider(Interaction startInteraction, Interaction endInteraction, Vector3 start, Vector3 end)
        {
            var obj = m_PoolManagerLocal.SpawnObject();
            obj.transform.position = start;
            obj.transform.LookAt(end);
            obj.transform.position = Vector3.Lerp(start, end, 0.5f);
            obj.transform.localScale = new Vector3(0.3f, 0.1f, Vector3.Distance(start, end));
            obj.gameObject.name = "InteractionSoundCollider";
            obj.m_Start = startInteraction;
            obj.m_End = endInteraction;
        }

        public void ClearColliders()
        {
            m_PoolManagerLocal.DeSpawnAll();

        }
    }
}
