using BeatSaberMarkupLanguage.Attributes;
using BeatSaberMarkupLanguage.ViewControllers;

namespace Sorigin.UI
{
    [ViewDefinition("Sorigin.Views.sorigin-view.bsml")]
    [HotReload(RelativePathToLayout = @"..\Views\sorigin-view.bsml")]
    internal class SoriginViewController : BSMLAutomaticViewController
    {
    }
}