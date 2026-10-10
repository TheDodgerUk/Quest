using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;

public class CompassUI : EventTrigger
{
    private const float MULTIPLIER = 0.25f;

    //The sun rises due east on only two days of the year, the equinoxes, near March 21 and September 22 each year. Every other day of the year it rises somewhere else
    // set Juialn Date to 80 (March 21), to double check , it needs an offset of 90%
    private const float OFFSET = 90;

    private Vector2 m_LastPosition = Vector2.zero;
    public float m_Rotation { get; private set; }
    private GameObject m_SkyBox;


    public void Initialise(GameObject skyBox)
    {
        m_SkyBox = skyBox;
    }

    public override void OnBeginDrag(PointerEventData eventData)
    {
        m_LastPosition = eventData.position;
    }

    public override void OnDrag(PointerEventData eventData)
    {
        m_Rotation += (m_LastPosition.x - eventData.position.x) * MULTIPLIER;
        m_LastPosition = eventData.position;
        SetCompass(m_Rotation);
    }

    public void SetCompass(float rotation)
    {
        this.gameObject.transform.rotation = Quaternion.Euler(0, 0, rotation);
        m_SkyBox.transform.rotation = Quaternion.Euler(0, rotation + OFFSET, 0);
    }
}

