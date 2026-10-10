using System;
using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

namespace HouseViewer
{
    public class UserSelectionMenu : MonoBehaviour
    {
        public static UserSelectionMenu Instance;

        private TMP_InputField m_NewUserNameInputField;
        private TMP_InputField m_NewPassword1InputField;
        private Button m_Login;
        private Button m_Error;

        private AudioClip m_ButtonPressClip;
        private AudioSource m_ButtonPressSource;
        private Action m_LoggedInCallBack;

        public bool HasBeenInitilised { get; private set; }

        public void Initialise()
        {
            HasBeenInitilised = false;
            Instance = this;

            Core.AssetBundlesRef.AudioClipAssetBundleRef.GetItem(null, "ButtonPress", (clip) =>
            {
                m_ButtonPressSource = this.gameObject.ForceComponent<AudioSource>();
                m_ButtonPressSource.clip = clip;
                Debug.LogError("UserSelectionMenu");
            });
            gameObject.ForceComponent<VRUICanvas>();

            m_NewUserNameInputField = this.transform.Search("ExistingUserNameInputField").GetComponent<TMP_InputField>();
            m_NewUserNameInputField.onSelect.AddListener(UserName);

            m_NewPassword1InputField = this.transform.Search("ExistingPassword1InputField").GetComponent<TMP_InputField>();
            m_NewPassword1InputField.onSelect.AddListener(Password);

            m_Login = this.transform.Search("BtnLogin").GetComponent<Button>();
            m_Login.onClick.AddListener(Login);


            this.transform.Search("orLine").SetActive(false);
            this.transform.Search("BtnNew").SetActive(false);

            m_Error = this.transform.Search("BtnError").GetComponent<Button>();
            m_Error.onClick.AddListener(RemoveError);
            m_Error.SetActive(false);

            if (Core.Network.Users.CurrentUser != null)
            {
                m_NewUserNameInputField.text = Core.Network.Users.CurrentUser.m_UserName;
            }
            else
            {
                m_NewUserNameInputField.text = "";
            }

            if (Core.Network.Users.CurrentUser != null)
            {
                m_NewPassword1InputField.text = Core.Network.Users.CurrentUser.m_Password;
            }
            else
            {
                m_NewPassword1InputField.text = "";
            }


            Core.Environment.OnEnvironmentLoadingComplete += OnEnvironmentLoadingComplete;
        }

        private void RemoveError()
        {
            m_Error.SetActive(false);
        }

        private void OnEnvironmentLoadingComplete()
        {
            Core.Environment.OnEnvironmentLoadingComplete -= OnEnvironmentLoadingComplete;
#if VR_INTERACTION
            Core.Keyboard.CreateKeyBoard(() =>
            {
                Core.Keyboard.SetStringCallback(m_NewUserNameInputField.text, (arg) =>
                {
                    m_NewUserNameInputField.text = arg;
                });

                Core.Keyboard.SetTransform(Vector3.zero, Quaternion.identity);
                this.gameObject.transform.position = new Vector3(0, 0.8f, 0);
                this.gameObject.transform.rotation = Quaternion.identity;

                Core.Keyboard.SetParent(this.gameObject);
                Core.Mono.WaitFor(1, () => 
                {
                    this.gameObject.SetActive(false);
                    HasBeenInitilised = true;
                });
 
            }, initialToggle: false);
#endif
        }

        private void UserName(string argIn)
        {
            m_ButtonPressSource.Play();
#if VR_INTERACTION
            Core.Keyboard.SetStringCallback(m_NewUserNameInputField.text, (arg) =>
            {
                m_NewUserNameInputField.text = arg;
            });
#endif
        }

        private void Password(string argIn)
        {
            m_ButtonPressSource.Play();
#if VR_INTERACTION
            Core.Keyboard.SetStringCallback(m_NewPassword1InputField.text,(arg) =>
            {
                m_NewPassword1InputField.text = arg;
            });
#endif
        }

        private void Login()
        {
            m_ButtonPressSource.Play();
            NetworkingUsers.NetworkingUser newUser = new NetworkingUsers.NetworkingUser();
            newUser.m_UserName = m_NewUserNameInputField.text;
            newUser.m_Password = m_NewPassword1InputField.text;

            if (Core.Network.Users.IsValidLoginDetails(newUser) == true)
            {
                Core.Network.Users.SetCurrentUser(newUser);
                HouseViewerRoot.Instance.GetSaveFiles();
                FileLoaderMenu.Instance.gameObject.SetActive(true);
                FileLoaderMenu.Instance.gameObject.transform.position = this.transform.position;
                FileLoaderMenu.Instance.gameObject.transform.rotation = this.transform.rotation;
                this.gameObject.SetActive(false);
                m_LoggedInCallBack?.Invoke();
            }
            else
            {
                m_Error.SetActive(true);
            }
        }

        public void AssignLoggedInCallback(Action callback)
        {
            m_LoggedInCallBack = callback;
        }

#if VR_INTERACTION
        private void OnEnable()
        {
            Core.Keyboard.Show(true);
        }


        private void OnDisable()
        {
            Core.Keyboard.Show(false);
        }
#endif
    }

}