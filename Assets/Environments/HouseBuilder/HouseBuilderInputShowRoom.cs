using System;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using UnityEngine.EventSystems;

namespace HouseBuilder
{
    public class HouseBuilderInputShowRoom : HouseBuilderInput
    {
        protected override string ClassName => nameof(HouseBuilderInputShowRoom);

        public override void Initialise()
        {
        }

        public override void CleanUp()
        {

        }

        protected override void UpdateInputControlsFor_EarlyUpdate()
        {
            CleanUp();
           // HBGUIManager.Instance.ShowRoomSettingsMenuRef.ManualUpdate();
        }

        protected override void UpdateInputControlsFor_Nothing()
        {
            if(HouseBuilderConstants.IsKeyboardCommand(KeyCommand.Cancel) == true)
            {
                //HBGUIManager.Instance.ShowRoomSettingsMenuRef.SetVisible(true);
                HBGUIManager.Instance.ShowRoomSettingsMenuRef.CancelCameraMovement();
                HBGUIManager.Instance.m_ViewMode.SetVisible(true);
                HBGUIManager.Instance.CameraWidgetRef.SetActive(true);
            }
        }


        protected override void UpdateInputControlsFor_FigureOutCreatingOrManipulating()
        {

        }

        protected override void UpdateInputControlsFor_Creating()
        {

        }


        protected override void UpdateInputControlsFor_Manipulating()
        {

        }

        protected override void UpdateInputControlsFor_Add()
        {

        }

        protected override void UpdateInputControlsFor_ManipulatingOther()
        {

        }

    }
}
