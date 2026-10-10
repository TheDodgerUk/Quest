using System;
using System.Collections;
using System.Collections.Generic;
using System.Text.RegularExpressions;
using UnityEngine;
//using static CurveLineRenderer;

namespace HouseBuilder
{
    public enum KeyCommand
    {
        Pan,
        MultipleSelect,
        Focus,
        RotateModifier,
        KeyboardMoveSlowModifier,
        Delete,
        Cancel,
        Duplicate,
        FPSMouseModifier,
        FPSMouseLockedToggle,
        CentreObject,

        MoveObjectForward,
        MoveObjectBackward,
        MoveObjectLeft,
        MoveObjectRight,

        RotateObjectClockWise,
        RotateObjectAntiClockWise,


        MoveCameraUp,
        MoveCameraDown,

        MoveCameraForward,
        MoveCameraBackward,
        MoveCameraLeft,
        MoveCameraRight,

        GridToggle,
        RecordStepForward,
        RecordStepBack,

    }
    public enum PressType
    {
        Tap,
        Hold
    }

    public enum MouseCommand
    {
        Left,
        Right,
        Middle,
        WheelUp,
        WheelDown,
    }

    public class HouseBuilderConstants
    {
        public const int CURVED_WALL_RESOLUTION = 24;
        public static readonly Color GRID_COLOR = new Color(0.7461286f, 0.7584166f, 0.764151f, 1f);
        public const int PRIMITIVE_WAIT_TIME = 2;
        public const int HIGHTLIGHT_WAIT_TIME = 2;
        public const int WALL_UPDATE_WAIT_TIME = 10;

        public const float Z_FIGHTING_OFFSET_GRID = 0.005f;
        public const float Z_FIGHTING_OFFSET = 0.01f;
        public const float Z_FIGHTING_OFFSET_BLUE_DOT = 0.013f;
        public const float Z_FIGHTING_OFFSET_TARGET = 0.016f;
        public static readonly Vector3 Z_FIGHTING_OFFSET_VECTOR3 = new Vector3(0, Z_FIGHTING_OFFSET_TARGET, 0);


        public const string CEILING_NAME = "newCeiling";
        public const string FLOOR_NAME = "newFloor";

        public const string DOT_TEXTURE = "Dot";
        public const string DOT_CURRENT_TEXTURE = "DotCurrent";
        public const string DOT_TEXTURE_FADE = "DotFade";
        public const string DOT_CURRENT_TEXTURE_FADE = "DotCurrentFade";
        public const string DOT_ERROR_TEXTURE = "DotError";

        public const string TARGET_TEXTURE = "Target";

        public const string WALL_INDICATOR_FLAG = "Flag";
        public const string WALL_INDICATOR_FLAGPOLE = "SimplePole";


        public const string GENERATED_UPPER_HULL = "Generated_Upper_Hull";
        public const string GENERATED_LOWER_HULL = "Generated_Lower_Hull";
        public const string GENERATED_ERROR = "GENERATED_ERROR";

        public const string SLICED_WALL_MATERIAL = "Material/Black";
        public static readonly int TILING_ID = Shader.PropertyToID("_MainTex");
        public static readonly int TILING_ID_SECONDARY = Shader.PropertyToID("_DetailAlbedoMap");

        public const string TEXTURE_SCALE = "Scale";

        public const float MAX_WALL_THICKNESS = 1;
        public const float MAX_WALL_HEIGHT = 5;

        public const float DOT_SIZE = 0.02f;
        public const float TARGET_SIZE = 0.1f;

        public const float WALL_POLE_DETECTION_RANGE = 0.02f;

        public const float SMALL_WALL_DEPTH = 0.2f;
        public const float SMALL_WALL_HEIGHT = 0.3f;

        public const float GAMEOBJECT_FADE_AMOUNT = 0.1f;

        public const float TOOLTIP_DELAY = 0.5f;

        public static readonly Vector3 CREATED_ITEM_START_POINT = new Vector3(0, -1000, 0);

        public static readonly Vector3 MOUSE_CENTRE_OF_SCREEN = new Vector3(Screen.width / 2, Screen.height / 2, 0);


        public static readonly Rect FULL_SCREEN = new Rect(0f, 0f, 1f, 1f);
        public static readonly Rect SMALL_SCREEN = new Rect(0.465f, 0.465f, 1f, 1f);


        public static string CONTENTS_JSON => $"_{Core.Network.Users.CurrentUser.m_UserName.ToLower()}_{nameof(HouseContentsData)}{GlobalConsts.JSON}";
        public static string FLOOR_JSON => $"_{Core.Network.Users.CurrentUser.m_UserName.ToLower()}_{nameof(FloorCollectionData)}{GlobalConsts.JSON}";
        public static string GENERAL_JSON => $"_{Core.Network.Users.CurrentUser.m_UserName.ToLower()}_{nameof(GeneralData)}{GlobalConsts.JSON}";

        public static string LOCAL_CONTENTS_JSON => $"{nameof(HouseContentsData)}";
        public static string LOCAL_FLOOR_JSON => $"{nameof(FloorCollectionData)}";
        public static string LOCAL_GENERAL_JSON => $"{nameof(GeneralData)}";

        public static int INVALID_GUI = -100;


        public static bool IsMouseCommand(MouseCommand mouse, PressType pressType)
        {
            switch (mouse)
            {
                case MouseCommand.Left:
                case MouseCommand.Right:
                case MouseCommand.Middle:
                    return MousePress(mouse, pressType);


                case MouseCommand.WheelUp:
                    return Input.mouseScrollDelta.y != 0;

                case MouseCommand.WheelDown:
                    return Input.mouseScrollDelta.y != 0;

                default:
                    break;
            }
            return false;
        }


        private static bool MousePress(MouseCommand code, PressType pressType)
        {
            if (pressType == PressType.Tap)
            {
                return Input.GetMouseButtonDown((int)code);
            }
            else
            {
                return Input.GetMouseButton((int)code);
            }
        }


        public static bool IsKeyboardCommand(KeyCommand key)
        {
            switch (key)
            {
                
                case KeyCommand.FPSMouseModifier:
                case KeyCommand.Pan:
                    return KeyPress(KeyCode.Space, PressType.Hold);

                case KeyCommand.FPSMouseLockedToggle:
                    return KeyPress(KeyCode.Tab, PressType.Tap);

                case KeyCommand.MultipleSelect:
                    return KeyPress(KeyCode.LeftControl, PressType.Hold) || KeyPress(KeyCode.RightControl, PressType.Hold);

                case KeyCommand.Focus:
                    return KeyPress(KeyCode.F, PressType.Tap);

                case KeyCommand.Delete:
                    return KeyPress(KeyCode.Delete, PressType.Tap) || KeyPress(KeyCode.Backspace, PressType.Tap);

                case KeyCommand.KeyboardMoveSlowModifier:
                    return KeyPress(KeyCode.LeftShift, PressType.Hold) || KeyPress(KeyCode.RightShift, PressType.Hold);

                case KeyCommand.RotateModifier:
                    return KeyPress(KeyCode.LeftAlt, PressType.Hold) || KeyPress(KeyCode.RightAlt, PressType.Hold);

                case KeyCommand.Duplicate:
                    return KeyPress(KeyCode.LeftControl, PressType.Hold) && KeyPress(KeyCode.D, PressType.Tap);


                case KeyCommand.MoveCameraUp:
                    return KeyPress(KeyCode.Q, PressType.Hold);

                case KeyCommand.MoveCameraDown:
                    return KeyPress(KeyCode.E, PressType.Hold);

                case KeyCommand.MoveCameraForward:
                    return KeyPress(KeyCode.W, PressType.Hold);

                case KeyCommand.MoveObjectForward:
                    return KeyPress(KeyCode.UpArrow, PressType.Hold);

                case KeyCommand.MoveCameraBackward:
                    return KeyPress(KeyCode.S, PressType.Hold);

                case KeyCommand.MoveObjectBackward:
                    return KeyPress(KeyCode.DownArrow, PressType.Hold);

                case KeyCommand.MoveCameraLeft:
                    return KeyPress(KeyCode.A, PressType.Hold);

                case KeyCommand.RotateObjectAntiClockWise:// Includes Holding Alt and Alt + Shift
                case KeyCommand.MoveObjectLeft:
                    return KeyPress(KeyCode.LeftArrow, PressType.Hold);

                case KeyCommand.MoveCameraRight:
                    return KeyPress(KeyCode.D, PressType.Hold);

                case KeyCommand.RotateObjectClockWise:// Includes Holding Alt and Alt + Shift
                case KeyCommand.MoveObjectRight:
                    return KeyPress(KeyCode.RightArrow, PressType.Hold);

                case KeyCommand.GridToggle:
                    return KeyPress(KeyCode.G, PressType.Tap);

                case KeyCommand.RecordStepForward:
                    return (KeyPress(KeyCode.LeftControl, PressType.Hold) && KeyPress(KeyCode.Y, PressType.Tap));

                case KeyCommand.RecordStepBack:
                    return (KeyPress(KeyCode.LeftControl, PressType.Hold) && KeyPress(KeyCode.Z, PressType.Tap));

                case KeyCommand.CentreObject:
                    return (KeyPress(KeyCode.C, PressType.Tap));

                case KeyCommand.Cancel:
                    return (KeyPress(KeyCode.Escape, PressType.Tap));

                default:
                    break;
            }
            return false;
        }

        private static bool KeyPress(KeyCode code, PressType pressType)
        {
            if(pressType == PressType.Tap)
            {
                return Input.GetKeyDown(code);
            }
            else
            {
                return Input.GetKey(code);
            }
        }

        public static Vector3 MousePosition()
        {
            if (HBGUIManager.Instance == null)
            {
                return Input.mousePosition;
            }

            if(HBGUIManager.Instance.m_ViewMode.CurrentFirstPersonType == FirstPersonType.Orbit)
            {
                return MOUSE_CENTRE_OF_SCREEN;
            }
            else
            {
                return Input.mousePosition;
            }
        }




        [SerializeField]
        public class GridInfo
        {
            [SerializeField]
            public float m_Width = 60; // 20, 40 , 60, 80 for each of the sizes
            [SerializeField]
            public float m_Spacing = 1f;
            [SerializeField]
            public Vector2 m_Offset = new Vector2();
            [SerializeField]
            public bool m_ShowGrid = true;
        }



        [SerializeField]
        public class GeneralData
        {
            [SerializeField]
            public GridInfo m_GridInfo = new GridInfo();

            [SerializeField]
            public int m_TimeOfDay = 4;

            [SerializeField]
            public SceneSettingsMenu.TimeFormat m_TimeFormat = SceneSettingsMenu.TimeFormat.PM;

            [SerializeField]
            public Color m_AmbientLight = new Color(255f / 255f, 251f / 255f, 211f / 255f);
            
            [SerializeField]
            public int m_FloorSizeIndex = 2;

            [SerializeField]
            public int m_DustParticleSystemMultiply = 0;

            [SerializeField]
            public float m_CompassRotation = 0;

            [SerializeField]
            public string m_LightingProfile = "";

           // [SerializeField]
           // public List<SplineData> m_SplineData = new List<SplineData>();
        }


        [SerializeField]
        public class SteamAudioData
        {
            [SerializeField]
            public string m_AudioFileName;

            [SerializeField]
            public string m_AudioFilerName;

            [SerializeField]
            public float m_Volume;

            [SerializeField]
            public float m_Time;

            [SerializeField]
            public bool m_Loop;

            [SerializeField]
            public SteamSoundChannels.PlayChannel m_PlayChannel = SteamSoundChannels.PlayChannel.Both;

            [SerializeField]
            public List<int> m_GUIDConnectedTo = new List<int>();
        }



        [SerializeField]
        public class FileData
        {
            [SerializeField]
            public string m_FileName;
        }

        [SerializeField]
        public class MaterialData : FileData
        {
            [SerializeField]
            public float m_TilingAmount;
            [SerializeField]
            public string m_MaterialName;

            [SerializeField]
            public Color m_MaterialColour = Color.white;

            [SerializeField]
            public SteamAudio.MaterialPreset m_SteamAudioMaterial = SteamAudio.MaterialPreset.Concrete;
        }

        [SerializeField]
        public class AdaptiveData : FileData
        {
            [SerializeField]
            public int[] m_MainItemScale = new int[Enum.GetNames(typeof(Axis)).Length];

            public AdaptiveData()
            {
                for(int i = 0; i < m_MainItemScale.Length; i++)
                {
                    m_MainItemScale[i] = 1;
                }
            }
        }

        [SerializeField]
        public class ModularData : FileData
        {
            [SerializeField]
            public List<int> m_SelectedChildIndexs = new List<int>();
        }

        [SerializeField]
        public class ScaleData : FileData
        {
            [SerializeField]
            public Vector3 m_Scale = Vector3.one;
        }


        [SerializeField]
        public class AudioData : FileData
        {
            public class AudioGroupData
            {
                [SerializeField] public string m_ColliderName = "";
                [SerializeField] public string m_AudioClip = "";
            }

            public List<AudioGroupData> m_AudioDatas = new List<AudioGroupData>();
        }

        [SerializeField]
        public class LightingData : FileData
        {
            [SerializeField] public int m_DmxStartNumber = 1;
            [SerializeField] public int m_ModeIndex = 1;

        }




        [SerializeField]
        public class StagingData : FileData
        {
            [SerializeField]
            public class Data
            {
                public int m_ConnectedToGUId = INVALID_GUI;
                public int m_ConnectedToPointIndex = INVALID_GUI;
                public int m_RotationIndex = INVALID_GUI;
                public int m_OwnPointIndex = INVALID_GUI;
            }
            [SerializeField] public List<Data> m_Connected = new List<Data>();
        }

        [SerializeField]
        public class CeilingData : MaterialData{ }

        [SerializeField]
        public class FloorData : MaterialData{ }

        public class WallData
        {
            [SerializeField]
            public float m_BendAmount = 0;

            [SerializeField]
            public bool m_RenderThisWall = true;

            [SerializeField]
            public bool m_EnableThisWall = true;

            [SerializeField]
            public MaterialData m_OverrideData = new MaterialData();

            [SerializeField]
            public bool m_UseOverrideData = false;
        }

        [SerializeField]
        public class RoomData : CeilingData
        {
            public int m_WallNumber;

            [SerializeField]
            public float m_Depth;

            [SerializeField]
            public float m_Height;

            [SerializeField]
            public bool m_HasRoof = true;

            [SerializeField]
            public bool m_HasFloor = true;

            [SerializeField]
            public List<WallData> m_RoomWalls = new List<WallData>();

            public RoomData Clone()
            {
                RoomData other = (RoomData)ReflectionExtensions.DeepCopy<RoomData>(this);
                return other;
            }
        }

        [SerializeField]
        public class HouseBuilderFloorPlanJsonData
        {
            public enum DotTypeEnum
            {
                Finished,
                Editing,
            }

            [NonSerialized]
            public DotTypeEnum m_DotTypeEnum = DotTypeEnum.Editing;

            [SerializeField]
            public bool m_IsWallLooped = false;

            [SerializeField]
            public RoomData m_RoomData = new RoomData();

            [SerializeField]
            public CeilingData m_CeilingData = new CeilingData();

            [SerializeField]
            public FloorData m_FloorData = new FloorData();

            [SerializeField]
            public List<Vector3> m_Positions = new List<Vector3>();

            [SerializeField]
            public int m_WallIndex = 0;

            public HouseBuilderFloorPlanJsonData()
            {
                m_RoomData.m_Depth = MAX_WALL_THICKNESS / 10f;
                m_RoomData.m_Height = 2.5f;
                m_RoomData.m_TilingAmount = 1;
                m_RoomData.m_MaterialName = "";

                m_CeilingData.m_TilingAmount = 1;
                m_CeilingData.m_MaterialName = "";


                m_FloorData.m_TilingAmount = 1;
                m_FloorData.m_MaterialName = "";
            }
        }


        [SerializeField]
        public class FloorCollectionData
        {
            [SerializeField]
            public List<HouseBuilderFloorPlanJsonData> m_StructureJsonData = new List<HouseBuilderFloorPlanJsonData>();
        }


        [SerializeField]
        public class PointLightJsonData
        {
            [SerializeField]
            public float m_Range;
            [SerializeField]
            public float m_Intensity;

            [SerializeField]
            public Color m_Colour = new Color();
        }

        [SerializeField]
        public class SpotLightJsonData : PointLightJsonData
        {
            [SerializeField]
            public float m_Angle;
        }

        [SerializeField]
        public class HouseBuilderContentsJsonData
        {
            [SerializeField]
            public string m_CatalogueEntryGUID;

            [SerializeField]
            public int m_GeneratedGUID = INVALID_GUI;

            [SerializeField]
            public int m_ParentGeneratedGUID = INVALID_GUI;

            [SerializeField]
            public string RuntimeDirectory = null;

            [SerializeField]
            public string m_GameObjectName;
            [SerializeField]
            public Vector3 m_Position;
            [SerializeField]
            public Quaternion m_Rotation;

            [SerializeField]
            public float m_MountToAllRotation = 0;
            [SerializeField]
            public float m_ExtraPivotRotation = 0;


            [SerializeField]
            public WallObjectLinkerData m_WallObjectLinkerData;

            [SerializeField]
            public MaterialData m_MaterialJsonData;

            [SerializeField]
            public ScaleData m_ScaleData;

            [SerializeField]
            public AdaptiveData m_AdaptiveData;

            [SerializeField]
            public ModularData m_ModularData;

            [SerializeField]
            public PointLightJsonData m_PointLightJsonData;

            [SerializeField]
            public SpotLightJsonData m_SpotLightJsonData;

            [SerializeField]
            public AudioData m_AudioData;

            [SerializeField]
            public FileData m_VideoData;

            [SerializeField]
            public LightingData m_LightingData;

            [SerializeField]
            public StagingData m_StagingData;

            [SerializeField]
            public SteamAudioData m_SteamAudioData;
        }

        public enum WallSide
        {
            Inside,
            Outside,
        }

        
        public class WallObjectLinkerData
        {
            [SerializeField]
            public int m_WallIndex;
            [SerializeField]
            public int m_WallPointIndex;
            [SerializeField]
            public float m_PercentageHeight;
            [SerializeField]
            public float m_PercentageLength;
            [SerializeField]
            public WallSide m_WallSide = WallSide.Inside;    
            
            [NonSerialized]
            public List<Renderer> m_RemovedWalls = new List<Renderer>(); 
        }


        [SerializeField]
        public class HouseContentsData
        {
            [SerializeField]
            public List<HouseBuilderContentsJsonData> m_HouseBuilderContentsJsonDataList = new List<HouseBuilderContentsJsonData>();
        }

        public static bool IsAtCreatedStartPoint(Vector3 pos)
        {
            return pos.y < (CREATED_ITEM_START_POINT.y + 10);
        }


        public static void AddInteractions(GameObject obj, CatalogueEntry entry, Interaction move)
        {
            foreach (var item in entry.GetLoadedMetaData())
            {
                // create new data, otherwiese its a ref, and not refer to this object
                var newJsonData = Json.FullSerialiser.WriteToText((MetaData)item, true);
                var newData = Json.FullSerialiser.ReadFromText<MetaData>(newJsonData);

                VrInteraction added = null;
                if (newData.GetType() == typeof(ContentLightMetaData))
                {
                    var light = obj.ForceComponent<VrInteractionLight>();
                    added = light;
                    light.Initialise((ContentLightMetaData)newData, obj);
                }
                else if (newData.GetType() == typeof(ContentPlacementMetaData))
                {
                    var itemDet = (ContentPlacementMetaData)newData;
                    move.SetPlacement(itemDet);
                }
                else if (newData.GetType() == typeof(ContentPivotMetaData))
                {
                    var pivot = obj.ForceComponent<VrInteractionPivot>();
                    added = pivot;
                    pivot.Initialise((ContentPivotMetaData)newData, obj);
                }
                else if (newData.GetType() == typeof(ContentAudioMetaData))
                {
                    var audio = obj.ForceComponent<VrInteractionAudio>();
                    added = audio;
                    audio.Initialise((ContentAudioMetaData)newData, obj);
                }
                else if (newData.GetType() == typeof(ContentSteamAudioMetaData))
                {
                    move.SteamSoundRef.SetupSteamSoundGeometry((ContentSteamAudioMetaData)newData, obj);
                }
                else if (newData.GetType() == typeof(ContentAnimationMetaData))
                {
                    var anim = obj.ForceComponent<VrInteractionAnimation>();
                    added = anim;
                    anim.Initialise((ContentAnimationMetaData)newData, obj);
                }
                else if (newData.GetType() == typeof(ContentVideoPlayerMetaData))
                {
                    var vid = obj.ForceComponent<VrInterationVideoPlayer>();
                    added = vid;
                    vid.Initialise((ContentVideoPlayerMetaData)newData, obj);
                }
                else if (newData.GetType() == typeof(ContentGameObjectCycleMetaData))
                {
                    var cycle = obj.ForceComponent<VrInteractionGameObjectCycle>();
                    added = cycle;
                    cycle.Initialise((ContentGameObjectCycleMetaData)newData, obj);
                }
                else if (newData.GetType() == typeof(ContentAdaptiveMetaData))
                {
                    HouseBuilderConstants.AdaptiveData data = new AdaptiveData();
                    move.CreateAdaptive();
                    move.AdaptiveRef.Setup((InteractionGameObject)move, (ContentAdaptiveMetaData)newData, data);
                }
                else if (newData.GetType() == typeof(ContentModularMetaData))
                {
                    HouseBuilderConstants.ModularData data = new ModularData();
                    move.CreateModular();
                    move.ModularRef.Setup((InteractionGameObject)move, (ContentModularMetaData)newData, data);
                }
                else if (newData.GetType() == typeof(ContentLightingMetaData))
                {
                    move.CreateLighting();
                    move.LightingRef.Setup((InteractionGameObject)move, (ContentLightingMetaData)newData);
                }
                else if (newData.GetType() == typeof(ContentStagingMetaData))
                {
                    HouseBuilderConstants.StagingData data = new StagingData();
                    move.CreateStaging();
                    move.StagingRef.Setup((InteractionGameObject)move, (ContentStagingMetaData)newData, data);
                }
                else if (newData.GetType() == typeof(ContentScriptMetaData))
                {
                    var itemData = (ContentScriptMetaData)newData;
                    itemData.AddItem(obj);
                }

                else if (newData.GetType() == typeof(ContentScaleMetaData))
                {
                    HouseBuilderConstants.ScaleData data = new ScaleData();
                    move.CreateScale();
                    move.ScaleRef.Setup((InteractionGameObject)move, (ContentScaleMetaData)newData, data);
                }
                else if (newData.GetType() == typeof(ContentSliderMetaData))
                {
                    move.CreateVRSliders();
                    move.VRSlidersRef.Setup((InteractionGameObject)move, (ContentSliderMetaData)newData);
                }
                else if (newData.GetType() == typeof(ContentButtonMetaData))
                {
                    move.CreateVRButtons();
                    move.VrButtonsRef.Setup((InteractionGameObject)move, (ContentButtonMetaData)newData);
                }

                if (added != null && move.m_VrInteractions.Contains(added) == false)
                {
                    move.m_VrInteractions.Add(added);
                }
            }
        }



        public static void CreateNewData(CatalogueEntry entry, CatalogueGeneratedData data)
        {
            data.m_JsonData = new HouseBuilderContentsJsonData();
            foreach (var item in data.CatalogueEntryRef.GetLoadedMetaData())
            {
                if (item.GetType() == typeof(ContentLightMetaData))
                {

                }

                else if (item.GetType() == typeof(ContentMaterialChangeMetaData))
                {

                }
                else if (item.GetType() == typeof(ContentScaleMetaData))
                {
                    data.m_JsonData.m_ScaleData = new ScaleData();
                }
                else if (item.GetType() == typeof(ContentAdaptiveMetaData))
                {
                    data.m_JsonData.m_AdaptiveData = new AdaptiveData();
                }
                else if (item.GetType() == typeof(ContentModularMetaData))
                {
                    data.m_JsonData.m_ModularData = new ModularData();
                    var itemDetails = (ContentModularMetaData)item;
                    data.m_JsonData.m_ModularData.m_SelectedChildIndexs.Resize(itemDetails.m_Modular.Count);
                }
                else if (item.GetType() == typeof(ContentAudioMetaData))
                {

                }
            }
        }


        public static Dictionary<Interaction, ContentStagingMetaData> CollectStageData()
        {
            Dictionary<Interaction, ContentStagingMetaData> collectedStagingData = new Dictionary<Interaction, ContentStagingMetaData>();

            List<CatalogueGeneratedData> list = null;
            if (HouseBuilderRoot.Instance != null)
            {
                list = HouseBuilderRoot.Instance.InputContents.ModeContainerRef.GetList;
            }
            else
            {
                list = HouseViewer.HouseViewerRoot.Instance.InputContents.ModeContainerRef.GetList;
            }
            foreach (var item in list)
            {
                if (item.InteractionRef.StagingRef != null)
                {
                    collectedStagingData.Add(item.InteractionRef, item.InteractionRef.StagingRef.ContentStagingMetaDataRef);
                }
            }
            return collectedStagingData;
        }


        public static void WriteMetaData(HouseBuilderContentsJsonData jsonData, CatalogueGeneratedData data)
        {
            Dictionary<Interaction, ContentStagingMetaData> collectedStagingData = CollectStageData();
            

            foreach (var item in data.CatalogueEntryRef.GetLoadedMetaData())
            {
                if (item.GetType() == typeof(ContentLightMetaData))
                {
                    jsonData.m_SpotLightJsonData = new SpotLightJsonData();
                    Light light = data.GeneratedItemRef.GetComponentInChildren<Light>();
                    jsonData.m_SpotLightJsonData.m_Intensity = light.intensity;
                    jsonData.m_SpotLightJsonData.m_Range = light.range;
                    jsonData.m_SpotLightJsonData.m_Angle = light.spotAngle;
                    jsonData.m_SpotLightJsonData.m_Colour = light.color;
                }
                else if (item.GetType() == typeof(ContentMaterialChangeMetaData))
                {
                    jsonData.m_MaterialJsonData = new MaterialData();
                    var itemDetails = (ContentMaterialChangeMetaData)item;
                    foreach (var matItem in itemDetails.m_MaterialItemData)
                    {
                        switch (matItem.m_MaterialType)
                        {
                            case ContentMaterialChangeMetaData.MaterialItemData.MaterialType.Colour:
                                jsonData.m_MaterialJsonData.m_MaterialColour = ContentMaterialChangeMetaData.GetColourStatic(data.GeneratedItemRef, matItem.m_MaterialGameObjectName, matItem.m_MaterialIndex);
                                break;
                            case ContentMaterialChangeMetaData.MaterialItemData.MaterialType.Material:
                                jsonData.m_MaterialJsonData.m_MaterialName = ContentMaterialChangeMetaData.GetMaterialName(data.GeneratedItemRef, matItem.m_MaterialGameObjectName, matItem.m_MaterialIndex);
                                ContentMaterialChangeMetaData.SetMaterial(data.GeneratedItemRef, matItem.m_MaterialGameObjectName, matItem.m_MaterialIndex, jsonData.m_MaterialJsonData.m_MaterialName, null);
                                break;
                            case ContentMaterialChangeMetaData.MaterialItemData.MaterialType.Texture:
                                jsonData.m_MaterialJsonData.m_MaterialName = ContentMaterialChangeMetaData.GetTextureName(data.GeneratedItemRef, matItem.m_MaterialGameObjectName, matItem.m_MaterialIndex, matItem.m_TextureMap);
                                ContentMaterialChangeMetaData.SetTexture(data.GeneratedItemRef, matItem.m_MaterialGameObjectName, matItem.m_MaterialIndex, jsonData.m_MaterialJsonData.m_MaterialName, matItem.m_TextureMap, null);
                                break;
                            default:
                                break;
                        }
                    }
                }
                else if (item.GetType() == typeof(ContentScaleMetaData))
                {
                    if (jsonData.m_ScaleData == null)
                    {
                        jsonData.m_ScaleData = new ScaleData();
                    }
                    jsonData.m_ScaleData.m_Scale = data.InteractionRef.ScaleRef.GetScale();

                }
                else if (item.GetType() == typeof(ContentAdaptiveMetaData))
                {
                    if(jsonData.m_AdaptiveData == null)
                    {
                        jsonData.m_AdaptiveData = new AdaptiveData();
                    }
                    jsonData.m_AdaptiveData.m_MainItemScale[(int)Axis.X] = data.InteractionRef.AdaptiveRef.m_ScaleData[(int)Axis.X];
                    jsonData.m_AdaptiveData.m_MainItemScale[(int)Axis.Y] = data.InteractionRef.AdaptiveRef.m_ScaleData[(int)Axis.Y];
                    jsonData.m_AdaptiveData.m_MainItemScale[(int)Axis.Z] = data.InteractionRef.AdaptiveRef.m_ScaleData[(int)Axis.Z];
                }
                else if (item.GetType() == typeof(ContentAudioMetaData))
                {
                    // nothing to write
                }
                else if (item.GetType() == typeof(ContentVideoPlayerMetaData))
                {
                    if (jsonData.m_VideoData == null)
                    {
                        jsonData.m_VideoData = new FileData();
                    }
                    jsonData.m_VideoData.m_FileName = data.InteractionRef.GetComponent<VrInterationVideoPlayer>().VideoName;
                    // nothing to write
                }
                else if (item.GetType() == typeof(ContentLightingMetaData))
                {
                    if (jsonData.m_LightingData == null)
                    {
                        jsonData.m_LightingData = new LightingData();
                    }

                    jsonData.m_LightingData.m_DmxStartNumber = data.InteractionRef.LightingRef.DmxStartNumber;
                    jsonData.m_LightingData.m_ModeIndex = data.InteractionRef.LightingRef.ModeIndex;
                }
            }

            if(data.InteractionRef.SteamSoundRef != null)
            {
                if(jsonData.m_SteamAudioData == null)
                {
                    jsonData.m_SteamAudioData = new SteamAudioData();
                }
                jsonData.m_SteamAudioData.m_GUIDConnectedTo.Clear();
                foreach (var item in data.InteractionRef.SteamSoundRef.m_InteractionDirectlyConnectedTo)
                {
                    jsonData.m_SteamAudioData.m_GUIDConnectedTo.Add(item.m_RootItemsGeneratedGUID);
                }
            }



            if (data.InteractionRef.StagingRef != null)
            {
                if (jsonData.m_StagingData == null)
                {
                    jsonData.m_StagingData = new StagingData();
                }
                Debug.Log($"another { data.InteractionRef.CatalogueEntryRef.FullName}");
                foreach (var left in data.InteractionRef.StagingRef.ContentStagingMetaDataRef.m_StagingData)
                {
                    if(left.m_ConnectedTo != null)
                    {
                        foreach(var rightBase in collectedStagingData)
                        {
                            foreach(var right in rightBase.Value.m_StagingData)
                            {
                                if (left.m_ConnectedTo == right) // if its found the correct InteractionBase
                                {
                                    if (left.m_ConnectedTo.m_OwnIndex == right.m_OwnIndex)
                                    {
                                        var newdata = new StagingData.Data()
                                        {
                                            m_ConnectedToGUId = rightBase.Key.m_RootItemsGeneratedGUID,
                                            m_ConnectedToPointIndex = right.m_OwnIndex,
                                            m_RotationIndex = right.m_RotationIndex,
                                            m_OwnPointIndex = left.m_OwnIndex,
                                        };

                                        jsonData.m_StagingData.m_Connected.Add(newdata);

                                        Debug.Log($"newdata, left: {data.InteractionRef.CatalogueEntryRef.FullName}, index:{left.m_OwnIndex} guid:{data.InteractionRef.m_RootItemsGeneratedGUID},        right: {rightBase.Key.CatalogueEntryRef.FullName} m_ConnectedToPointIndex {newdata.m_ConnectedToPointIndex}, guid:{rightBase.Key.m_RootItemsGeneratedGUID} ");
                                    }
                                }
                            }
 
                        }
                        
                    }
                }
            }
        }


        public static void CopyData(CatalogueGeneratedData source, CatalogueGeneratedData destination)
        {
            if (destination.m_JsonData == null)
            {
                destination.m_JsonData = new HouseBuilderContentsJsonData();
            }
            foreach (var item in source.CatalogueEntryRef.GetLoadedMetaData())
            {
                if (item.GetType() == typeof(ContentLightMetaData))
                {
                    Light lightSource = source.GeneratedItemRef.GetComponentInChildren<Light>();
                    Light lightdestination = destination.GeneratedItemRef.GetComponentInChildren<Light>();
                    lightdestination.intensity = lightSource.intensity;
                    lightdestination.range = lightSource.range;
                    lightdestination.spotAngle = lightSource.spotAngle;
                    lightdestination.color = lightSource.color;
                }
                else if (item.GetType() == typeof(ContentMaterialChangeMetaData))
                {
                    var itemDetails = (ContentMaterialChangeMetaData)item;
                    foreach (var matData in itemDetails.m_MaterialItemData)
                    {

                        foreach (var matItem in itemDetails.m_MaterialItemData)
                        {
                            switch (matItem.m_MaterialType)
                            {
                                case ContentMaterialChangeMetaData.MaterialItemData.MaterialType.Colour:
                                    Color col = ContentMaterialChangeMetaData.GetColourStatic(source.GeneratedItemRef, matItem.m_MaterialGameObjectName, matItem.m_MaterialIndex);
                                    ContentMaterialChangeMetaData.SetColourStatic(destination.GeneratedItemRef, matItem.m_MaterialGameObjectName, matItem.m_MaterialIndex, col);
                                    break;
                                case ContentMaterialChangeMetaData.MaterialItemData.MaterialType.Material:
                                    string materialName = ContentMaterialChangeMetaData.GetMaterialName(source.GeneratedItemRef, matItem.m_MaterialGameObjectName, matItem.m_MaterialIndex);
                                    ContentMaterialChangeMetaData.SetMaterial(destination.GeneratedItemRef, matItem.m_MaterialGameObjectName, matItem.m_MaterialIndex, materialName, null);
                                    break;
                                case ContentMaterialChangeMetaData.MaterialItemData.MaterialType.Texture:
                                    string textureName = ContentMaterialChangeMetaData.GetTextureName(source.GeneratedItemRef, matItem.m_MaterialGameObjectName, matItem.m_MaterialIndex, matItem.m_TextureMap);
                                    ContentMaterialChangeMetaData.SetTexture(destination.GeneratedItemRef, matItem.m_MaterialGameObjectName, matItem.m_MaterialIndex, textureName, matItem.m_TextureMap, null);
                                    break;
                                default:
                                    break;
                            }
                        }
                    }

                }
                else if (item.GetType() == typeof(ContentScaleMetaData))
                {
                    if (destination.m_JsonData.m_ScaleData == null)
                    {
                        destination.m_JsonData.m_ScaleData = new ScaleData();
                    }
                    destination.m_JsonData.m_ScaleData.m_Scale = source.m_JsonData.m_ScaleData.m_Scale;
                    var itemDetails = (ContentScaleMetaData)item;
                    var inter = destination.GeneratedItemRef.GetComponent<InteractionGameObject>();
                    inter.CreateScale();
                    inter.ScaleRef.Setup(inter, itemDetails, destination.m_JsonData.m_ScaleData);
                }
                else if (item.GetType() == typeof(ContentAdaptiveMetaData))
                {
                    if (destination.m_JsonData.m_AdaptiveData == null)
                    {
                        destination.m_JsonData.m_AdaptiveData = new AdaptiveData();
                    }
                    destination.m_JsonData.m_AdaptiveData.m_MainItemScale = source.m_JsonData.m_AdaptiveData.m_MainItemScale;
                    var itemDetails = (ContentAdaptiveMetaData)item;
                    var inter = destination.GeneratedItemRef.GetComponent<InteractionGameObject>();
                    inter.CreateAdaptive();
                    inter.AdaptiveRef.Setup(inter, itemDetails, destination.m_JsonData.m_AdaptiveData);
                }
                else if (item.GetType() == typeof(ContentModularMetaData))
                {
                    if (destination.m_JsonData.m_ModularData == null)
                    {
                        destination.m_JsonData.m_ModularData = new ModularData();
                    }
                    var itemDetails = (ContentModularMetaData)item;
                    var inter = destination.GeneratedItemRef.GetComponent<InteractionGameObject>();
                    inter.ModularRef.Setup(inter, itemDetails, destination.m_JsonData.m_ModularData);
                }
                else if (item.GetType() == typeof(ContentAudioMetaData))
                {
                    //jsonData.m_AudioData = new AudioGroupData();
                    //jsonData.m_AudioData.m_FileName = ((ContentAudioMetaData)item).m_AudioClip.ToString();
                }
                else if (item.GetType() == typeof(ContentLightingMetaData))
                {
                    destination.InteractionRef.LightingRef.DmxStartNumber = source.InteractionRef.LightingRef.DmxStartNumber;
                    destination.InteractionRef.LightingRef.ModeIndex = source.InteractionRef.LightingRef.ModeIndex;
                }
            }
        }



        public static void ReadMetaData(InteractionGameObjectMove move, CatalogueEntry entry, HouseBuilderContentsJsonData jsonData, GameObject obj, CatalogueGeneratedData data)
        {
            foreach (var item in entry.GetLoadedMetaData())
            {
                if (item.GetType() == typeof(ContentLightMetaData))
                {
                    if (null != jsonData.m_SpotLightJsonData)
                    {
                        Light light = obj.GetComponentInChildren<Light>();
                        light.intensity = jsonData.m_SpotLightJsonData.m_Intensity;
                        light.range = jsonData.m_SpotLightJsonData.m_Range;
                        light.spotAngle = jsonData.m_SpotLightJsonData.m_Angle;
                        light.color = jsonData.m_SpotLightJsonData.m_Colour;
                    }
                }
                else if (item.GetType() == typeof(ContentMaterialChangeMetaData))
                {
                    if (null != jsonData.m_MaterialJsonData)
                    {
                        var itemDetails = (ContentMaterialChangeMetaData)item;
                        foreach (var matData in itemDetails.m_MaterialItemData)
                        {
                            switch (matData.m_MaterialType)
                            {
                                case ContentMaterialChangeMetaData.MaterialItemData.MaterialType.Colour:
                                    ContentMaterialChangeMetaData.SetColourStatic(obj, matData.m_MaterialGameObjectName, matData.m_MaterialIndex, jsonData.m_MaterialJsonData.m_MaterialColour);
                                    break;
                                case ContentMaterialChangeMetaData.MaterialItemData.MaterialType.Material:
                                    ContentMaterialChangeMetaData.SetMaterial(obj, matData.m_MaterialGameObjectName, matData.m_MaterialIndex, jsonData.m_MaterialJsonData.m_MaterialName, null);
                                    break;
                                case ContentMaterialChangeMetaData.MaterialItemData.MaterialType.Texture:
                                    ContentMaterialChangeMetaData.SetTexture(obj, matData.m_MaterialGameObjectName, matData.m_MaterialIndex, jsonData.m_MaterialJsonData.m_MaterialName, matData.m_TextureMap, null);
                                    break;
                                default:
                                    break;
                            }
                            
                        }

                    }
                }
                else if (item.GetType() == typeof(ContentScaleMetaData))
                {
                    if (null != jsonData.m_ScaleData)
                    {
                        var itemDetails = (ContentScaleMetaData)item;
                        var inter = obj.GetComponent<InteractionGameObject>();
                        move.CreateScale();
                        move.ScaleRef.Setup(move, itemDetails, jsonData.m_ScaleData);
                    }
                }
                else if (item.GetType() == typeof(ContentAdaptiveMetaData))
                {
                    if (null != jsonData.m_AdaptiveData)
                    {
                        var itemDetails = (ContentAdaptiveMetaData)item;
                        var inter = obj.GetComponent<InteractionGameObject>();
                        move.CreateAdaptive();
                        move.AdaptiveRef.Setup(move, itemDetails, jsonData.m_AdaptiveData);
                    }
                }
                else if (item.GetType() == typeof(ContentModularMetaData))
                {
                    if (null != jsonData.m_ModularData)
                    {
                        var itemDetails = (ContentModularMetaData)item;
                        var inter = obj.GetComponent<InteractionGameObject>();
                        move.CreateModular();
                        move.ModularRef.Setup(move, itemDetails, jsonData.m_ModularData);
                    }
                }
                else if (item.GetType() == typeof(ContentAudioMetaData))
                {
                    // nothing extra to read
                }
                else if (item.GetType() == typeof(ContentLightingMetaData))
                {
                    if (jsonData.m_LightingData != null)
                    {
                        data.InteractionRef.LightingRef.DmxStartNumber = jsonData.m_LightingData.m_DmxStartNumber;
                        data.InteractionRef.LightingRef.ModeIndex = jsonData.m_LightingData.m_ModeIndex;
                    }
                }
                else if (item.GetType() == typeof(ContentVideoPlayerMetaData))
                {
                    var vid = obj.GetComponent<VrInterationVideoPlayer>();
                    if(vid != null)
                    {
                        if (jsonData.m_VideoData != null)
                        {
                            if (string.IsNullOrEmpty(jsonData.m_VideoData.m_FileName) == false)
                            {
                                vid.VideoName = jsonData.m_VideoData.m_FileName;
                            }
                        }
                        else
                        {
                            jsonData.m_VideoData = new FileData();
                        }
                    }
                }
                else if (item.GetType() == typeof(ContentStagingMetaData))
                {
                    var itemDetails = (ContentStagingMetaData)item;
                    move.CreateStaging();
                    move.StagingRef.Setup(move, itemDetails, jsonData.m_StagingData);
                }
            }
        }


        public static float GetTilingAmountFromTextureName(string name)
        {
            var split = name.Split('_').ToList();
            string toParse = split.Last();
            if (toParse.StartsWith(TEXTURE_SCALE, System.StringComparison.CurrentCultureIgnoreCase) == true)
            {
                string numberString = toParse.GetNumberString();
                if (true == float.TryParse(numberString, out float amount))
                {
                    return amount;
                }
            }
            return 1;
        }


        public static GameObject GetValidGameObject(GameObject testObject) => GetValidInteraction(testObject);

        public static GameObject GetValidInteraction(GameObject testObject)
        {
            var baseObject = testObject;
            // find the correct base gameobject 
            var inter = baseObject.GetComponent<Interaction>();
            if (null == inter)
            {
                inter = baseObject.GetComponentInParent<Interaction>();
            }
            if(inter != null)
            {
                return inter.gameObject;
            }
            return null;
        }


    }
}
