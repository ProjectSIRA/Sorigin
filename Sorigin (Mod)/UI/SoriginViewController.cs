using BeatSaberMarkupLanguage.Attributes;
using BeatSaberMarkupLanguage.ViewControllers;
using SiraUtil.Logging;
using Sorigin.Models;
using Sorigin.Services;
using Sorigin.UI.Panels;
using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using Tweening;
using UnityEngine;
using Zenject;

namespace Sorigin.UI
{
    [ViewDefinition("Sorigin.Views.sorigin-view.bsml")]
    [HotReload(RelativePathToLayout = @"..\Views\sorigin-view.bsml")]
    internal class SoriginViewController : BSMLAutomaticViewController
    {
        private readonly List<(Panel, CanvasGroup)> _panels = new List<(Panel, CanvasGroup)>();

        [Inject]
        protected readonly SiraLog _siraLog = null!;

        [Inject]
        protected readonly ISoriginManager _soriginManager = null!;

        [Inject]
        protected readonly TimeTweeningManager _tweeningManager = null!;

        [Inject]
        protected readonly SoriginGrantService _soriginGrantService = null!;

        [UIValue("auth-host")]
        protected readonly AuthPanel _authPanel = new AuthPanel();

        [UIValue("login-host")]
        protected readonly LoginPanel _loginPanel = new LoginPanel();

        [UIValue("welcome-host")]
        protected readonly WelcomePanel _welcomePanel = new WelcomePanel();

        [UIValue("profile-host")]
        protected readonly ProfilePanel _profilePanel = new ProfilePanel();

        [UIComponent("auth-panel")]
        protected readonly RectTransform _auth = null!;
        private CanvasGroup _authCanvas = null!;

        [UIComponent("login-panel")]
        protected readonly RectTransform _login = null!;
        private CanvasGroup _loginCanvas = null!;

        [UIComponent("welcome-panel")]
        protected readonly RectTransform _welcome = null!;
        private CanvasGroup _welcomeCanvas = null!;

        [UIComponent("profile-panel")]
        protected readonly RectTransform _profile = null!;
        private CanvasGroup _profileCanvas = null!;

        [UIAction("#post-parse")]
        protected void Parsed()
        {
            _authCanvas = _auth.gameObject.AddComponent<CanvasGroup>();
            _loginCanvas = _login.gameObject.AddComponent<CanvasGroup>();
            _welcomeCanvas = _welcome.gameObject.AddComponent<CanvasGroup>();
            _profileCanvas = _profile.gameObject.AddComponent<CanvasGroup>();

            _panels.Clear();
            _panels.Add((Panel.Auth, _authCanvas));
            _panels.Add((Panel.Login, _loginCanvas));
            _panels.Add((Panel.Welcome, _welcomeCanvas));
            _panels.Add((Panel.Profile, _profileCanvas));
            _authCanvas.alpha = 0;
            _loginCanvas.alpha = 0;
            _welcomeCanvas.alpha = 0;
            _profileCanvas.alpha = 0;

            if (_soriginManager.Player != null)
            {
                SoriginManager_LoggedIn(_soriginManager.Player);
            }
        }

        protected override void DidActivate(bool firstActivation, bool addedToHierarchy, bool screenSystemEnabling)
        {
            _tweeningManager.KillAllTweens(this);
            base.DidActivate(firstActivation, addedToHierarchy, screenSystemEnabling);
            _soriginGrantService.GrantReceived += SoriginGrantService_GrantReceived;
            _profilePanel.LinkDiscordClicked += ProfilePanel_LinkDiscordClicked;
            _loginPanel.ButtonClicked += LoginPanel_ButtonClicked;
            _soriginManager.LoggedIn += SoriginManager_LoggedIn;
            _authPanel.SetTextVisibility(true);

            if (_soriginManager.Player != null)
            {
                SwitchToPanel(Panel.Profile, true);
            }
            else
            {
                SwitchToPanel(Panel.Login, true);
            }
        }

        private void ProfilePanel_LinkDiscordClicked()
        {
            SwitchToPanel(Panel.Login);
        }

        private void LoginPanel_ButtonClicked()
        {
            SwitchToPanel(Panel.Auth);
        }

        protected override void DidDeactivate(bool removedFromHierarchy, bool screenSystemDisabling)
        {
            _authPanel.SetTextVisibility(true);
            _tweeningManager.KillAllTweens(this);
            _soriginManager.LoggedIn -= SoriginManager_LoggedIn;
            _loginPanel.ButtonClicked -= LoginPanel_ButtonClicked;
            _profilePanel.LinkDiscordClicked -= ProfilePanel_LinkDiscordClicked;
            _soriginGrantService.GrantReceived -= SoriginGrantService_GrantReceived;
            base.DidDeactivate(removedFromHierarchy, screenSystemDisabling);
        }

        private async void SoriginManager_LoggedIn(SoriginUser user)
        {
            try
            {
                _welcomePanel.SetName(user.Username);
                SwitchToPanel(Panel.Welcome);
                _profilePanel.SetProfile(user);
                await Task.Delay(2000);
                SwitchToPanel(Panel.Profile);
            }
            catch (Exception e)
            {
                _siraLog.Info(e);
            }
        }

        private void SoriginGrantService_GrantReceived(string _)
        {
            _authPanel.SetTextVisibility(false);
        }

        private void SwitchToPanel(Panel panel, bool instant = false)
        {
            const float inSpeed = 0.7f;
            const float outSpeed = 0.35f;
            _tweeningManager.KillAllTweens(this);
            foreach (var panelSet in _panels)
            {
                if (panelSet.Item1 == panel)
                {
                    panelSet.Item2.gameObject.SetActive(true);
                    if (instant)
                    {
                        panelSet.Item2.alpha = 1f;
                    }
                    else
                    {
                        Tween t = new FloatTween(panelSet.Item2.alpha, 1f, val => panelSet.Item2.alpha = val, inSpeed, EaseType.InQuad, outSpeed * 1.5f);
                        _tweeningManager.AddTween(t, this);
                    }
                }
                else
                {
                    if (instant)
                    {
                        panelSet.Item2.alpha = 1f;
                        panelSet.Item2.gameObject.SetActive(false);
                    }
                    else if (panelSet.Item2.gameObject.activeSelf)
                    {
                        Tween t = new FloatTween(panelSet.Item2.alpha, 0, val => panelSet.Item2.alpha = val, outSpeed, EaseType.OutQuad);
                        t.onCompleted += delegate () { panelSet.Item2.gameObject.SetActive(false); };
                        _tweeningManager.AddTween(t, this);
                    }
                }
            }
        } 

        private enum Panel
        {
            Auth,
            Login,
            Welcome,
            Profile
        }
    }
}