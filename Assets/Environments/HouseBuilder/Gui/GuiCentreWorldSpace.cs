using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Playables;
using UnityEngine.SceneManagement;
using System;
using UnityEngine.XR;
using UnityEngine.EventSystems;
using UnityEngine.UI;
using EnvironmentHelpers;

namespace HouseBuilder
{
    public class GuiCentreWorldSpace : MonoBehaviour
    {
        public void Initialise()
        {
            var direct = this.transform.GetDirectChildren();
            for(int i = 0; i < direct.Count; i++)
            {
                switch (direct[i].name)
                {
                    case nameof(MeasurementLabelPanel):
                        MeasurementLabelPanel measurementLabelPanel = direct[i].AddComponent<MeasurementLabelPanel>();
                        measurementLabelPanel.Initialise();
                        break;
                    default:
                        Debug.LogError($"GuiCentre does not contain {direct[i].name} ");
                        break;
                }
            }
            
        }
    }
}

