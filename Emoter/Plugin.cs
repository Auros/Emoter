using Emoter.Installers;
using IPA;
using SiraUtil.Zenject;
using System;
using IPALogger = IPA.Logging.Logger;

namespace Emoter;

[NoEnableDisable, Plugin(RuntimeOptions.DynamicInit)]
public class Plugin
{
    public static readonly Guid FavoritesCategoryId = Guid.Parse("2cc144dd-c606-4aef-b9ce-09958abc7e7b");

    [Init]
    public Plugin(IPALogger logger, Zenjector zenjector)
    {
        zenjector.UseAutoBinder();
        zenjector.UseLogger(logger);
        zenjector.UseMetadataBinder<Plugin>();
        zenjector.Install<EmoterCoreInstaller>(Location.App);
        zenjector.Install<EmoterMenuInstaller>(Location.Menu);
    }
}