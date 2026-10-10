using System;
using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.Events;
using UnityEngine.EventSystems;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

namespace HouseViewer
{

    public class SoundLightMenu : MonoBehaviour
    {
        public static SoundLightMenu Instance;

        private Button m_BtnPlay;
        private Button m_BtnReset;
        private Button m_BtnStop;
        private Button m_BtnExit;

        private Slider m_Slider;

        private AudioSource m_ButtonPressSource;

        public void Initialise()
        {
            Instance = this;

            Core.AssetBundlesRef.AudioClipAssetBundleRef.GetItem(null, "ButtonPress", (clip) =>
            {
                m_ButtonPressSource = this.gameObject.ForceComponent<AudioSource>();
                m_ButtonPressSource.clip = clip;
            });

            var canvas = gameObject.GetComponentInChildren<Canvas>();
            canvas.gameObject.ForceComponent<VRUICanvas>();

            m_BtnPlay = gameObject.transform.SearchComponent<Button>("BtnPlay");
            m_BtnPlay.onClick.AddListener(PlaySound);

            m_BtnReset = gameObject.transform.SearchComponent<Button>("BtnReset");
            m_BtnReset.onClick.AddListener(ResetSound);

            m_BtnStop = gameObject.transform.SearchComponent<Button>("BtnStop");
            m_BtnStop.onClick.AddListener(StopSound);

            m_BtnExit = gameObject.transform.SearchComponent<Button>("BtnExit");
            m_BtnExit.onClick.AddListener(Exit);


            m_Slider = gameObject.transform.SearchComponent<Slider>("Slider");
            m_Slider.onValueChanged.AddListener((amount) => SliderChange(amount));


            var timeSlider = m_Slider.transform.gameObject.ForceComponent<ImageMouseBase>();
            timeSlider.OnPointerEnd.AddListener(SliderRelease);

            Core.Mono.WaitUntil(1, () => (OscController.OscControllerRoot.Instance != null && OscController.OscControllerRoot.Instance.PlayDataRef != null), () =>
            {
                OscController.OscControllerRoot.Instance.PlayDataRef.GetPlayTimeLine((time) => m_Slider.SetValueWithoutNotify(time));
            });


            this.gameObject.SetActive(false);
        }


        public void SetSliderMaxValue(float max)
        {
            m_Slider.maxValue = max;
            m_Slider.SetValueWithoutNotify(0f);
        }

        private void SliderChange(float time)
        {
            m_ButtonPressSource.Play();
            HouseViewerRoot.Instance.InputContents.ModeContainerRef.GetList.ForEach(e => e.InteractionRef.SteamSoundRef.SetTimeConnectedSounds(time));
            OscController.OscControllerRoot.Instance.PlayDataRef.PlayTimeOnTimeLine(time);
            if (time == 0f)
            {
                OscController.OscControllerRoot.Instance.PlayDataRef.ClearAllDmx();
            }
        }

        private void SliderRelease()
        {
            HouseViewerRoot.Instance.InputContents.ModeContainerRef.GetList.ForEach(e => e.InteractionRef.SteamSoundRef.SetTimeConnectedSounds(m_Slider.value));
            OscController.OscControllerRoot.Instance.PlayDataRef.PlayTimeLineFastUntil(m_Slider.value);
            if (OscController.OscControllerRoot.Instance.PlayDataRef.IsPlayTimeLine() == true)
            {
                HouseViewerRoot.Instance.PlayAllSoundAndLighting(m_Slider.value);
            }
        }



        private void OnEnable()
        {
            this.gameObject.PlaceInfrontMainCamera(2);
        }

        private void OnDisable()
        {
            this.gameObject.PlaceInfrontMainCamera(2);
        }


        private void Exit()
        {
            this.gameObject.SetActive(false);
        }

        private void PlaySound()
        {
            m_ButtonPressSource.Play();
            HouseViewerRoot.Instance.PlayAllSoundAndLighting(m_Slider.value);
        }

        private void ResetSound()
        {
            m_ButtonPressSource.Play();
            HouseViewerRoot.Instance.ResetAllSoundAndLighting();
            m_Slider.SetValueWithoutNotify(0f);
        }

        private void StopSound()
        {
            m_ButtonPressSource.Play();
            HouseViewerRoot.Instance.StopAllSoundAndLighting();
        }

    }
}
