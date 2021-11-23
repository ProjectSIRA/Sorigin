using Sorigin.Services;
using Zenject;

namespace Sorigin.Installers
{
    internal class SoriginModCoreInstaller : Installer
    {
        public override void InstallBindings()
        {
            Container.BindInterfacesTo<SoriginService>().AsSingle();
        }
    }
}