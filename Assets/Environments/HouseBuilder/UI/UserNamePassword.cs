using System;
using System.Collections.Generic;
using System.Net.Mail;
using TMPro;
using UnityEngine;
using UnityEngine.UI;
using static NetworkingUsers;

namespace HouseBuilder
{
    public class UserNamePassword : MonoBehaviour
    {
        public static UserNamePassword Instance;
        private Toggle m_MenuToggle;
        //After few attempts I realised that Unity TMP_InputField already does password encryption, so I tried to 
        //create a function fot all the three password input fieds, instead of creating three different functions.
        //To do that, I created this dictionary that makes me link the toggles to the correspective TMP_InputField.
        //When a toggle is pressed I pass the name of the toggle as key to get the right TMP_InputField and change its parameter
        private Dictionary<string, TMP_InputField> m_PasswordInputFieldsList = new Dictionary<string, TMP_InputField>();
        
       
        //New User View Components
        private CanvasGroup m_CanvasGroup;
        private GameObject m_NewUserView;
        private TMP_InputField m_NewUserNameInputField;
        private TMP_InputField m_NewPassword1InputField;
        private TMP_InputField m_NewPassword2InputField;
        private TMP_InputField m_NewEmailInputField;
        private Button m_CreateBtn;                      
        private Button m_BackBtn;


        //Log in View Components
        private GameObject m_LogInView;
        private Transform m_LowLogInComponents;
        private TMP_InputField m_UserNameInputField;
        private TMP_InputField m_UserPasswordInputField;

        private Button m_LogInBtn;
        private Button m_LogOutBtn;
        private Button m_NewUserBtn;
        private Button m_CloseBtn;
        private Button m_ExampleBtn;

        private NetworkingUser m_NetworkingUser;

        public bool IsVisible { get; protected set; }

        private List<TextMeshProUGUI> m_ListOfText = new List<TextMeshProUGUI>();

        private LoadSaveMenuManager m_LoadSaveMenuManager;
        public void Initialise(Toggle toggle, LoadSaveMenuManager loadSaveMenuManager)
        {
            Instance = this;
            this.SetActive(true);
            m_MenuToggle = toggle;
            m_LoadSaveMenuManager = loadSaveMenuManager;
            m_CanvasGroup = this.GetComponent<CanvasGroup>();
            SetUpNewUserView();
            SetUpLogInView();
            HBGUIManager.Instance?.SetUserName();
            ShowLogInView(true);
            Toggle(false);
        }

        private void SetUpNewUserView()
        {
            m_NewUserView = this.transform.Find("NewUserMenu").gameObject;

            m_BackBtn = m_NewUserView.transform.Find("BackButton").GetComponent<Button>();
            m_BackBtn.onClick.AddListener(() => { ShowLogInView(true); });

            m_NewUserNameInputField = this.transform.Search("NewUserNameInputField").GetComponent<TMP_InputField>();
            m_NewUserNameInputField.onValueChanged.AddListener((newText) =>
            {
                m_NewUserNameInputField.text = UserNameValidation(newText);
            });

            var newPassword1InputField = this.transform.Search("NewPassword1InputField"); //Cashing the transform since will be called more than once
            var toggleFirstPassword = newPassword1InputField.GetComponentInChildren<Toggle>(); //Finding the toggle of the input field
            m_NewPassword1InputField = newPassword1InputField.GetComponent<TMP_InputField>();  //Cashing the input field
            m_PasswordInputFieldsList.Add(toggleFirstPassword.name, m_NewPassword1InputField);  //Linking the toggle name to the Input field
            toggleFirstPassword.onValueChanged.AddListener((value) => ShowHidePassword(value, toggleFirstPassword.name)); //Link the function to the toggle state change
            m_NewPassword1InputField.onValueChanged.AddListener((newText) =>
            {
                m_NewPassword1InputField.text = PasswordValidation(newText);
            });

            var newPassword2InputField = this.transform.Search("NewPassword2InputField");
            m_NewPassword2InputField = newPassword2InputField.GetComponent<TMP_InputField>();
            var toggleSecondPassword = newPassword2InputField.GetComponentInChildren<Toggle>();
            m_PasswordInputFieldsList.Add(toggleSecondPassword.name, m_NewPassword2InputField);
            toggleSecondPassword.onValueChanged.AddListener((value) => ShowHidePassword(value, toggleSecondPassword.name));
            m_NewPassword2InputField.onValueChanged.AddListener((newText) =>
            {
                m_NewPassword2InputField.text = PasswordValidation(newText);
            });

            m_NewEmailInputField = this.transform.Search("NewEmailInputField").GetComponent<TMP_InputField>();
            m_NewEmailInputField.characterValidation = TMP_InputField.CharacterValidation.EmailAddress;

            m_CreateBtn = this.transform.Search("BtnCreate").GetComponent<Button>();
            m_CreateBtn.onClick.AddListener(CreateNew);
        }


        private void SetUpLogInView()
        {
            m_LogInView = this.transform.Find("LogInMenu").gameObject;
            m_LowLogInComponents = m_LogInView.transform.Find("LogInButtons&Text");


            m_UserNameInputField = this.transform.Search("ExistingUserNameInputField").GetComponent<TMP_InputField>();
            m_UserNameInputField.onValueChanged.AddListener((newText) =>
            {
                m_UserNameInputField.text = UserNameValidation(newText);
            });


            var existingPasswordInputField = this.transform.Search("ExistingPassword1InputField");
            var togglePass = existingPasswordInputField.GetComponentInChildren<Toggle>();
            m_UserPasswordInputField = existingPasswordInputField.GetComponent<TMP_InputField>();
            m_PasswordInputFieldsList.Add(togglePass.name, m_UserPasswordInputField);
            togglePass.onValueChanged.AddListener((value) => ShowHidePassword(value, togglePass.name));
            m_UserPasswordInputField.onValueChanged.AddListener((newText) =>
            {
                m_UserPasswordInputField.text = UserNameValidation(newText);
            });

            m_ExampleBtn = this.transform.Search("ExampleBtn").GetComponent<Button>();
            m_LogInBtn = this.transform.Search("BtnLogin").GetComponent<Button>();
            m_LogInBtn.onClick.AddListener(Login);
            m_LogOutBtn = this.transform.Search("BtnLogout").GetComponent<Button>();
            m_LogOutBtn.onClick.AddListener(Logout);
            m_NewUserBtn = this.transform.Search("BtnNew").GetComponent<Button>();
            m_NewUserBtn.onClick.AddListener(() => { ShowLogInView(false); });

            m_CloseBtn = m_LogInView.transform.Find("CloseButton").GetComponent<Button>();
            m_CloseBtn.onClick.AddListener(() => Toggle(false));

            var logInInputFields = m_LogInView.transform.Find("InputFields");
            foreach (Transform child in logInInputFields)
            {
                var text = child.gameObject.GetComponent<TextMeshProUGUI>();
                if (text != null)
                    m_ListOfText.Add(text);
            }

            m_ExampleBtn = this.transform.Search("ExampleBtn").GetComponent<Button>();
            m_ExampleBtn.onClick.AddListener(ExamplesBtn);
        }

        private void ExamplesBtn()
        {
            m_UserNameInputField.text = LoadSaveMenuManager.EXAMPLES;
            m_UserPasswordInputField.text = LoadSaveMenuManager.EXAMPLES;
            Login();
        }


        private string UserNameValidation(string item)
        {
            if (item.Length > 0)
            {
                var lastchar = item[item.Length - 1];
                bool valid = (char.IsLetterOrDigit(lastchar) || lastchar == '-');
                if (valid == false)
                {
                    item = item.Remove(item.Length - 1);
                }
            }
            return item;
        }

        private string PasswordValidation(string item)
        {
            if (item.Length > 0)
            {
                var lastchar = item[item.Length - 1];
                bool valid = (char.IsWhiteSpace(lastchar) == false);
                if (valid == false)
                {
                    item = item.Remove(item.Length - 1);
                }
            }
            return item;
        }


        private void CreateNew()
        {
            NetworkingUser user = new NetworkingUser();
            user.m_UserName = m_NewUserNameInputField.text;
            user.m_Password = m_NewPassword1InputField.text;
            user.m_Email = m_NewEmailInputField.text;

            if (string.IsNullOrWhiteSpace(m_NewUserNameInputField.text))
            {
                OverrideGenericPromptMenu("Please enter a valid user name.");
                return;
            }

            if (string.IsNullOrWhiteSpace(m_NewPassword1InputField.text))
            {
                OverrideGenericPromptMenu("Please enter a valid new password.");
                return;
            }

            if (string.IsNullOrWhiteSpace(m_NewPassword2InputField.text))
            {
                OverrideGenericPromptMenu("Please enter a valid new password.");
                return;
            }

            if (string.IsNullOrWhiteSpace(m_NewEmailInputField.text))
            {
                OverrideGenericPromptMenu("Please enter a valid e-mail address.");
                return;
            }

            if (m_NewPassword1InputField.text != m_NewPassword2InputField.text)
            {
                OverrideGenericPromptMenu("Please ensure passwords match.");
                return;
            }

            try
            {
                MailAddress m = new MailAddress(m_NewEmailInputField.text);
            }
            catch(Exception e)
            {
                OverrideGenericPromptMenu("Please enter a valid e-mail address.");
                Debug.LogError(e.Message);
                return;
            }
            


            if (Core.Network.Users.DoesUserExist(user) == false)
            {
                Core.Network.Users.AddUser(user);
                Core.Network.Users.SaveUsers(null);
                Core.Network.Users.SetCurrentUser(user);
                HBGUIManager.Instance?.SetUserName();
                OverrideGenericPromptMenu($"{user.m_UserName} logged in successfully.");
                ShowLogInView(true);
                Toggle(false);
            }
            else
            {
                GenericPromptMenu.Instance.ShowOkayMenu("This username is already taken.");
            }


        }

        private void Login()
        {
            //I removed the else statement because was never called since when there is a user logged in the 
            //Loginbtn isn't visible nor interactable. I left the first statement for security though 
            if (Core.Network.Users.CurrentUser == null)
            {
                NetworkingUser user = new NetworkingUser();
                user.m_UserName = UserNameValidation(m_UserNameInputField.text);
                user.m_Password = PasswordValidation(m_UserPasswordInputField.text);

                if (Core.Network.Users.IsValidLoginDetails(user) == true)
                {
                    Core.Network.Users.SetCurrentUser(user);
                    HBGUIManager.Instance?.SetUserName();
                    m_LoadSaveMenuManager.FileNameListManualRefresh(null);
                    OverrideGenericPromptMenu($"Welcome, {Core.Network.Users.CurrentUser.m_UserName}.");
                    Toggle(false);
                }
                else
                {
                    Core.Network.Users.DebugPrintUserNames();
                    OverrideGenericPromptMenu("The username or password you entered is incorrect.");
                    m_UserPasswordInputField.text = null;
                }
            }
        }

        [InspectorButton]
        private void Logout()
        {
            Core.Network.Users.ClearCurrentUser();
            HBGUIManager.Instance?.SetUserName();
            m_LoadSaveMenuManager.ResetFileNameList();
            ResetLogInView();
        }

        public void Toggle(bool state)
        {
            IsVisible = state;
            Core.SceneLoader.AllowMenu = IsVisible;
            m_CanvasGroup.interactable = IsVisible;
            m_CanvasGroup.blocksRaycasts = IsVisible;

            if (IsVisible)
            {
                if (Core.Network.Users.CurrentUser == null)
                {
                    ClearLogInViewInputFields();
                    ShowLogInBtn(true);
                }
                else
                {
                    m_UserNameInputField.text = Core.Network.Users.CurrentUser.m_UserName;
                    ShowLogInBtn(false);
                }
                m_CanvasGroup.alpha = 1;
            }
            else
            {
                m_CanvasGroup.alpha = 0;
                if (m_MenuToggle != null)
                {
                    m_MenuToggle.isOn = false;
                }
            }
            
        }

        private void ShowHidePassword(bool state, string name)
        {
            TMP_InputField inputField = m_PasswordInputFieldsList[name]; //Take the input field corresponding to the toggle the user just interact with 
            inputField.contentType = (state) ? TMP_InputField.ContentType.Standard : TMP_InputField.ContentType.Password;  //change the state to make the password visible/invisible
            inputField.ForceLabelUpdate(); //Force the update, otherwise the input field is not updated till the next user interaction
        }

        private void ShowLogInView(bool state)
        {
            if (state)
            {
                m_LogInView.SetActive(state);
                m_NewUserView.SetActive(!state);
        

                if(Core.Network.Users.CurrentUser == null)
                {
                    ShowLogInBtn(true);
                }
                else
                {
                    ShowLogInBtn(false);
                }
            }
            else
            {
                m_LogInView.SetActive(state);
                m_NewUserView.SetActive(!state);;
            }
        }

        private void ShowLogInBtn(bool state)
        {
            m_LogOutBtn.SetActive(!state);
            DisableInputField(!state);
            ShowTextLabel(state);
            m_LowLogInComponents.gameObject.SetActive(state);
        }

        private void ResetLogInView()
        {
            ClearLogInViewInputFields();
            ShowLogInBtn(true);
        }

        private void ClearLogInViewInputFields()
        {
            m_UserNameInputField.text = null;
            m_UserPasswordInputField.text = null;
        }

        private void DisableInputField(bool state)
        {
            m_UserNameInputField.interactable = !state;
            m_UserPasswordInputField.interactable = !state;
        }

        private void ShowTextLabel(bool state)
        {
            foreach(var TMP in m_ListOfText)
            {
                TMP.enabled = state;
            }
        }

        private void OverrideGenericPromptMenu(string message)
        {
            if (HouseBuilderRoot.Instance != null)
            {
                GenericPromptMenu.Instance.ShowOkayMenu(message);
            }
            else
            {
                OscController.OscGenericPromptMenu.Instance.ShowOkayMenu(message);
            }
        }
    }
}
