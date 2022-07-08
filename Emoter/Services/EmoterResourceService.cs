using IPA.Loader;
using IPA.Utilities.Async;
using SiraUtil.Logging;
using SiraUtil.Zenject;
using System.IO;
using System.Reflection;
using System.Threading.Tasks;
using UnityEngine;
using Zenject;

namespace Emoter.Services;

internal interface IEmoterResourceService
{
    Task<Shader> LoadEmoteShaderAsync();
}

internal class EmoterResourceService : IEmoterResourceService, ILateDisposable
{
    private Shader? _shader;
    private bool _isLoading;
    private AssetBundle? _assetBundle;

    private const string RESOURCE_PATH = "Emoter.Resources.Bundle.asset";

    private readonly SiraLog _siraLog;
    private readonly Assembly _assembly;

    public EmoterResourceService(SiraLog siraLog, UBinder<Plugin, PluginMetadata> metadataBinder)
    {
        _siraLog = siraLog;
        _assembly = metadataBinder.Value.Assembly;
    }

    public async Task<Shader> LoadEmoteShaderAsync()
    {
        if (_shader != null)
            return _shader;

        await UnityMainThreadTaskScheduler.Factory.StartNew(async () =>
        {
            if (_shader != null)
                return;

            if (_isLoading)
            {
                while (_isLoading)
                    await Task.Yield();

                return;
            }

            _isLoading = true;
            _siraLog.Debug("Loading bundle resource stream.");
            using Stream stream = _assembly.GetManifestResourceStream(RESOURCE_PATH);
            _siraLog.Debug("Loading asset bundle from stream.");
            var request = AssetBundle.LoadFromStreamAsync(stream);
            while (!request.isDone)
                await Task.Yield();

            _assetBundle = request.assetBundle;
            _siraLog.Debug("Asset bundle loaded. Fetching Emote Shader.");
            _shader = _assetBundle.LoadAsset<Shader>("S_EmoterEmote");
            _assetBundle.Unload(false);
            _isLoading = false;
        });
        return _shader!;
    }

    public void LateDispose()
    {
        if (_assetBundle == null)
            return;

        _shader = null;
        _isLoading = false;
        _assetBundle.Unload(true);
        
    }
}
