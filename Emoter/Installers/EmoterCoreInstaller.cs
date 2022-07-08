using Emoter.Services;
using Zenject;

namespace Emoter.Installers;

internal class EmoterCoreInstaller : Installer
{
    public override void InstallBindings()
    {
        Container.BindInterfacesTo<OfflineEmoteService>().AsSingle();
    }
}