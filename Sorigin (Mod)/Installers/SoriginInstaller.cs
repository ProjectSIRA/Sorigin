using Sorigin.Services;
using Zenject;

namespace Sorigin.Installers
{
    internal class SoriginInstaller : Installer
    {
        public override void InstallBindings()
        {
            Container.BindInterfacesTo<SoriginManager>().AsSingle();
            Container.BindInterfacesAndSelfTo<SoriginGrantService>().AsSingle();
            Container.BindInterfacesAndSelfTo<SoriginNetworkService>().AsSingle();
        }
    }
}