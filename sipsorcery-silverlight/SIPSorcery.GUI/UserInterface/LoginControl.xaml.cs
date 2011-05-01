﻿using System;
using System.Linq;
using System.ServiceModel.DomainServices.Client;
using System.ServiceModel.DomainServices.Client.ApplicationServices;
using System.Text.RegularExpressions;
using System.Threading;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Documents;
using System.Windows.Ink;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Animation;
using System.Windows.Shapes;
using SIPSorcery.Persistence;
using SIPSorcery.Sys;
using SIPSorcery.Entities.Services;
//using SIPSorcery.Web.RIA;

namespace SIPSorcery
{
    public partial class LoginControl : UserControl
    {
        private const int MAX_STATUS_MESSAGE_LENGTH = 512;

        //private SIPSorceryInvite.SIPSorceryInviteServiceClient m_inviteProxy;
        private string m_loginUsername;
        //private string m_inviteCode;
        //private AuthenticationService m_authenticationService;
        private SIPEntitiesDomainContext m_riaContext;

        public event Action<string> CreateNewAccountClicked;    // The parameter is the verified invite code.
        public event Action<string, string> Authenticated;      // Parameters are the authenticated username and authID from the server.

        public LoginControl()
        {
            InitializeComponent();
        }

        public void SetProxy(
            //SIPSorceryInvite.SIPSorceryInviteServiceClient inviteProxy, 
            SIPEntitiesDomainContext riaContext)
        {
            //m_inviteProxy = inviteProxy;
            //m_inviteProxy.IsInviteCodeValidCompleted += CheckInviteCodeComplete;
            //m_authenticationService = authenticationService;
            m_riaContext = riaContext;
        }

        private void LoginButton_Click(object sender, System.Windows.RoutedEventArgs e)
        {
            Login();
        }

        private void Login()
        {
            WriteLoginMessage(null);
            string username = m_usernameTextBox.Text;
            string password = m_passwordTextBox.Password;

            if (username == null || username.Trim().Length == 0)
            {
                WriteLoginMessage("Username was empty.");
            }
            else if (password == null || password.Trim().Length == 0)
            {
                WriteLoginMessage("Password was empty.");
            }
            else
            {
                WriteLoginMessage("Attempting login...");
                m_loginUsername = username.Trim();
                LoginParameters loginParams = new LoginParameters(m_loginUsername, password);
                //m_webContext.Authentication.Login(loginParams, LoginComplete, null);
                //m_authenticationService.Login(loginParams, LoginComplete, null);
                var query = m_riaContext.LoginQuery(m_loginUsername, password, true, null);
                m_riaContext.Load(query, LoadBehavior.RefreshCurrent, LoginComplete, null);
            }
        }

        public void Clear()
        {
            UIHelper.SetText(m_usernameTextBox, String.Empty);
            UIHelper.SetText(m_passwordTextBox, String.Empty);
            UIHelper.SetText(m_loginError, String.Empty);
            UIHelper.SetText(m_inviteCodeError, String.Empty);
            UIHelper.SetText(m_inviteCodeTextBox, String.Empty);
        }

        public void WriteLoginMessage(string message)
        {
            if (message.IsNullOrBlank())
            {
                UIHelper.SetVisibility(m_loginError, Visibility.Collapsed);
            }
            else
            {
                UIHelper.SetVisibility(m_loginError, Visibility.Visible);
                UIHelper.SetText(m_loginError, (message.Length > MAX_STATUS_MESSAGE_LENGTH) ? message.Substring(0, MAX_STATUS_MESSAGE_LENGTH) : message);
            }
        }

        public void WriteInviteCodeErrorMessage(string message)
        {
            if (message.IsNullOrBlank())
            {
                UIHelper.SetText(m_inviteCodeError, null);
                UIHelper.SetVisibility(m_inviteCodeError, Visibility.Collapsed);
            }
            else
            {

                UIHelper.SetVisibility(m_inviteCodeError, Visibility.Visible);
                UIHelper.SetText(m_inviteCodeError, (message.Length > MAX_STATUS_MESSAGE_LENGTH) ? message.Substring(0, MAX_STATUS_MESSAGE_LENGTH) : message);
            }
        }

        public void EnableCreateAccount()
        {
            UIHelper.SetVisibility(m_orLabel, Visibility.Visible);
            UIHelper.SetVisibility(m_createAccountLink, Visibility.Visible);
        }

        /// <summary>
        /// If an invite code is required to create a new account this method will be called. If hides
        /// the create customer link and instead displays the enter invite code text box.
        /// </summary>
        public void EnableInviteCode()
        {
            UIHelper.SetVisibility(m_orLabel, Visibility.Visible);
            UIHelper.SetVisibility(m_accountLoginPanel, Visibility.Visible);
            UIHelper.SetVisibility(m_createAccountLink, Visibility.Collapsed);
            UIHelper.SetVisibility(m_inviteCodeGrid, Visibility.Visible);
        }

        public void DisableNewAccounts(string disabledMessage)
        {
            UIHelper.SetVisibility(m_inviteCodeGrid, Visibility.Collapsed);
            UIHelper.SetVisibility(m_orLabel, Visibility.Collapsed);
            UIHelper.SetVisibility(m_createAccountLink, Visibility.Collapsed);
        }

        private void LoginTextBox_KeyUp(object sender, System.Windows.Input.KeyEventArgs e)
        {
            if (e.Key == Key.Enter)
            {
                Login();
            }
        }

        private void CreateNewAccount_Click(object sender, System.Windows.RoutedEventArgs e)
        {
            if (CreateNewAccountClicked != null)
            {
                CreateNewAccountClicked(null);
            }
        }

        private void CheckInviteCode_Click(object sender, System.Windows.RoutedEventArgs e)
        {
            //WriteInviteCodeErrorMessage(null);
            //m_inviteCode = m_inviteCodeTextBox.Text.Trim();
            //Guid inviteCodeGUID = Guid.Empty;

            //if (m_inviteCode.IsNullOrBlank())
            //{
            //    WriteInviteCodeErrorMessage("The invite code was empty. Please enter a valid invite code.");
            //}
            //else if (!Guid.TryParse(m_inviteCode, out inviteCodeGUID))
            //{
            //    WriteInviteCodeErrorMessage("The invite code specified was not in a valid format. Please enter a valid invite code.");
            //}
            //else
            //{
            //    WriteInviteCodeErrorMessage("Checking invite code...");
            //    m_inviteProxy.IsInviteCodeValidAsync(m_inviteCode);
            //}
        }

        private void CheckInviteCodeComplete(object sender, SIPSorceryInvite.IsInviteCodeValidCompletedEventArgs e)
        {
            try
            {
                if (e.Error != null)
                {
                    WriteInviteCodeErrorMessage("Server error. " + e.Error.Message);
                }
                else if (e.Result != null)
                {
                    WriteInviteCodeErrorMessage(e.Result);
                }
                else
                {
                    if (CreateNewAccountClicked != null)
                    {
                        //CreateNewAccountClicked(m_inviteCode);
                        CreateNewAccountClicked(null);
                    }
                }
            }
            catch (Exception excp)
            {
                WriteInviteCodeErrorMessage("There was an unexpected error checking the invite code. " + excp.Message);
            }
        }

        //private void LoginComplete(LoginOperation op)
        private void LoginComplete(LoadOperation op)
        {
            if (op.HasError)
            {
                // Remove the error information the RIA domain services framework adds in and that will just confuse things.
                string errorMessage = Regex.Replace(op.Error.Message, @"Load operation failed .*?\.", "");
                WriteLoginMessage("Error logging in." + errorMessage);
                op.MarkErrorAsHandled();
                UIHelper.SetText(m_passwordTextBox, String.Empty);
            }
            else if (op.Entities.Count() == 0)
            {
                WriteLoginMessage("Login failed.");
                UIHelper.SetText(m_passwordTextBox, String.Empty);
            }
            else
            {
                Authenticated(m_loginUsername, null);
            }
        }

        private void InviteCodeTextBox_KeyUp(object sender, System.Windows.Input.KeyEventArgs e)
        {
            if (e.Key == Key.Enter)
            {
                CheckInviteCode_Click(null, null);
            }
        }
    }
}