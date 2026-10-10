using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

namespace HouseBuilder
{
    public class MaterialSelectionGUI : MonoBehaviour
    {
        public static MaterialSelectionGUI Instance;
        private Canvas m_Canvas;
        private Image m_Sprite;
        private bool m_HasCatElementBeenSelected = false;


        private const float WIDGET_OFFSET = 30f;

        private Dictionary<string, Image> m_ListOfCataItem = new Dictionary<string, Image>();

        public void Initialise(List<CatalogueMenuObject> itemList, Canvas canvas)
        {
            Instance = this;
            m_Canvas = canvas;
            m_Sprite = this.transform.Find("Icon").GetComponent<Image>();
            foreach (var item in itemList)
            {
                if (m_ListOfCataItem.ContainsKey(item.name) == false)
                {
                    m_ListOfCataItem.Add(item.name, item.transform.Find("Icon").GetComponent<Image>());
                }
                else
                {
                    Debug.LogError($"key has been added before key: {item.name}", item);
                }
            }
            EnableSelectionGui(false);
        }
        void Update()
        {
            if (m_HasCatElementBeenSelected)
            {
                SetImagePosition();
                if (HouseBuilderConstants.IsKeyboardCommand(KeyCommand.Cancel) == true)
                {
                    ForceClearSelectedMaterial();
                }
            }
        }

        private void SetImage(string sprite)
        {
            if (m_ListOfCataItem.ContainsKey(sprite) == true)
            {
                var image = m_ListOfCataItem[sprite].sprite;
                m_Sprite.sprite = image;
            }
            else
            {
                Debug.LogError($"Cannot find sprite called: {sprite}");
            }
        }

        public void SetImagePosition()
        {
            Vector2 pos;
            RectTransformUtility.ScreenPointToLocalPointInRectangle(m_Canvas.transform as RectTransform, Input.mousePosition, m_Canvas.worldCamera, out pos);
            pos.x += WIDGET_OFFSET;
            pos.y += WIDGET_OFFSET;
            transform.position = m_Canvas.transform.TransformPoint(pos);
        }
        public void EnableSelectionGui(bool state, string texturename = null)
        {

            m_HasCatElementBeenSelected = state;
            this.gameObject.SetActive(state);

            if (texturename != null && state)
                SetImage(texturename);
            else if(texturename == null && !state )
                m_Sprite.sprite = null;
        }

        public bool IsSelectionGuiEnabled()
        {
            return m_HasCatElementBeenSelected;
        }

        public void ForceClearSelectedMaterial() //This should no go here!! TO BE IMPROVED 
        {
            EnableSelectionGui(false);
            HBGUIManager.Instance.WallSettingsMenuRef.ClearNonItemMaterial();
            HBGUIManager.Instance.CeilingSettingsMenuRef.ClearNonItemMaterial();
            HBGUIManager.Instance.FloorSettingsMenuRef.ClearNonItemMaterial();
        }
    }
}
