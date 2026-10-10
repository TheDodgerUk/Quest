using System;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;

namespace HouseBuilder
{
    public class ModeToggle : MonoBehaviour, IPointerEnterHandler
    {
        public Toggle m_Toggle;
        public Action<string> m_HighlightLabel;
        private string m_ModeName;
        private BuilderMode m_Mode;

        public virtual void Initialise(BuilderMode mode)
        {
            m_Toggle = GetComponent<Toggle>();
            m_Mode = mode;
            m_ModeName = mode.ToString().ToUpper() + " MODE";
            m_HighlightLabel = HBGUIManager.Instance.CurrentModeLabelFadeOff;

            m_Toggle.onValueChanged.AddListener((value) =>
            {
                m_HighlightLabel.Invoke(m_ModeName);

                if (value)
                {
                    if (HouseBuilderRoot.Instance.CurrentMode != m_Mode)
                    {
                        HouseBuilderRoot.Instance.CurrentMode = m_Mode;
                    }
                }
            });
        }

        public void OnPointerEnter(PointerEventData eventData)
        {
            m_HighlightLabel.Invoke(m_ModeName);
        }
    }

    public class ModeToggleHidden : ModeToggle
    {
        public override void Initialise(BuilderMode mode)
        {
            base.Initialise(mode);
            this.SetActive(false);
        }

        public void ForceOnChange()
        {
            this.SetActive(true);
            m_Toggle.isOn = true;
            this.SetActive(false);
        }
    }
}