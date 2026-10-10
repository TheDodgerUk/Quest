using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

namespace HouseBuilder
{
    public class FloorAndSkyboxManager : MonoBehaviour
    {
        public static FloorAndSkyboxManager Instance;

        private List<GameObject> m_ListOfFloors = new List<GameObject>();
        private GameObject m_CurrentSelectedFloor;
        private int m_CurrentSelectedFloorIndex = 0;
        private GameObject m_SkyBox;
        private GameObject m_StartDirectionLight;
        private Sky m_Sky;
        private CompassUI m_CompassUI;

        private ParticleSystem m_ParticleSystem;
        private const float PARTICLES_PER_SQUARE_METRE = 2;

        private const int MAX_NUMBER_OF_PARTICLES = 10000;
        private const int DOME_DIMENSION = 50000;


        private List<ReflectionProbe> m_ReflectionProbeList = new List<ReflectionProbe>();
        private Color m_AmbientLight = Color.white;
        private bool m_UseAmbientLight = true;

        private void Awake()
        {
            Instance = this;

            var lights = FindObjectsOfType<Light>();
            foreach (var light in lights)
            {
                if (light.name == "Directional Light")
                {
                    m_StartDirectionLight = light.gameObject;
                }
            }

            FindAllTheFloors();

            m_ParticleSystem = this.transform.GetComponentInChildren<ParticleSystem>();
            var main = m_ParticleSystem.main;
            main.maxParticles = MAX_NUMBER_OF_PARTICLES;
            m_SkyBox = Instantiate(Resources.Load<GameObject>("Sky Dome (Mobile)"));
            m_Sky = m_SkyBox.GetComponent<Sky>();
            m_SkyBox.transform.SetParent(this.transform);
            m_SkyBox.transform.ClearLocals();
            m_SkyBox.transform.localScale = new Vector3(DOME_DIMENSION, DOME_DIMENSION, DOME_DIMENSION);

            //The sun rises due east on only two days of the year, the equinoxes, near March 21 and September 22 each year. Every other day of the year it rises somewhere else
            // set Juialn Date to 80 (March 21), to double check
            m_SkyBox.transform.rotation = Quaternion.Euler(0, 90, 0); // to compinsate for the placement of the grid in system, so that forward on floor is north

            var compassButton = this.transform.SearchComponent<Button>("Compass");
            m_CompassUI = compassButton.AddComponent<CompassUI>();
            m_CompassUI.Initialise(m_SkyBox);

            var rect = compassButton.GetComponent<RectTransform>();
            rect.pivot = new Vector2(0.514f, 0.498f);
            Core.Environment.OnEnvironmentLoadingComplete += OnEnvironmentLoadingComplete;
        }

        private void OnEnvironmentLoadingComplete()
        {
            Core.Environment.OnEnvironmentLoadingComplete -= OnEnvironmentLoadingComplete;
            ToggleSkybox(true);
        }

        private void FindAllTheFloors()
        {
            var floorsGroup = this.transform.Find("Floors");
            foreach (Transform tr in floorsGroup)
            {
                m_ListOfFloors.Add(tr.gameObject);
                tr.gameObject.ForceComponent<InteractionFloorPlan>();
                var r = tr.gameObject.ForceComponent<Rigidbody>();
                r.isKinematic = true;
                r.useGravity = false;
                tr.transform.localScale = new Vector3(1.001f, 1, 1.001f); // slight scale increase this is so the dots are not on the line
                var m = tr.gameObject.ForceComponent<MeshCollider>();
                m.convex = false;
            }
            m_CurrentSelectedFloor = m_ListOfFloors[0];
            if (HouseBuilderRoot.Instance == null)
            {
                foreach (var floor in m_ListOfFloors)
                {
                    floor.layer = Layers.TeleportLayer;
                }
            }

            ReflectionProbe reflection = this.AddComponent<ReflectionProbe>();
            reflection.mode = UnityEngine.Rendering.ReflectionProbeMode.Realtime;
            reflection.refreshMode = UnityEngine.Rendering.ReflectionProbeRefreshMode.ViaScripting;
            reflection.transform.position = Vector3.zero;
            reflection.intensity = 0.1f;    // it was far to harsh before 
            m_ReflectionProbeList.Add(reflection);
        }

        private void ToggleSkybox(bool state)
        {
            m_SkyBox.SetActive(state);
            m_StartDirectionLight.SetActive(!state);
        }

        public void SelectTheFloor(int index)
        {
            m_CurrentSelectedFloorIndex = index;
            m_ListOfFloors.ForEach(e => e.SetActive(false));

            m_ListOfFloors[index].SetActive(true);
            m_CurrentSelectedFloor = m_ListOfFloors[index];
            m_ListOfFloors.Last().SetActive(true); // this is the invisible one

            if (PositionGrid.Instance != null)
            {
                PositionGrid.Instance.SetWidth(m_CurrentSelectedFloor);
                var width = PositionGrid.GetWidthFromGameObject(m_CurrentSelectedFloor);
                m_ReflectionProbeList.ForEach(e => e.size = new Vector3(width, width, width));
            }
        }

        public void SetTimeOfDay(int time) => m_Sky.Cycle.TimeOfDay = time;

        public int GetCurrentFloorIndex() => m_CurrentSelectedFloorIndex;

        public void ToggleParticleSystems(bool state)
        {
            if (state)
            {
                var gridSize = PositionGrid.GetWidthFromGameObject(m_CurrentSelectedFloor);
                var shape = m_ParticleSystem.shape;
                Vector3 size = new Vector3(gridSize, 2f, gridSize);
                shape.scale = size;
                var em = m_ParticleSystem.emission;
                em.rateOverTime = (gridSize * gridSize) * PARTICLES_PER_SQUARE_METRE;
                m_ParticleSystem.Play();
            }
            else
            {
                m_ParticleSystem.Stop();
            }
        }

        public void ToggleCompass(bool state) => m_CompassUI.SetActive(state);

        public Color GetAmbientLight() => m_AmbientLight;

        public void SetAmbientLight(Color colour)
        {
            m_AmbientLight = colour;
            if(m_UseAmbientLight == true)
            {
                RenderSettings.ambientLight = colour;
            }
            else
            {
                RenderSettings.ambientLight = Color.white;
            }
            
        }

        public void SetUseAmbientLight(bool state)
        {
            m_UseAmbientLight = state;
            SetAmbientLight(m_AmbientLight);
        }

        public float GetCompassRotation() => m_CompassUI.m_Rotation;

        public void SetCompassRotation(float rot) => m_CompassUI.SetCompass(rot);

        [InspectorButton]
        public void UpdateReflectionProbe()
        {
            foreach(var item in m_ReflectionProbeList)
            {
                if(item.isActiveAndEnabled == true)
                {
                    item.RenderProbe();
                }
            }
        }

    }
}