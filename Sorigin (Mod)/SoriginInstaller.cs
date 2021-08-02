using Sorigin.Services;
using Zenject;

namespace Sorigin
{
    internal class SoriginInstaller : Installer
    {
        public override void InstallBindings()
        {
            Container.Bind<Http>().AsSingle();
            Container.BindInterfacesTo<SoriginManager>().AsSingle();
            Container.BindInterfacesAndSelfTo<SoriginGrantService>().AsSingle();
            Container.BindInterfacesAndSelfTo<SoriginNetworkService>().AsSingle();
        }
    }
}