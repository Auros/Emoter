using Emoter.Models;
using IPA.Loader;
using IPA.Utilities.Async;
using SiraUtil.Zenject;
using System.Collections.Generic;
using System.IO;
using System.Reflection;
using System.Threading.Tasks;
using UnityEngine;
using Zenject;

namespace Emoter.Services.Sprites;

internal class AssemblySpriteSourceBuilder : ISpriteSourceBuilder, ILateDisposable
{
    private readonly Assembly _assembly;
    private readonly IEmoteErrorSpriteService _emoteErrorSpriteService;

    private readonly List<Sprite> _spritesMade = new();

    public AssemblySpriteSourceBuilder(UBinder<Plugin, PluginMetadata> metadataBinder, IEmoteErrorSpriteService emoteErrorSpriteService)
    {
        _assembly = metadataBinder.Value.Assembly;
        _emoteErrorSpriteService = emoteErrorSpriteService;
    }

    public async Task<Sprite> BuildSpriteAsync(string source)
    {
        if (source == Emote.Empty.Source)
            return await _emoteErrorSpriteService.GetErrorSpriteAsync(EmoteError.NotFound);

        byte[]? bytes = null;
        try
        {
            using MemoryStream ms = new();
            using var resourceStream = _assembly.GetManifestResourceStream(source);
            await resourceStream.CopyToAsync(ms);

            bytes = ms.ToArray();
        }
        catch (FileNotFoundException)
        {
            return await _emoteErrorSpriteService.GetErrorSpriteAsync(EmoteError.NotFound);
        }
        catch
        {
            return await _emoteErrorSpriteService.GetErrorSpriteAsync(EmoteError.LoadingFailed);
        }

        if (bytes is null)
            return await _emoteErrorSpriteService.GetErrorSpriteAsync(EmoteError.LoadingFailed);

        var sprite = await UnityMainThreadTaskScheduler.Factory.StartNew(() =>
        {
            try
            {
                var tex = BeatSaberMarkupLanguage.Utilities.LoadTextureRaw(bytes);
                tex.wrapMode = TextureWrapMode.Clamp;
                var sprite = Sprite.Create(tex, new(0f, 0f, tex.width, tex.height), Vector2.zero, 100f, 0, SpriteMeshType.FullRect);
                tex.name = source;
                return sprite;
            }
            catch
            {
                return null;
            }
        });

        if (sprite is null)
            return await _emoteErrorSpriteService.GetErrorSpriteAsync(EmoteError.LoadingFailed);

        _spritesMade.Add(sprite);
        return sprite;
    }

    public void LateDispose()
    {
        foreach (var sprite in _spritesMade)
        {
            Object.Destroy(sprite.texture);
            Object.Destroy(sprite);
        }
        _spritesMade.Clear();
    }
}