using BeatSaberMarkupLanguage.Attributes;
using System;
using UnityEngine;

namespace Sorigin.UI.Panels
{
    internal class LoginPanel
    {
        public event Action? ButtonClicked;

        [UIAction("login")]
        protected void Login()
        {
            ButtonClicked?.Invoke();
            Application.OpenURL("https://sorigin.org/login?redirect_url=http://localhost:20549");
        }
    }
}