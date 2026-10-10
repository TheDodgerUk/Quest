using UnityEngine;

public class InteractionGameObjectPlotWallCollider : Interaction
{
    public int WallIndex { get; private set; }
    public int WallPartIndex { get; private set; }

    public override void Awake()
    {
        this.gameObject.ForceComponent<BoxCollider>();
    }
    public void SetData(int wallIndex, int partIndex)
    {
        WallIndex = wallIndex;
        WallPartIndex = partIndex;
        this.name = $"PlotColliderHolder wallIndex {WallIndex}, partIndex {WallPartIndex}";

    }

}
