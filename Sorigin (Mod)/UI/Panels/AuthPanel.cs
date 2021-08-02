using BeatSaberMarkupLanguage.Attributes;
using HMUI;

namespace Sorigin.UI.Panels
{
    internal class AuthPanel
    {
        [UIComponent("check-text")]
        protected readonly CurvedTextMeshPro _checkText = null!;

        [UIComponent("sign-in-text")]
        protected readonly CurvedTextMeshPro _signInText = null!;

        public void SetTextVisibility(bool areVisible)
        {
            _checkText.alpha = areVisible ? 1 : 0;
            _signInText.alpha = areVisible ? 1 : 0;
        }
    }
}