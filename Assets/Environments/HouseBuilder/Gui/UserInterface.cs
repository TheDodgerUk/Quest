using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using EnvironmentHelpers;

namespace HouseBuilder
{
    public class UserInterface : MonoBehaviour
    {
        protected CanvasGroup m_PanelGroupLayout;


        public virtual void Initialise(){ }
        protected virtual void Deinitialise() { }

        public bool IsOpen => m_PanelGroupLayout.interactable;

        public virtual void OpenWithTimer()
        {
            this.StopAllTweens();
            this.gameObject.SetActive(true);
            this.Create<ValueTween>(0.3f, EaseType.Linear, () =>
            {
                m_PanelGroupLayout.interactable = true;

            }).Initialise(0, 1f, (f) =>
            {
                m_PanelGroupLayout.alpha = Mathf.Lerp(0, 1, f);
            });
        }

        public virtual void CloseWithTimer()
        {
            this.StopAllTweens();
            this.Create<ValueTween>(0.3f, EaseType.Linear, () =>
            {
                m_PanelGroupLayout.interactable = false;
                Deinitialise();
                this.gameObject.SetActive(false);
            }).Initialise(0, 1f, (f) =>
            {
                m_PanelGroupLayout.alpha = Mathf.Lerp(1, 0, f);
            });
        }

        public virtual void OpenInstant()
        {
            this.gameObject.SetActive(true);
            if (m_PanelGroupLayout != null)
            {
                m_PanelGroupLayout.interactable = true;
                m_PanelGroupLayout.alpha = 1;
            }
        }

        public virtual void CloseInstant()
        {
            this.gameObject.SetActive(false);
            if (m_PanelGroupLayout != null)
            {
                m_PanelGroupLayout.interactable = false;
                m_PanelGroupLayout.alpha = 0;
            }
        }
    }
}
