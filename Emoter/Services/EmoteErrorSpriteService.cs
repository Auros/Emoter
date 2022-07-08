using BeatSaberMarkupLanguage;
using Emoter.Models;
using System;
using System.Collections.Generic;
using UnityEngine;
using Zenject;

namespace Emoter.Services;

internal interface IEmoteErrorSpriteService
{
    Sprite GetErrorSprite(EmoteError type);
}

internal class EmoteErrorSpriteService : IEmoteErrorSpriteService, ILateDisposable
{
    private readonly Dictionary<EmoteError, Sprite> _errorSprites = new();

    public Sprite GetErrorSprite(EmoteError type)
    {
        if (!_errorSprites.TryGetValue(type, out Sprite sprite))
        {
            var tex = Utilities.FindTextureInAssembly(GetErrorPath(type));
            sprite = Sprite.Create(tex, new(0f, 0f, tex.width, tex.height), Vector2.zero, 100f, 0, SpriteMeshType.FullRect);
            tex.wrapMode = TextureWrapMode.Clamp;
            _errorSprites.Add(type, sprite);
        }
        return sprite;
    }

    public void LateDispose()
    {
        foreach (var sprite in _errorSprites.Values)
        {
            UnityEngine.Object.Destroy(sprite.texture);
            UnityEngine.Object.Destroy(sprite);
        }
        _errorSprites.Clear();
    }

    private string GetErrorPath(EmoteError type) => type switch
    {
        EmoteError.LoadingFailed => "Emoter.Resources.Errors.LoadingFailed.png",
        EmoteError.NotFound => "Emoter.Resources.Errors.NotFound.png",
        EmoteError.InvalidPermissions => "Emoter.Resources.Errors.InvalidPermissions.png",
        _ => throw new NotImplementedException(),
    };
}