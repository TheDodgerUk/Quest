using HouseBuilder;
using System;
using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.UI.Extensions;
using static OscController.DataContainer.OscAllUsedFixtures;
using static OscController.OscControllerRoot;

namespace OscController
{
    public class FixtureWebInfoPanel : MonoBehaviour
    {
#if WEB_BROWSER
        private SimpleWebBrowser.WebBrowser2D m_WebBrowser2D;
#endif
        private Button m_Manual;
        private Button m_Other;
        private Button m_ProductPage;
        private Button m_Video;

        private int m_ManualIndex = 0;
        private int m_OtherIndex = 0;
        private int m_ProductPageIndex = 0;
        private int m_VideoIndex = 0;
        private Fixtures.Fixture m_Fixture;
        private Action m_OnClose;

        public void Initialise()
        {
            this.transform.FindAndFixMasks();
#if WEB_BROWSER
            var browser = this.transform.SearchComponent<RectTransform>("Browser");
            m_WebBrowser2D = browser.gameObject.ForceComponent<SimpleWebBrowser.WebBrowser2D>();

            m_Manual = m_WebBrowser2D.transform.SearchComponent<Button>("Manual");
            m_Other = m_WebBrowser2D.transform.SearchComponent<Button>("Other");
            m_ProductPage = m_WebBrowser2D.transform.SearchComponent<Button>("ProductPage");
            m_Video = m_WebBrowser2D.transform.SearchComponent<Button>("Video");


            Button exit = m_WebBrowser2D.transform.SearchComponent<Button>("Exit");
            exit.onClick.AddListener(() =>
            {
                m_WebBrowser2D.transform.parent.SetActive(false);
                m_OnClose?.Invoke();
            });

            m_WebBrowser2D.AutoFitParent = true;
            m_WebBrowser2D.Navigate("https://blog.mozvr.com/a-browser-plugin-for-unity/");
            m_WebBrowser2D.transform.parent.SetActive(false);
            m_WebBrowser2D.OnPageLoaded(() =>
            {
                Debug.LogError("FixtureWebInfoPanel  OnPageLoaded");
            });
#endif
            this.transform.SetActive(false);
        }

        public void SetFixtureDetails(Fixtures.Fixture fixture, Action onClose)
        {
#if WEB_BROWSER
            if(m_Fixture == fixture)
            {
                m_Fixture = null;

                m_WebBrowser2D.transform.parent.SetActive(false);

                m_OnClose?.Invoke();
                m_OnClose = null;
                return;
            }
            m_OnClose?.Invoke();
            m_OnClose = onClose;
            m_Fixture = fixture;
            m_Manual.interactable = (fixture.Links.Manual != null && fixture.Links.Manual.Count > 0);
            m_Other.interactable = (fixture.Links.Other != null && fixture.Links.Other.Count > 0);
            m_ProductPage.interactable = (fixture.Links.ProductPage != null && fixture.Links.ProductPage.Count > 0);
            m_Video.interactable = (fixture.Links.Video != null && fixture.Links.Video.Count > 0);


            if(m_Manual.interactable == true)
            {
                NextManual();
            }
            else if (m_Other.interactable == true)
            {
                NextOther();
            }
            else if (m_ProductPage.interactable == true)
            {
                NextProductPage();
            }
            else if (m_Video.interactable == true)
            {
                NextVideo();
            }
            m_Manual.onClick.RemoveAllListeners();
            m_Manual.onClick.AddListener(NextManual);

            m_Other.onClick.RemoveAllListeners();
            m_Other.onClick.AddListener(NextOther);

            m_ProductPage.onClick.RemoveAllListeners();
            m_ProductPage.onClick.AddListener(NextProductPage);

            m_Video.onClick.RemoveAllListeners();
            m_Video.onClick.AddListener(NextVideo);
#else
            Debug.LogError("SetFixtureDetails has not been blanked out");
#endif
        }
#if WEB_BROWSER
        private void NextManual()
        {

            m_WebBrowser2D.transform.parent.SetActive(true);
            m_WebBrowser2D.Navigate(m_Fixture.Links.Manual[m_ManualIndex].AbsoluteUri);
            m_ManualIndex.WrapIncrement(m_Fixture.Links.Manual);

        }

        private void NextOther()
        {

            m_WebBrowser2D.transform.parent.SetActive(true);
            m_WebBrowser2D.Navigate(m_Fixture.Links.Other[m_OtherIndex].AbsoluteUri);
            m_OtherIndex.WrapIncrement(m_Fixture.Links.Other);
        }

        private void NextProductPage()
        {
            m_WebBrowser2D.transform.parent.SetActive(true);
            m_WebBrowser2D.Navigate(m_Fixture.Links.ProductPage[m_ProductPageIndex].AbsoluteUri);
            m_ProductPageIndex.WrapIncrement(m_Fixture.Links.ProductPage);
        }

        private void NextVideo()
        {
            m_WebBrowser2D.transform.parent.SetActive(true);
            m_WebBrowser2D.Navigate(m_Fixture.Links.Video[m_VideoIndex].AbsoluteUri);
            m_VideoIndex.WrapIncrement(m_Fixture.Links.Video);
        }
#endif

    }
}
