using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.UI.Extensions;

namespace OscController
{
    public class WheelPanel
    {
        private WheelPanelMono m_WheelPanelMono;
        
        public WheelPanel(WheelPanelMono wheelPanelMono)
        {
            m_WheelPanelMono = wheelPanelMono;
            m_WheelPanelMono.Initialise();
        }

        public void SetWheelData(SliderPrefab slider, Transform trans) => m_WheelPanelMono.SetWheelData(slider, trans);
        public void SetActive(bool active) => m_WheelPanelMono.SetActive(active);
        public bool IsActive => m_WheelPanelMono.isActiveAndEnabled;
    }

    public class WheelPanelMono : MonoBehaviour
    {
        private const float F_DISTANCE_MULTIPLYER = 16f;
        private const string WHEEL_ITEM_PREFAB = "WheelItemPrefab";

        private RadialLayout m_RadialLayout;
        private PoolManagerLocalComponent<Button> m_ButtonPool;

        public void Initialise()
        {
            m_RadialLayout = this.gameObject.GetComponentInChildren<RadialLayout>();
            var prefab = CachedObjects.Instance.InstantiateCachedGameObject(WHEEL_ITEM_PREFAB);
            m_ButtonPool = new PoolManagerLocalComponent<Button>(prefab, m_RadialLayout.transform);
            Destroy(prefab);
            m_RadialLayout.fDistance = 0; // going to use this to see if it has objects in it 
            this.SetActive(false);
        }


        // SliderSimplePanel.cs  private void CreateScrollView(SliderPrefab sliderPrefab, Button buttonPosition)
        public void SetWheelData(SliderPrefab slider, Transform trans)
        {
            // move to position
            m_RadialLayout.transform.position = trans.position;
            m_RadialLayout.MinAngle = 90f;
            m_RadialLayout.StartAngle = 270f;

            m_ButtonPool.DeSpawnAll();
            for (int i = 0; i < slider.m_WheelData.Count; i++)
            {
                SliderPrefab.ButtonText newItem = new SliderPrefab.ButtonText(m_ButtonPool.SpawnObject(m_RadialLayout.transform));
                int radialIndex = i;
                var currentWheelData = slider.m_WheelData[radialIndex];

                // set the colour of the button on the wheel 
                if (newItem.ButtonRef.image != null)
                {
                    newItem.ButtonRef.image.color = OscControllerRoot.UN_SELECTED; // clear the colour
                }
                else
                {
                    Debug.LogError("newItem.ButtonRef.image is null ???", newItem.ButtonRef.gameObject);
                }
                newItem.TextRef.text = "";

                float halfDmxValue = Mathf.Lerp(currentWheelData.DmxMin, currentWheelData.DmxMax, 0.5f);
                OscControllerRoot.Instance.SliderPanelRef.SetSpecialButton(halfDmxValue, slider, newItem);

                if (currentWheelData.m_Color != null)
                {
                    newItem.ButtonRef.image.color = (Color)currentWheelData.m_Color;
                }

                newItem.ButtonRef.onClick.RemoveAllListeners();
                newItem.ButtonRef.onClick.AddListener(() =>
                {
                    slider.m_Slider.value = Mathf.Lerp(currentWheelData.DmxMin, currentWheelData.DmxMax, 0.5f);

                    OscControllerRoot.Instance.SliderPanelRef.DisplayVisualsAndSendSlideData(slider.m_Slider.value, slider.m_DmxIndex, slider, OscControllerRoot.Instance.CurveEditorPanelRef.SpecailButton);
                    this.SetActive(false);
                });
            }
            m_RadialLayout.fDistance = slider.m_WheelData.Count * F_DISTANCE_MULTIPLYER;
        }

    }


}
