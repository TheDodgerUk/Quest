using UnityEngine;

[AddComponentMenu("Mesh/Renderer Additional Settings")]
public class RendererAdditionalSettings : MonoBehaviour
{
    [SerializeField]
    private int m_SortingLayerId = 0;

    [SerializeField]
    private int m_SortingOrder = 0;

    private void OnEnable()
    {
        Apply();
    }

    public void Apply()
    {
        var r = GetRenderer();
        r.sortingOrder = m_SortingOrder;
        r.sortingLayerID = m_SortingLayerId;
        r.sortingLayerName = SortingLayer.IDToName(m_SortingLayerId);
    }

    public Renderer GetRenderer() => GetComponent<Renderer>();
}
