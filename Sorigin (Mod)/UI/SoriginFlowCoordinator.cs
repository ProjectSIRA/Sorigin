using BeatSaberMarkupLanguage;
using HMUI;
using Zenject;

namespace Sorigin.UI
{
    internal class SoriginFlowCoordinator : FlowCoordinator
    {
        private FlowCoordinator? _parentFlow;

        [Inject]
        protected readonly SoriginViewController _soriginView = null!;

        protected override void DidActivate(bool firstActivation, bool addedToHierarchy, bool screenSystemEnabling)
        {
            if (firstActivation)
            {
                showBackButton = true;
                SetTitle(nameof(Sorigin));
                ProvideInitialViewControllers(_soriginView);
            }
        }

        public void Present(FlowCoordinator parentFlow)
        {
            _parentFlow = parentFlow;
            BeatSaberUI.PresentFlowCoordinator(_parentFlow, this);
        }

        protected override void BackButtonWasPressed(ViewController topViewController)
        {
            if (_parentFlow != null)
            {
                BeatSaberUI.DismissFlowCoordinator(_parentFlow, this);
            }
        }
    }
}