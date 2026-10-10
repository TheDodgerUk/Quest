using System;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

namespace HouseBuilder
{

    public class CatalogueMenuVideo : CatalogueMenuMaterial
    {
        protected override string Prefix => Texture2DAssetBundle.THUMBNAIL_VIDEO_PREFIX;
    }


    public class CatalogueMenuMaterial : CatalogueMenuObject
    {
        protected virtual string Prefix => Texture2DAssetBundle.THUMBNAIL_MATERIAL_PREFIX;

        public void Initialise(string materialName, string group, string filter, Action onClick)
        {

            m_FilterList = new List<string>() { group + "/" + filter };
            m_Guid = "guidplaceholder?";
            m_ShortName = materialName;
            gameObject.name = m_ShortName;
            m_Button = GetComponent<Button>();
            m_Button.onClick.AddListener(() => onClick());
            m_Icon = transform.Find("Icon").GetComponent<Image>();

            // using Texture for icons, a
            Core.AssetBundlesRef.Texture2DAssetBundleRef.GetItem(Core.Mono, $"{Prefix}{materialName}", (texture) =>
            {
                int width = 1024;
                int height = 1024;

                if (texture != null)
                {
                    width = texture.width;
                    height = texture.height;
                }
                else
                {
                    Debug.LogError($"Could not find  {Prefix}{materialName} in Texture2DAssetBundle");
                }

                Sprite sp = Sprite.Create((Texture2D)texture, new Rect(0, 0, width, height), Vector2.zero);
                m_Icon.sprite = sp;
            });

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