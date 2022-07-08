using Emoter.Services;
using SiraUtil.Attributes;
using SiraUtil.Zenject;
using System.Threading;
using System.Threading.Tasks;
using UnityEngine;

namespace Emoter.Testing;

[Bind(Location.Menu)]
internal class CustomShaderLoadTest : IAsyncInitializable
{
    private readonly ISpriteSourceBuilder _spriteSourceBuilder;
    private readonly IEmoterResourceService _emoterResourceService;

    public CustomShaderLoadTest(ISpriteSourceBuilder spriteSourceBuilder, IEmoterResourceService emoterResourceService)
    {
        _spriteSourceBuilder = spriteSourceBuilder;
        _emoterResourceService = emoterResourceService;
    }

    public async Task InitializeAsync(CancellationToken token)
    {
        var shader = await _emoterResourceService.LoadEmoteShaderAsync();
        Material mat = new(shader);
        var spr = await _spriteSourceBuilder.BuildSpriteAsync("Emoter.Resources.Emotes.Unicode.Joy.png");
        mat.mainTexture = spr.texture;

        var cube = GameObject.CreatePrimitive(PrimitiveType.Cube);
        cube.GetComponent<MeshRenderer>().material = mat;

    }
}