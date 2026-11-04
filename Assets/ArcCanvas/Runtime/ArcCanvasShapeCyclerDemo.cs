using UnityEngine;

namespace ArcCanvas
{
    /// <summary>Cycles the bound canvas through every shape in order, morphing each time. Wired to the demo scene's "Cycle Shape" button.</summary>
    [AddComponentMenu("")]
    public class ArcCanvasShapeCyclerDemo : MonoBehaviour
    {
        public ArcCanvasTransitionAnimator animator;

        static readonly ArcShape[] Order =
        {
            ArcShape.Cylinder, ArcShape.Ring, ArcShape.Sphere,
            ArcShape.Wave, ArcShape.Cone, ArcShape.Spiral, ArcShape.Ellipse
        };

        int _index;

        public void CycleToNextShape()
        {
            if (animator == null) return;
            _index = (_index + 1) % Order.Length;
            animator.MorphTo(Order[_index]);
        }
    }
}
