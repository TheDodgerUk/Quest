using System;
using System.Collections;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

namespace HouseBuilder
{
    public class AssetSyncMenu: AssetSyncMenuBase
    {
        public static AssetSyncMenu Instance;
        public override void Initialise()
        {
            Instance = this;
            base.Initialise();
        }
    }

    public class AssetSyncMenuBase : MonoBehaviour
    {
        

        private CanvasGroup m_Group;
        private Slider m_ProgressSlider;
        private GameObject m_LoadingBar;
        private TextMeshProUGUI m_LblInfo;
        private TextMeshProUGUI m_LblProgressBarInfo;
        private RectTransform m_SpinningObject;
        private Button m_BtnCancel;
        private IEnumerator m_SpinningCoroutine;

        public virtual void Initialise()
        {
            m_Group = gameObject.GetComponent<CanvasGroup>();

            m_LblInfo = transform.Find("Information/InfoMsg").GetComponent<TextMeshProUGUI>();
            m_ProgressSlider = transform.Find("Information/LoadingBar/Slider").GetComponent<Slider>();
            m_LblProgressBarInfo = transform.Find("Information/LoadingBarInfo").GetComponent<TextMeshProUGUI>();
            m_LoadingBar = transform.Find("Information/LoadingBar").gameObject;
            m_SpinningObject = transform.Find("Information/Spinning").GetComponent<RectTransform>();
            m_BtnCancel = transform.Find("Information/BtnCancel").GetComponent<Button>();

            m_LblInfo.SetText("Changes to assets have been detected. Please wait while the database is updated.");
            m_ProgressSlider.value = 0;
            m_LblProgressBarInfo.SetText("Calculating...");
            SetVisible(false);
            Core.Environment.OnEnvironmentLoadingComplete += OnEnvironmentLoadingComplete;
        }

        private IEnumerator SpinningItem()
        {
            yield return null;
            while(true)
            {
                yield return new WaitForEndOfFrame();
                m_SpinningObject.Rotate(Vector3.forward, -250 * Time.deltaTime);
            }    
        }


        public void SetVisible(bool state)
        {
            SetProgress(0);
            if (m_Group != null)
            {
                m_Group.VisibleAndInteractive(state);
                this.gameObject.SetActive(true);
            }
            else
            {
                this.gameObject.SetActive(state);//TODO: find out why this is here as its not designed to work this way. If it has to do with the HouseViewer, should be a different script
            }
            if(state == false)
            {
                m_BtnCancel?.SetActive(false);
                if (m_SpinningCoroutine != null)
                {
                    StopCoroutine(m_SpinningCoroutine);
                }              
            }
        }

        public void SetActiveSpinning()
        {
            m_LblProgressBarInfo.SetActive(false);
            m_LoadingBar.SetActive(false);
            m_SpinningObject.SetActive(true);

            if (m_SpinningCoroutine != null)
            {
                Core.Mono.StopCoroutine(m_SpinningCoroutine);
            }
            m_SpinningCoroutine = SpinningItem();
            Core.Mono.StartCoroutine(m_SpinningCoroutine);
        }

        public void SetProgressBar()
        {
            m_LblProgressBarInfo.SetActive(true);
            m_LoadingBar.SetActive(true);
            m_SpinningObject.SetActive(false);
        }



        private void OnEnvironmentLoadingComplete()
        {
            //And other stuff here for pre-load
            Core.Environment.OnEnvironmentLoadingComplete -= OnEnvironmentLoadingComplete;
        }

        public void SetProgressBarText(string message)
        {
            m_LblProgressBarInfo.SetText(message);
        }

        public void SetProgress(float value)
        {
            m_ProgressSlider.value = value;
        }

        public void SetHeaderText(string text)
        {
            m_LblInfo.SetText(text);
        }

        public void ShowCancel(Action pressedCallback)
        {
            m_BtnCancel?.SetActive(true);
            m_BtnCancel?.onClick.RemoveAllListeners();
            m_BtnCancel?.onClick.AddListener(() =>
            {
                // Turn the menu off before the invoke behaviour just incase we want to turn it back on again.
                SetVisible(false);
                pressedCallback?.Invoke();
            });
        }


        public void HideCancel()
        {
            m_BtnCancel?.SetActive(false);
        }
    }
}