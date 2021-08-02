using BeatSaberMarkupLanguage;
using BeatSaberMarkupLanguage.MenuButtons;
using System;
using Zenject;

namespace Sorigin.UI
{
    internal class MenuButtonManager : IInitializable, IDisposable
    {
        private readonly MenuButton _menuButton;
        private readonly MainFlowCoordinator _mainFlowCoordinator;
        private readonly SoriginFlowCoordinator _soriginFlowCoordinator;

        public MenuButtonManager(MainFlowCoordinator mainFlowCoordinator, SoriginFlowCoordinator soriginFlowCoordinator)
        {
            _mainFlowCoordinator = mainFlowCoordinator;
            _soriginFlowCoordinator = soriginFlowCoordinator;
            _menuButton = new MenuButton(nameof(Sorigin), ShowFlow);
        }

        public void Initialize()
        {
            MenuButtons.instance.RegisterButton(_menuButton);
        }

        public void Dispose()
        {
            if (MenuButtons.IsSingletonAvailable && BSMLParser.IsSingletonAvailable)
                MenuButtons.instance.UnregisterButton(_menuButton);
        }

        private void ShowFlow()
        {
            _soriginFlowCoordinator.Present(_mainFlowCoordinator);
        }
    }
}