using Emoter.Models;
using IPA.Utilities.Async;
using System.Threading.Tasks;
using Tweening;
using UnityEngine;

namespace Emoter.Services;

internal class BasicEmoteDisplayService : IEmoteDisplayService
{
    private readonly TweeningManager _tweeningManager;
    private readonly ISpriteSourceBuilder _spriteSourceBuilder;
    private readonly IEmoterResourceService _emoterResourceService;

    public BasicEmoteDisplayService(TimeTweeningManager tweeningManager, ISpriteSourceBuilder spriteSourceBuilder, IEmoterResourceService emoterResourceService)
    {
        _tweeningManager = tweeningManager;
        _spriteSourceBuilder = spriteSourceBuilder;
        _emoterResourceService = emoterResourceService;
    }

    public void Spawn(Emote emote, Vector3 position)
    {
        _ = Task.Run(() => SpawnAsync(emote, position));
    }

    private async Task SpawnAsync(Emote emote, Vector3 position)
    {
        var shader = await _emoterResourceService.LoadEmoteShaderAsync();
        var sprite = await _spriteSourceBuilder.BuildSpriteAsync(emote.Source);

        await UnityMainThreadTaskScheduler.Factory.StartNew(() =>
        {
            Material mat = new(shader);
            var cube = GameObject.CreatePrimitive(PrimitiveType.Cube);
            cube.GetComponent<MeshRenderer>().material = mat;
            mat.mainTexture = sprite.texture;

            cube.transform.localPosition = position;
            var tween = _tweeningManager.AddTween(new FloatTween(1f, 0.05f, v => cube.transform.localScale = Vector3.one * v, 2f, EaseType.InQuad), cube);
            tween.onCompleted += () =>
            {
                Object.Destroy(cube);
                Object.Destroy(mat);
            };
        });
    }
}