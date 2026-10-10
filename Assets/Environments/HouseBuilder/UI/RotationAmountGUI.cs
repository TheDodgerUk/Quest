using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;

namespace HouseBuilder
{
    public class RotationAmountGUI : MonoBehaviour
    {
        public float WIDGET_OFFSET_X = 150;
        public float WIDGET_OFFSET_Y = -10;

        private TextMeshProUGUI m_Amount;
        private Canvas m_Canvas;

        public void Initialise(Canvas canvas)
        {
            m_Canvas = canvas;
            m_Amount = this.transform.Find("txtAmount").GetComponent<TextMeshProUGUI>();
            this.SetActive(false);
        }

        void Update()
        {
            SetImagePosition();
        }

        public void SetAmount(float amount)
        {
            m_Amount.text = amount.WrapTo180().ToString("0.00");
        }

        public void SetImagePosition()
        {
            if (m_Canvas != null)
            {
                Vector2 pos;
                RectTransformUtility.ScreenPointToLocalPointInRectangle(m_Canvas.transform as RectTransform, Input.mousePosition, m_Canvas.worldCamera, out pos);
                pos.x += WIDGET_OFFSET_X;
                pos.y += WIDGET_OFFSET_Y;
                transform.position = m_Canvas.transform.TransformPoint(pos);
            }
        }
    }
}