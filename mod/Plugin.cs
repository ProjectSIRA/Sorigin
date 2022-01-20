using IPA;
using SiraUtil.Zenject;
using Sorigin.Installers;
using IPALogger = IPA.Logging.Logger;

namespace Sorigin
{
    [Plugin(RuntimeOptions.DynamicInit)]
    public class Plugin
    {
        [Init]
        public Plugin(IPALogger logger, Zenjector zenjector)
        {
            zenjector.UseHttpService();
            zenjector.UseLogger(logger);
            zenjector.Install<SoriginModCoreInstaller>(Location.App);
        }
    }
}