using Emoter.Installers;
using IPA;
using SiraUtil.Zenject;
using System;
using IPALogger = IPA.Logging.Logger;
using Conf = IPA.Config.Config;
using IPA.Config.Stores;
using IPA.Loader;
using SiraUtil.Attributes;

namespace Emoter;

[Slog, NoEnableDisable, Plugin(RuntimeOptions.DynamicInit)]
public class Plugin
{
    public static readonly Guid FavoritesCategoryId = Guid.Parse("2cc144dd-c606-4aef-b9ce-09958abc7e7b");

    [Init]
    public Plugin(Conf conf, IPALogger logger, Zenjector zenjector, PluginMetadata pluginMetadata)
    {
        var config = conf.Generated<Config>();
        config.MaximumEmoteRatePerPlayer = 0.22f;
        config.Version = pluginMetadata.HVersion;

        zenjector.UseAutoBinder();
        zenjector.UseHttpService();
        zenjector.UseLogger(logger);
        zenjector.UseMetadataBinder<Plugin>();
        zenjector.Install<EmoterCoreInstaller>(Location.App);
        zenjector.Install<EmoterMenuInstaller>(Location.Menu);
        zenjector.Install(Location.App, Container => Container.BindInstance(config).AsSingle());
    }
}