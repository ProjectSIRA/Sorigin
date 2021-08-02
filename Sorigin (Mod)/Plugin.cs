using IPA;
using IPA.Loader;
using SiraUtil;
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
        public Plugin(IPALogger logger, Zenjector zenjector, PluginMetadata metadata)
        {
            zenjector.On<PCAppInit>().Pseudo(Container =>
            {
                Container.BindLoggerAsSiraLogger(logger);
                Container.BindInstance(new UBinder<Plugin, PluginMetadata>(metadata)).AsCached();
            });
            zenjector.OnApp<SoriginInstaller>();
            zenjector.OnMenu<SoriginMenuInstaller>();
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