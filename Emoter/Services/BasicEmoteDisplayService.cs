using Emoter.Components;
using Emoter.Models;
using IPA.Utilities.Async;
using System;
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

    public void Spawn(Emote emote, Vector3 position, Vector3? direction = null)
    {
        _ = Task.Run(() => SpawnAsync(emote, position, direction));
    }

    private async Task SpawnAsync(Emote emote, Vector3 position, Vector3? direction = null)
    {
        var shader = await _emoterResourceService.LoadEmoteShaderAsync();
        var sprite = await _spriteSourceBuilder.BuildSpriteAsync(emote.Source);

        await UnityMainThreadTaskScheduler.Factory.StartNew(() =>
        {
            Material mat = new(shader);
            var cube = GameObject.CreatePrimitive(PrimitiveType.Cube);
            cube.GetComponent<MeshRenderer>().material = mat;
            mat.mainTexture = sprite.texture;
            cube.AddComponent<OneWayMaterialPropertyScaleBinder>();

            Vector3 endPos;
            var originalPos = position;
            cube.transform.localPosition = originalPos;
            endPos = cube.transform.localPosition + (direction ?? Vector3.forward) * 10f;

            var tween = _tweeningManager.AddTween(new FloatTween(0f, 1f, v =>
            {
                var lerped = Vector3.Lerp(originalPos, endPos, v);
                cube.transform.localPosition = Vector3.Lerp(originalPos, endPos, v);
            }, 4f, EaseType.OutSine), cube);
            tween.onCompleted += () =>
            {
                UnityEngine.Object.Destroy(cube);
                UnityEngine.Object.Destroy(mat);
            };
        });
    }
}