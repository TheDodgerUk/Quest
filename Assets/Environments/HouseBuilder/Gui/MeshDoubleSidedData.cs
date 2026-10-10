using System.Collections.Generic;
using UnityEngine;

namespace HouseBuilder
{
    public class MeshDoubleSidedData : MonoBehaviour
    {
        public float m_Height;
        public MeshFilter m_MeshFilter;
        public MeshCollider m_MeshCollider;
        public MeshRenderer m_MeshRender;

        public Mesh m_LookingUpMesh;
        public Mesh m_LookingDownMesh;
        public Vector3[] m_LookingUpVertices;
        public Vector3[] m_LookingDownVertices;

        public List<Vector3> m_LookingDownNormals = new List<Vector3>();
        public List<Vector3> m_LookingUpNormals = new List<Vector3>();

        private void SetMesh(bool lookingDown)
        {
            if (lookingDown == true)
            {
                m_MeshFilter.mesh = m_LookingDownMesh;
                m_MeshCollider.sharedMesh = m_LookingDownMesh;
                Interaction.SetGeneratedMeshRenderer(m_MeshRender);
            }
            else
            {
                m_MeshFilter.mesh = m_LookingUpMesh;
                m_MeshCollider.sharedMesh = m_LookingUpMesh;
                Interaction.SetGeneratedMeshRenderer(m_MeshRender);
            }
        }

        [InspectorButton]
        public void SetLookingDown() => SetMesh(true);

        [InspectorButton]
        public void SetLookingUp() => SetMesh(false);
    }
}