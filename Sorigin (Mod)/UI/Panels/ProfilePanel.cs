using BeatSaberMarkupLanguage;
using BeatSaberMarkupLanguage.Attributes;
using HMUI;
using Sorigin.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using UnityEngine.UI;

namespace Sorigin.UI.Panels
{
    internal class ProfilePanel
    {
        [UIComponent("pfp-image")]
        protected readonly Image _pfpImage = null!;

        [UIComponent("name-text")]
        protected readonly CurvedTextMeshPro _nameText = null!;

        [UIComponent("platforms-text")]
        protected readonly CurvedTextMeshPro _platformsText = null!;

        [UIComponent("link-discord-button")]
        protected readonly Button _linkDiscordButton = null!;

        public event Action? LinkDiscordClicked;

        public void SetProfile(SoriginUser user)
        {
            _nameText.text = user.Username;

            List<string> platforms = new List<string>();
            if (user.Discord != null)
                platforms.Add("<color=#7289da>Discord</color>");
            if (user.Steam != null)
                platforms.Add("<color=#075a8e>Steam</color>");

            if (platforms.Count != 0)
                _platformsText.text = platforms.Aggregate((a, b) => $"{a}, {b}");

            _pfpImage.SetImage(user.GetProfilePicture());

            _linkDiscordButton.gameObject.SetActive(user.Discord == null);
        }

        [UIAction("link-discord")]
        protected void LinkDiscord()
        {
            LinkDiscordClicked?.Invoke();
        }
    }
}