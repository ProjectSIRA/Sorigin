using IPA;
using SiraUtil.Attributes;
using SiraUtil.Zenject;
using Sorigin.Installers;
using IPALogger = IPA.Logging.Logger;

namespace Sorigin
{
    [Plugin(RuntimeOptions.DynamicInit), Slog]
    public class Plugin
    {
        [Init]
        public Plugin(IPALogger logger, Zenjector zenjector)
        {
            zenjector.Install<SoriginInstaller>(Location.App);
            zenjector.Install<SoriginMenuInstaller>(Location.Menu);
            zenjector.UseMetadataBinder<Plugin>();
            zenjector.UseLogger(logger);
            zenjector.UseHttpService();
        }

        [OnEnable]
        public void OnEnable()
        {

        }

        [OnDisable]
        public void OnDisable()
        {

        }
    }
}