using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

namespace HouseBuilder
{
    public class RayCastTester : MonoBehaviour
    {
        public static RayCastTester Instance;
        public void Initialise()
        {
            Instance = this;
        }

        private Ray m_Ray = new Ray();
        private float m_Distance = 0f;
        public void SetRay(Ray ray, float length)
        {
            m_Ray = ray;
            m_Distance = length;
            SetPosition();
        }

        public void SetRay(Vector3 origin, Vector3 direction, float length)
        {
            m_Ray.origin = origin;
            m_Ray.direction = direction;
            m_Distance = length;
            SetPosition();
        }

        private void SetPosition()
        {
            this.transform.position = m_Ray.origin;
            this.transform.rotation = Quaternion.Euler(m_Ray.direction);
        }

        [InspectorButton]
        private void Test()
        {

            RaycastHit[] hit = Physics.RaycastAll(m_Ray.origin, m_Ray.direction, m_Distance).OrderBy(h => h.distance).ToArray();
            Debug.LogError($"RayCast {m_Ray.direction}, distance {m_Distance}  hit count:{hit.Length}");
            for (int i = 0; i < hit.Length; i++)
            {
                Debug.LogError($"RayCastTester {hit[i].collider.name}", hit[i].collider);
            }
        }


        [InspectorButton]
        private void TestDown()
        {

            RaycastHit[] hit = Physics.RaycastAll(m_Ray.origin, Vector3.down, 10f).OrderBy(h => h.distance).ToArray();
            Debug.LogError($"RayCast TestDown {m_Ray.direction}, distance {m_Distance}  hit count:{hit.Length}");
            for (int i = 0; i < hit.Length; i++)
            {
                Debug.LogError($"RayCastTester {hit[i].collider.name}", hit[i].collider);
            }
        }



        void OnDrawGizmos()
        {
#if UNITY_EDITOR
            Gizmos.color = Color.black;
            Gizmos.DrawSphere(this.transform.position, 0.05f);
            Gizmos.DrawRay(m_Ray.origin, m_Ray.direction * m_Distance);

            Gizmos.color = Color.white;
            Gizmos.DrawSphere(this.transform.position + (m_Ray.direction * m_Distance), 0.05f);
#endif
        }



    }
}
