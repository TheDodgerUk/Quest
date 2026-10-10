using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;

namespace HouseBuilder
{
    public class RotationPanel : UserInterface
    {
        public static RotationPanel Instance;


        private Slider m_Slider;
        private Text m_Text;
        private Transform m_Transform;

        public override void Initialise()
        {
            Instance = this;
            this.SetActive(true);
            Slider sliderRef = transform.SearchComponent<Slider>("Slider");

            m_Slider = sliderRef;
            m_Slider.minValue = 0;
            m_Slider.maxValue = 360;
            m_Text = transform.SearchComponent<Text>("Text");

            m_PanelGroupLayout = this.GetComponent<CanvasGroup>();
            this.gameObject.SetActive(false);

            m_Slider.onValueChanged.AddListener((amount) =>
            {
                Vector3 rotation = m_Transform.eulerAngles;
                rotation.y = amount;
                m_Transform.eulerAngles = rotation;
            });
        }

        public void SetTransform(Transform transformRef)
        {
            m_Transform = transformRef;
        }

        private void Update()
        {
            if(null != m_Transform)
            {
                if (null != m_Slider)
                {
                    m_Slider.value = m_Transform.eulerAngles.y;
                    m_Text.text = ((int)m_Transform.eulerAngles.y).ToString();
                }
            }
        }
    }
}
