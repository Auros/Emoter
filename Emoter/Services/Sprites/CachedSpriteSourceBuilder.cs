using System.Collections.Generic;
using System.Threading.Tasks;
using UnityEngine;

namespace Emoter.Services.Sprites;

/// <summary>
/// Supports loading from our assembly and the internet.
/// </summary>
internal class CachedSpriteSourceBuilder : ISpriteSourceBuilder
{
    private readonly Dictionary<string, Sprite> _cache = new();
    private readonly AssemblySpriteSourceBuilder _assemblySpriteSourceBuilder;

    public CachedSpriteSourceBuilder(AssemblySpriteSourceBuilder assemblySpriteSourceBuilder)
    {
        _assemblySpriteSourceBuilder = assemblySpriteSourceBuilder;
    }

    public async Task<Sprite> BuildSpriteAsync(string source)
    {
        if (_cache.TryGetValue(source, out var sprite))
            return sprite;


        if (source.StartsWith(nameof(Emoter)))
            sprite = await _assemblySpriteSourceBuilder.BuildSpriteAsync(source);

        _cache.Add(source, sprite);
        return sprite;
    }
}