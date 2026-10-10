using UnityEngine;

namespace TMPro
{
    [AddComponentMenu("Mesh/TextMeshPro - Additional Settings")]
    public class AdditionalSettings : MonoBehaviour
    {
        [SerializeField]
        private bool m_EnableCulling = false;

        private void OnEnable()
        {
            Apply();
        }

        public void Apply()
        {
            GetText().enableCulling = m_EnableCulling;
        }

        public TMP_Text GetText() => GetComponent<TMP_Text>();
    }
}
