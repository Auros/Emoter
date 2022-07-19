using IPA.Loader;
using IPA.Utilities.Async;
using SiraUtil.Logging;
using SiraUtil.Zenject;
using System.IO;
using System.Reflection;
using System.Threading.Tasks;
using UnityEngine;
using Zenject;

namespace Emoter.Services.Other;

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

        _siraLog.Debug("Starting shader load");
        await UnityMainThreadTaskScheduler.Factory.StartNew(async () =>
        {
            if (_shader != null)
            {
                _siraLog.Warn("Shader prefire!");
                return;
            }    

            if (_isLoading)
            {
                _siraLog.Warn("The shader is already loading, awaiting response.");
                while (_isLoading)
                    await Task.Yield();
                _siraLog.Debug($"Shader load response received: Shader Status: {_shader != null}");
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
            _siraLog.Debug($"Loaded Shader: {_shader != null}");
            _assetBundle.Unload(false);
            _isLoading = false;
        });

        while (_shader == null)
            await Task.Yield();

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
