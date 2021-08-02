using BeatSaberMarkupLanguage.Attributes;
using HMUI;

namespace Sorigin.UI.Panels
{
    internal class WelcomePanel
    {
        [UIComponent("welcome-text")]
        protected readonly CurvedTextMeshPro _welcomeText = null!;

        public void SetName(string name)
        {
            _welcomeText.text = $"Welcome, {name}.";
        }
    }
}