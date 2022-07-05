using Emoter.Installers;
using IPA;
using SiraUtil.Zenject;
using IPALogger = IPA.Logging.Logger;

namespace Emoter;

[NoEnableDisable, Plugin(RuntimeOptions.DynamicInit)]
public class Plugin
{
    [Init]
    public Plugin(IPALogger logger, Zenjector zenjector)
    {
        zenjector.UseLogger(logger);
        zenjector.Install<EmoterCoreInstaller>(Location.App);
    }
}