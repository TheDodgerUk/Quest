using System;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

namespace HouseBuilder
{


    public class CatalogueMenuTexture : CatalogueMenuObject
    {
        public virtual void Initialise(string textureName, string group, string filter, Action onClick)
        {
            Core.AssetBundlesRef.Texture2DAssetBundleRef.GetItem(Core.Mono, textureName, (texture) =>
            {
                Initialise(textureName, group, filter, texture, onClick);
            });

        }

        public void Initialise(string textureName, string group, string filter, Texture texture, Action onClick)
        {
            m_FilterList = new List<string>() { group + "/" + filter };
            m_Guid = "guidplaceholder?";
            m_ShortName = textureName;
            gameObject.name = m_ShortName;
            m_Button = GetComponent<Button>();
            m_Button.onClick.AddListener(() => onClick());
            m_Icon = transform.Find("Icon").GetComponent<Image>();


            int width = 1024;
            int height = 1024;

            if (texture != null)
            {
                width = texture.width;
                height = texture.height;
            }
            try
            {
                Sprite sp = Sprite.Create((Texture2D)texture, new Rect(0, 0, width, height), Vector2.zero);
                m_Icon.sprite = sp;
            }
            catch (Exception e)
            {
                Debug.LogError($"Texture name {textureName}  generated an error {e.Message}");
            }


            transform.Find("Icon/Colourable").gameObject.SetActive(false);
            transform.Find("Icon/Modular").gameObject.SetActive(false);

            m_Thumbnail = transform.Find("Label").gameObject;
            m_SelectedBorder = transform.Find("SelectedBorder").GetComponent<Image>();
            TextMeshProUGUI thumbnailLabel = transform.Find("Label/Text").GetComponent<TextMeshProUGUI>();

            m_SelectedBorder.color = NORMAL;

            AssignMaterialNameTolabel(thumbnailLabel);
            ToggleThumbnail(false);
        }
    }
}


