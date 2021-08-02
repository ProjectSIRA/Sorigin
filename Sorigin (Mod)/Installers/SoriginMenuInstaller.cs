using SiraUtil;
using Sorigin.UI;
using Zenject;

namespace Sorigin.Installers
{
    internal class SoriginMenuInstaller : Installer
    {
        public override void InstallBindings()
        {
            Container.BindInterfacesTo<MenuButtonManager>().AsSingle();
            Container.Bind<SoriginFlowCoordinator>().FromNewComponentOnNewGameObject().AsSingle();
            Container.Bind<SoriginViewController>().FromNewComponentAsViewController().AsSingle();
        }
    }
}