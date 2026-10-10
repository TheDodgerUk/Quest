using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using static OscController.DataContainer;


// WORK IN PROGRESS
namespace OscController
{

    public class OscLoadSaveMenuManager : HouseBuilder.LoadSaveMenuManager
    {
        protected override string NEW_FILENAME => "New Lights";
        protected override string CONTENTS_JSON => OscControllerRoot.ALL_JSON;

        private string m_CurrentLoadedFile;



        protected override void SaveButtonPressed()
        {
            if (Core.Network.Users.CurrentUser != null)
            {
                string fileName = GetInputFieldText();
                if (string.IsNullOrEmpty(fileName) == false)
                {

                    if (IsFileNameAlreadyUsed(fileName))
                    {
                        OscGenericPromptMenu.Instance.ShowYesNoMenu("Do you want to overwrite existing file", () => SaveFileName(fileName, true), null);
                    }
                    else
                    {
                        SaveFileName(fileName, false);
                    }
                }
                else
                {
                    OscGenericPromptMenu.Instance.ShowOkayMenu("You need a validSaveName", null);
                }
            }
            else
            {
                OscGenericPromptMenu.Instance.ShowOkayMenu("Please log in before saving your scene.");
            }
        }




        protected override void SaveFileName(string fileName, bool overwrite)
        {
            TaskAction task = new TaskAction(4, () =>
            {
                Save(fileName);
            });

            Core.Network.DeleteManifestJsonFile(fileName + OscControllerRoot.ALL_JSON, (valid) => task.Increment());
            Core.Network.DeleteManifestJsonFile(fileName + OscControllerRoot.SEQUENCE_JSON, (valid) => task.Increment());
            Core.Network.DeleteManifestJsonFile(fileName + OscControllerRoot.TIME_LINE_JSON, (valid) => task.Increment());
            Core.Network.DeleteManifestJsonFile(fileName + OscControllerRoot.GENERIC_JSON, (valid) => task.Increment());
        }

        protected override void Save(string fileName)
        {
            m_CurrentLoadedFile = fileName;
            TaskAction task = new TaskAction(4, () =>
            {
                ShowSuccessFeedback();
                SetCurrentFileName(fileName);
                ResetInterface();
                DisableAllToggle();
                Toggle(false);
            });

            foreach (var scene in DataContainer.Instance.SequenceDataRef)
            {
                foreach (var item in scene.m_SliderData)
                {
                    int index = DataContainer.Instance.AllUsedFixturesRef.m_UsedFixtureDataList.FindIndex(e => e == item.m_FixtureData);
                    item.m_FixtureDataIndex = index;
                }
            }

            bool valid = true;
            foreach (var group in DataContainer.Instance.TimeLineDataRef)
            {
                if (group != null && group.m_InputFieldName != null)
                {
                    group.m_GroupName = group.m_InputFieldName.text;
                    foreach (var item in group.m_GroupLinkageData)
                    {
                        int index = DataContainer.Instance.SequenceDataRef.FindIndex(e => e == item.m_Sequence);
                        item.m_SequenceIndex = index;
                    }
                }
                else
                {
                    Debug.LogError($"group is null, why ?????????????");
                    valid = false;
                }
            }

            if (valid == true)
            {
                Core.Network.SaveManifestJsonFile<OscAllUsedFixtures>(fileName + OscControllerRoot.ALL_JSON, DataContainer.Instance.AllUsedFixturesRef, (valid) => { task.Increment(); });
                Core.Network.SaveManifestJsonFile<List<OscSequenceData>>(fileName + OscControllerRoot.SEQUENCE_JSON, DataContainer.Instance.SequenceDataRef, (valid) => { task.Increment(); });
                Core.Network.SaveManifestJsonFile<List<OscGroupData>>(fileName + OscControllerRoot.TIME_LINE_JSON, DataContainer.Instance.TimeLineDataRef, (valid) => { task.Increment(); });
                Core.Network.SaveManifestJsonFile<OscGenericData>(fileName + OscControllerRoot.GENERIC_JSON, DataContainer.Instance.GenericDataDataRef, (valid) => { task.Increment(); });
            }
            else
            {
                task.Increment();
                task.Increment();
                task.Increment();
                task.Increment();
            }
        }



        public void LoadFile(string fileName)
        {
            m_SelectedFileName = fileName;
            LoadButtonPressed();
        }

        public string GetSelectedFileName() => m_CurrentLoadedFile;

        protected override void LoadButtonPressed()
        {
            if (m_SelectedFileName != null)
            {
                OscControllerRoot.Instance.DeleteOldData();

                TaskAction task = new TaskAction(4, () =>
                {
                    OscControllerRoot.Instance.TimeLinePanelRef.ResetSliders();
                    OscControllerRoot.Instance.InitialiseLoadedData();
                    SetCurrentFileName(m_SelectedFileName);
                    ResetInterface();
                    DisableAllToggle();
                    Toggle(false);
                    OscControllerRoot.Instance.SequencePanelRef.SelectFirstItem();
                    OscControllerRoot.Instance.SequenceRunTimePanelRef.CreateDisplaySequenceItems();

                    var debugTest = DataContainer.Instance.TimeLineDataRef;
                });

                m_CurrentLoadedFile = m_SelectedFileName;
                Core.Network.LoadManifestJsonFile<OscAllUsedFixtures>(m_CurrentLoadedFile + OscControllerRoot.ALL_JSON, (data) =>
                {
                    DataContainer.Instance.AllUsedFixturesRef = data;
                    if(DataContainer.Instance.AllUsedFixturesRef == null)
                    {
                        DataContainer.Instance.AllUsedFixturesRef = new OscAllUsedFixtures();
                    }
                    task.Increment();
                });
                Core.Network.LoadManifestJsonFile<List<OscSequenceData>>(m_CurrentLoadedFile + OscControllerRoot.SEQUENCE_JSON, (data) =>
                {
                    DataContainer.Instance.SequenceDataRef = data;
                    if (DataContainer.Instance.SequenceDataRef == null)
                    {
                        DataContainer.Instance.SequenceDataRef = new List<OscSequenceData>();
                    }
                    task.Increment();
                });
                Core.Network.LoadManifestJsonFile<List<OscGroupData>>(m_CurrentLoadedFile + OscControllerRoot.TIME_LINE_JSON, (data) =>
                {
                    DataContainer.Instance.TimeLineDataRef = data;
                    if (DataContainer.Instance.TimeLineDataRef == null)
                    {
                        DataContainer.Instance.TimeLineDataRef = new List<OscGroupData>();
                    }

                    if (DataContainer.Instance.TimeLineDataRef.Count > 1)
                    {
                        Debug.LogError($"DataContainer.Instance.TimeLineDataRef   {DataContainer.Instance.TimeLineDataRef.Count}");
                    }
                    task.Increment();
                });

                Core.Network.LoadManifestJsonFile<OscGenericData>(m_CurrentLoadedFile + OscControllerRoot.GENERIC_JSON, (data) =>
                {
                    DataContainer.Instance.GenericDataDataRef = data;
                    if(DataContainer.Instance.GenericDataDataRef == null)
                    {
                        DataContainer.Instance.GenericDataDataRef = new OscGenericData();
                    }
                    task.Increment();
                });
            }
        }

        protected override void NewButtonPressed()
        {
            OscGenericPromptMenu.Instance.ShowYesNoMenu($"Are you sure you wish to make new lighting? ", () =>
            {
                OscControllerRoot.Instance.DeleteOldData();

                SetCurrentFileName(NEW_FILENAME);
                ResetInterface();

                DisableAllToggle();
            }, null);
        }

        protected override void DeleteButtonPressed()
        {
            if (m_SelectedFileName != null)
            {
                OscGenericPromptMenu.Instance.ShowYesNoMenu($"Are you sure you want to delete light: {m_SelectedFileName}? ", () =>
                {
                    if (m_SelectedFileName == m_CurrentFileName)
                    {
                        SetCurrentFileName(NEW_FILENAME);
                    }
                    TaskAction task = new TaskAction(4, () =>
                    {
                        FileNameListManualRefresh(() =>
                        {
                            ResetInterface();
                            DisableAllToggle();
                        });
                    });

                    Core.Network.DeleteManifestJsonFile(m_SelectedFileName + OscControllerRoot.ALL_JSON, (valid) => task.Increment());
                    Core.Network.DeleteManifestJsonFile(m_SelectedFileName + OscControllerRoot.SEQUENCE_JSON, (valid) => task.Increment());
                    Core.Network.DeleteManifestJsonFile(m_SelectedFileName + OscControllerRoot.TIME_LINE_JSON, (valid) => task.Increment());
                    Core.Network.DeleteManifestJsonFile(m_SelectedFileName + OscControllerRoot.GENERIC_JSON, (valid) => task.Increment());
                }, null);
            }
        }

        protected override void SetCurrentFileName(string currentScene)
        {
            m_CurrentFileName = currentScene;
            OscControllerRoot.Instance.HeaderPanelRef.SetCurrentFileName(currentScene);
        }

    }
}
