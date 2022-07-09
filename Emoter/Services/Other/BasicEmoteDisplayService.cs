using Emoter.Components;
using Emoter.Models;
using IPA.Utilities.Async;
using System;
using System.Threading.Tasks;
using Tweening;
using UnityEngine;

namespace Emoter.Services.Other;

internal class BasicEmoteDisplayService : IEmoteDisplayService
{
    private readonly TweeningManager _tweeningManager;
    private readonly ISpriteSourceBuilder _spriteSourceBuilder;
    private readonly IEmoterResourceService _emoterResourceService;

    public const float MaximumDistance = 10f;
    public const float MaximumDuration = 5f;

    public BasicEmoteDisplayService(TimeTweeningManager tweeningManager, ISpriteSourceBuilder spriteSourceBuilder, IEmoterResourceService emoterResourceService)
    {
        _tweeningManager = tweeningManager;
        _spriteSourceBuilder = spriteSourceBuilder;
        _emoterResourceService = emoterResourceService;
    }

    public void Spawn(Emote emote, EmoteDisplayOptions options)
    {
        _ = Task.Run(() => SpawnAsync(emote, options));
    }

    private async Task SpawnAsync(Emote emote, EmoteDisplayOptions options)
    {
        var shader = await _emoterResourceService.LoadEmoteShaderAsync();
        var sprite = await _spriteSourceBuilder.BuildSpriteAsync(emote.Source);

        await UnityMainThreadTaskScheduler.Factory.StartNew(() =>
        {
            Material mat = new(shader);
            var cube = GameObject.CreatePrimitive(PrimitiveType.Cube);
            cube.GetComponent<MeshRenderer>().material = mat;
            cube.GetComponent<Collider>().enabled = false;
            mat.mainTexture = sprite.texture;
            cube.AddComponent<OneWayMaterialPropertyScaleBinder>();

            Vector3 endPos;
            var originalPos = options.position;
            cube.transform.localPosition = originalPos;
            endPos = cube.transform.localPosition + (options.direction ?? Vector3.forward) * options.distance;

            var tween = _tweeningManager.AddTween(new FloatTween(0f, 1f, v =>
            {
                var lerped = Vector3.Lerp(originalPos, endPos, v);
                cube.transform.localPosition = Vector3.Lerp(originalPos, endPos, v);
                cube.transform.localScale = Vector3.one * (1f - v);
            }, options.duration, EaseType.OutSine), cube);
            tween.onCompleted += () =>
            {
                UnityEngine.Object.Destroy(cube);
                UnityEngine.Object.Destroy(mat);
            };
        });
    }
}