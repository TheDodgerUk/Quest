using UnityEngine;

[ExecuteInEditMode]
public class RenderAtNight : MonoBehaviour
{
    public Sky sky;

    protected void OnEnable()
    {
        if (!sky)
        {
            Debug.LogError("Sky instance reference not set. Disabling script.");
            this.enabled = false;
        }
    }

    protected void Update()
    {
        this.GetComponent<Renderer>().enabled = sky.IsNight;
    }
}
