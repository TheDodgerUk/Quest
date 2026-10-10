using Fixtures;
using OscController;
using System;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text.RegularExpressions;
using UnityEngine;
using UnityEngine.EventSystems;

namespace HouseBuilder
{
    public class HouseBuilderInputLighting : HouseBuilderInput
    {
        private Lighting m_LightingRef;

        private ModeContainerCatalogueGeneratedData ModeContainerRef;


        public override void Initialise()
        {
            ModeContainerRef = HouseBuilderRoot.Instance.InputContents.ModeContainerRef;
            FixtureContainer.CreateInstance();
            FixtureContainer.Instance.Initialise();          
        }


        protected override void UpdateInputControlsFor_Nothing()
        {
            if (HouseBuilderRoot.Instance.PanMode == true)
            {
                return;
            }

            if ((true == HouseBuilderConstants.IsMouseCommand(MouseCommand.Left, PressType.Hold)) && false == EventSystem.current.IsPointerOverGameObject())
            {
                m_MainRay = Camera.main.ScreenPointToRay(HouseBuilderConstants.MousePosition());
                Debug.DrawRay(m_MainRay.origin, m_MainRay.direction * 100, Color.red, 1);


                // find only m_MainColiider on gameObject
                Interaction newInteraction = null;
                var hits = Physics.RaycastAll(m_MainRay).OrderBy(h => h.distance).ToList();
                foreach (var hit in hits)
                {
                    m_MainRayCastHit = hit;
                    newInteraction = GetCurrentInteractionClassForFillRoomLeftMouse(m_MainRayCastHit.collider, StructureType.None);
                    if(newInteraction is InteractionGameObject)
                    {
                        if (newInteraction.LightingRef != null)
                        {
                            m_LightingRef = newInteraction.LightingRef;
                            break;
                        }
                        else
                        {
                            m_LightingRef = null;
                        }

                    }
                }  
                SetCurrentInteraction(newInteraction);
            }
        }


 

    }
}
