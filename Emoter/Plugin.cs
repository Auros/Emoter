using Emoter.Daemons;
using Emoter.Installers;
using Emoter.UI.Main.Controllers;
using IPA;
using SiraUtil.Zenject;
using Zenject;
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
        zenjector.Install(Location.Menu, Container =>
        {
            Container.BindInterfacesTo<EmoteScreenDaemon>().AsSingle();
            Container.Bind<QuickEmoteViewController>().FromNewComponentAsViewController().AsSingle();
        });
    }
}