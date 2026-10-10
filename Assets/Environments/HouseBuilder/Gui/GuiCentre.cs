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
    public class GuiCentre: MonoBehaviour
    {
        public void Initialise()
        {
            var direct = this.transform.GetDirectChildren();
            for(int i = 0; i < direct.Count; i++)
            {
                switch (direct[i].name)
                {
                    case nameof(WallPointMeasurementLabelPanel):
                        WallPointMeasurementLabelPanel measurementLabelPanel = direct[i].AddComponent<WallPointMeasurementLabelPanel>();
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

