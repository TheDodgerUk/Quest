using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Events;
using UnityEngine.EventSystems;
using UnityEngine.UI;

[System.Serializable]
public class MyFloatEvent : UnityEvent<float>
{
}

[RequireComponent(typeof(Slider))]
public class SliderDrag : MonoBehaviour, IPointerUpHandler
{
    public MyFloatEvent onEndDrag = new MyFloatEvent();

    private float SliderValue
    {
        get
        {
            return gameObject.GetComponent<Slider>().value;
        }
    }

    public void OnPointerUp(PointerEventData data)
    {
        if (onEndDrag != null)
        {
            onEndDrag?.Invoke(SliderValue);
        }
    }
}