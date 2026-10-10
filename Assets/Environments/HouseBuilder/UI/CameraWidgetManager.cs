using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Analytics;
using UnityEngine.UI;
using UnityEngine.EventSystems;
using System.Collections;

namespace HouseBuilder
{
    public class CameraWidgetManager : MonoBehaviour
    {
        private Toggle m_Toggle;
        private CanvasGroup m_CanvasGroup;
        private Transform m_BtnsTransform;
        private Transform m_TglTransform;

        private const float MIN_BTNS_SCALE = 0f;
        private const float MAX_BTNS_SCALE = 1f;

        private Vector2 m_ToggleOnPosition = new Vector2(0,0);
        private Vector2 m_ToggleOffPosition = new Vector2(62,56);

        private List<Button> m_RotationBtns = new List<Button>();

        private string m_Direction = null;
        private bool m_MoveCamera = false;

        public void Initialise()
        {
            m_TglTransform = this.transform.Find("OnOffToggle");
            m_Toggle = m_TglTransform.GetComponent<Toggle>();
            m_Toggle.onValueChanged.AddListener((value) => ToggleWidget(value));

            m_BtnsTransform = m_TglTransform.Find("Buttons").transform;
            MovementBtnsSetup();
            RotationBtnsSetup();
            m_CanvasGroup = m_BtnsTransform.GetComponent<CanvasGroup>();

            m_Toggle.isOn = false;
        }

        private void ToggleWidget(bool state)
        {
            float start = state ? MIN_BTNS_SCALE : MAX_BTNS_SCALE;
            float end = state ? MAX_BTNS_SCALE : MIN_BTNS_SCALE;

            Vector2 startPos = state ? m_ToggleOffPosition : m_ToggleOnPosition;
            Vector2 endPos = state ? m_ToggleOnPosition : m_ToggleOffPosition;

            this.Create<ValueTween>(0.5f, EaseType.ExpoOut, () =>
            {
                m_CanvasGroup.interactable = state;
                m_CanvasGroup.blocksRaycasts = state;

            }).Initialise(start, end, (f) =>      //This is the widget animation, scaling and moving
            {
                Vector3 scale = Vector3.one * f;
                m_BtnsTransform.localScale = scale;
                m_CanvasGroup.alpha =  f;
                m_TglTransform.localPosition = Vector2.Lerp(m_ToggleOffPosition, m_ToggleOnPosition, f);
            });

            
        }
        //I've got some problems with the buttons to detect when the button was kept pressed so I used
        //EventTrigger to detect when the button was pressed and release
        private void MovementBtnsSetup()
        {
            Transform movementBtns = m_BtnsTransform.Find("MovementBtns");
            foreach (Transform btnTransform in movementBtns)
            {
                var trigger = btnTransform.GetComponent<UnityEngine.EventSystems.EventTrigger>();

                UnityEngine.EventSystems.EventTrigger.Entry entry = new UnityEngine.EventSystems.EventTrigger.Entry();
                entry.eventID = EventTriggerType.PointerDown;
                entry.callback.AddListener((data) => MoveCamera(btnTransform.name));
                trigger.triggers.Add(entry);


                UnityEngine.EventSystems.EventTrigger.Entry secondEntry = new UnityEngine.EventSystems.EventTrigger.Entry();
                secondEntry.eventID = EventTriggerType.PointerUp;
                secondEntry.callback.AddListener((data) => StopAllCameraMovements());
                trigger.triggers.Add(secondEntry);
            }
        }

        private void RotationBtnsSetup()
        {
            Transform rotateBtns = m_BtnsTransform.Find("RotationBtns");
            foreach (Transform btnTransform in rotateBtns)
            {
                if(btnTransform.name == "UpRot" || btnTransform.name == "DownRot")
                    m_RotationBtns.Add(btnTransform.GetComponent<Button>());


                var trigger = btnTransform.GetComponent<UnityEngine.EventSystems.EventTrigger>();

                UnityEngine.EventSystems.EventTrigger.Entry entry = new UnityEngine.EventSystems.EventTrigger.Entry();
                entry.eventID = EventTriggerType.PointerDown;
                entry.callback.AddListener((data) => RotateCamera(btnTransform.name));
                trigger.triggers.Add(entry);


                UnityEngine.EventSystems.EventTrigger.Entry secondEntry = new UnityEngine.EventSystems.EventTrigger.Entry();
                secondEntry.eventID = EventTriggerType.PointerUp;
                secondEntry.callback.AddListener((data) => StopAllCameraMovements());
                trigger.triggers.Add(secondEntry);
            }
        }

        //The movements are done with a coroutine to continue the movements when button keep pressed
        private void MoveCamera(string btnName)  
        {
            StartCoroutine(HouseBuilderRoot.Instance.InputCamera.MoveCameraWithWidget(btnName));
        }

        //When the buttons are released the coroutines stop
        private void StopAllCameraMovements()
        {
            StopAllCoroutines();
        }

        private void RotateCamera(string btnName)
        {
            StartCoroutine(HouseBuilderRoot.Instance.InputCamera.m_InteractionCameraOrbit.RotateCameraWithWidget(btnName));
        }

        //Up and Down Rotations need to be off when in 2D view, so the btns are disabled. This is only visual (the buttons look disabled)
        //Anyway since the interaction is done vai eventtrigger the buttons are still working, so I've to add a control on the functions
        public void EnableRotationBtns(bool state) 
        {
            foreach (var btn in m_RotationBtns)
                btn.interactable = state;
        }
    }
}
