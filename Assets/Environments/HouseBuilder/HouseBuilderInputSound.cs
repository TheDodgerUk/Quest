using System;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using UnityEngine.EventSystems;

namespace HouseBuilder
{
    public class HouseBuilderInputSound : HouseBuilderInput
    {
        public class InteractionCombined
        {
            private Interaction Start;
            private Interaction End;

            public InteractionCombined(Interaction int1, Interaction int2)
            {
                Start = int1;
                End = int2;
            }
            public bool Contains(Interaction int1, Interaction int2) => (int1 == Start && int2 == End) || (int2 == Start && int1 == End);
          
        }

        List<InteractionCombined> m_CreatedLinkage = new List<InteractionCombined>();



        protected override string ClassName => nameof(HouseBuilderInputSound);

        protected override float MANIPULATE_HELD_TIME => 0.1f;
        public ModeContainerCatalogueGeneratedData ModeContainerRef { get; set; }

        private Func<KeyCommand, bool> InputKey;
        private DrawDots m_DrawDotsFinished;
        private DrawDots m_DrawDotsEditing;
        private DrawDots m_DrawDotsError;
        private DrawSoundConnectionColliders m_DrawSoundConnectionColliders;


        public override void Initialise()
        {
            ModeContainerRef = HouseBuilderRoot.Instance.InputContents.ModeContainerRef;

            InputKey = HouseBuilderConstants.IsKeyboardCommand;

            m_DrawDotsFinished = this.AddComponent<DrawDots>();
            m_DrawDotsFinished.Initialise(HouseBuilderConstants.DOT_TEXTURE, "m_DrawDotsSoundFinished", null);

            m_DrawDotsEditing = this.AddComponent<DrawDots>();
            m_DrawDotsEditing.Initialise(HouseBuilderConstants.DOT_CURRENT_TEXTURE, "m_DrawDotsSoundEditing", null);

            m_DrawDotsError = this.AddComponent<DrawDots>();
            m_DrawDotsError.Initialise(HouseBuilderConstants.DOT_ERROR_TEXTURE, "m_DrawDotsSoundError", null);

            m_DrawSoundConnectionColliders = this.AddComponent<DrawSoundConnectionColliders>();
            m_DrawSoundConnectionColliders.Initialise();

            m_DrawDotsFinished.transform.SetParent(this.transform);
            m_DrawDotsEditing.transform.SetParent(this.transform);
            m_DrawDotsError.transform.SetParent(this.transform);

            m_DrawSoundConnectionColliders.transform.SetParent(this.transform);
        }

        public override void CleanUp()
        {
            m_DrawSoundConnectionColliders.ClearColliders();
            m_DrawDotsFinished.ClearLines();
            m_CreatedLinkage.Clear();
        }

        protected override void UpdateInputControlsFor_EarlyUpdate()
        {
            CleanUp();
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
                    newInteraction = m_MainRayCastHit.collider.GetComponent<InteractionSoundCollider>();
                    if (newInteraction == null)
                    {
                        newInteraction = GetCurrentInteractionClassForFillRoomLeftMouse(m_MainRayCastHit.collider, StructureType.None);
                        if (newInteraction != null && newInteraction.SteamSoundRef.AudioSoundType != ContentSteamAudioMetaData.SoundEnumType.None)
                        {
                            break;
                        }
                    }
                    else
                    {
                        break;
                    }
                }
                if(newInteraction != null && newInteraction is InteractionSoundCollider)
                {
                    Debug.LogError("InteractionSoundCollider");
                    CurrentInputMode = InputMode.Waiting;
                    GenericPromptMenu.Instance.ShowYesNoMenu("Do you want to break this connection", () =>
                    {
                        Disconnect(newInteraction);
                    }, () =>
                    {
                        CurrentInputMode = InputMode.Nothing;
                    });
                    return;
                }
                SetCurrentInteraction(newInteraction);
            }
        }

        private void Disconnect(Interaction newInteraction)
        {
            Debug.LogError("Break connection ");
            CurrentInputMode = InputMode.Nothing;
            var data = (InteractionSoundCollider)newInteraction;
            data.m_Start.SteamSoundRef.m_InteractionDirectlyConnectedTo.Remove(data.m_End);
            data.m_End.SteamSoundRef.m_InteractionDirectlyConnectedTo.Remove(data.m_Start);

            foreach (var audio in data.m_Start.SteamSoundRef.AudioSources)
            {
                audio.Stop();
            }

            foreach (var audio in data.m_End.SteamSoundRef.AudioSources)
            {
                audio.Stop();
            }

            if(GetLastInteraction() != null)
            {
                GetLastInteraction().SelectObject();
            }
        }

        protected override void UpdateInputControlsFor_FigureOutCreatingOrManipulating()
        {

        }

        protected override void UpdateInputControlsFor_Creating()
        {
            UpdateNewConnnection();
            if (true == HouseBuilderConstants.IsMouseCommand(MouseCommand.Left, PressType.Hold) && false == EventSystem.current.IsPointerOverGameObject())
            {
                CurrentInputMode = InputMode.Nothing;
                var item = GetMouseItem(out Vector3 pos);
                if (item != null)
                {
                    GetLastInteraction().SteamSoundRef.m_InteractionDirectlyConnectedTo.Add(item.InteractionRef);
                    item.InteractionRef.SteamSoundRef.m_InteractionDirectlyConnectedTo.Add(GetLastInteraction());
                    Debug.LogError("Connect to this");
                    GetLastInteraction().SelectObject();
                }
                m_DrawDotsError.ClearLines();
                m_DrawDotsEditing.ClearLines();
            }
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


        private void UpdateNewConnnection()
        {
            var item = GetMouseItem(out Vector3 pos);

            m_DrawDotsError.ClearLines();
            m_DrawDotsEditing.ClearLines();           
           

            if (item == null)
            {
                var line = m_DrawDotsError.DrawLines(GetCurrentInteractions().First().RealWorldBoundsCentre(), pos);
                line.alignment = LineAlignment.View;
                line.startWidth = 0.03f;
                line.endWidth = 0.03f;
            }
            else
            {
                var line = m_DrawDotsEditing.DrawLines(GetCurrentInteractions().First().RealWorldBoundsCentre(), item.InteractionRef.RealWorldBoundsCentre());
                line.alignment = LineAlignment.View;
                line.startWidth = 0.03f;
                line.endWidth = 0.03f;
            }
        }

        private CatalogueGeneratedData GetMouseItem(out Vector3 pos)
        {
            m_MainRay = Camera.main.ScreenPointToRay(HouseBuilderConstants.MousePosition());
            var hits = Physics.RaycastAll(m_MainRay).OrderBy(h => h.distance).ToList();
            pos = Vector3.zero;
            Interaction hitInteraction = null;
            foreach (var hit in hits)
            {
                pos = hit.point;
                hitInteraction = hit.collider.gameObject.GetComponentInParent<Interaction>();
                break;
            }

            var items = ModeContainerRef.GetList.FindAll(e => e.InteractionRef.SteamSoundRef.AudioSoundType != ContentSteamAudioMetaData.SoundEnumType.None);
            var item = items.Find(e => (e.InteractionRef == hitInteraction));
            return item;
        }



        protected override void UpdateInputControlsFor_LateUpdate()
        {
            CleanUp();
            var items = ModeContainerRef.GetList.FindAll(e => e.InteractionRef.SteamSoundRef.AudioSoundType != ContentSteamAudioMetaData.SoundEnumType.None);
            foreach(var item in items)
            {
                foreach(var connected in item.InteractionRef.SteamSoundRef.m_InteractionDirectlyConnectedTo)
                {
                    bool found = false;
                    foreach(var created in m_CreatedLinkage)
                    {
                        if(created.Contains(connected, item.InteractionRef) == true)
                        {
                            found = true;
                            break;
                        }
                        
                    }
                    if (found == false)
                    {
                        var line = m_DrawDotsFinished.DrawLines(item.InteractionRef.RealWorldBoundsCentre(), connected.RealWorldBoundsCentre());
                        line.alignment = LineAlignment.View;
                        line.startWidth = 0.03f;
                        line.endWidth = 0.03f;
                        m_DrawSoundConnectionColliders.DrawCollider(item.InteractionRef, connected, item.InteractionRef.RealWorldBoundsCentre(), connected.RealWorldBoundsCentre());
                        m_CreatedLinkage.Add(new InteractionCombined(connected, item.InteractionRef));
                    }
                }
            }
        }

    }
}
