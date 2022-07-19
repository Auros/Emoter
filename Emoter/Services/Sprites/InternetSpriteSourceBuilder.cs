using Emoter.Models;
using IPA.Utilities.Async;
using SiraUtil.Web;
using System.Collections.Generic;
using System.Threading.Tasks;
using UnityEngine;
using Zenject;

namespace Emoter.Services.Sprites;

internal class InternetSpriteSourceBuilder : ISpriteSourceBuilder, ILateDisposable
{
    private readonly IHttpService _httpService;
    private readonly List<Sprite> _spritesMade = new();
    private readonly IEmoteErrorSpriteService _emoteErrorSpriteService;

    public InternetSpriteSourceBuilder(IHttpService httpService, IEmoteErrorSpriteService emoteErrorSpriteService)
    {
        _httpService = httpService;
        _emoteErrorSpriteService = emoteErrorSpriteService;
    }

    public async Task<Sprite> BuildSpriteAsync(string source)
    {
        if (source == Emote.Empty.Source)
            return await _emoteErrorSpriteService.GetErrorSpriteAsync(EmoteError.NotFound);

        var response = await _httpService.GetAsync(source);
        if (!response.Successful)
            return await _emoteErrorSpriteService.GetErrorSpriteAsync(EmoteError.NotFound);

        var bytes = await response.ReadAsByteArrayAsync();
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
            UnityEngine.Object.Destroy(sprite.texture);
            UnityEngine.Object.Destroy(sprite);
        }
        _spritesMade.Clear();
    }
}
