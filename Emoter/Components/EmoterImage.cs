using BeatSaberMarkupLanguage;
using BeatSaberMarkupLanguage.Components;
using System.Linq;
using UnityEngine;
using UnityEngine.UI;
using Zenject;

namespace Emoter.Components;

internal class EmoterImage : ClickableImage
{
    private LayoutElement _layoutElement = null!;
    private static readonly float _imageSize = 8f;

    protected override void Awake()
    {
        base.Awake();
        material = RoundEdgeMaterial;
        sprite = Utilities.ImageResources.BlankSprite;
        _layoutElement = GetOrAddComponent<LayoutElement>();
        _layoutElement.preferredHeight = _imageSize;
        _layoutElement.preferredWidth = _imageSize;
        _layoutElement.flexibleHeight = -1;
        _layoutElement.flexibleWidth = -1;
        _layoutElement.minHeight = -1;
        _layoutElement.minWidth = -1;

        GetOrAddComponent<RectTransform>().sizeDelta = new Vector2(_imageSize, _imageSize);
    }

    private T GetOrAddComponent<T>() where T : Component
    {
        var component = GetComponent<T>();
        if (component == null)
            component = gameObject.AddComponent<T>();
        return component;
    }

    private static Material _roundEdgeMaterial = null!;
    public static Material RoundEdgeMaterial
    {
        get
        {
            if (_roundEdgeMaterial == null)
                _roundEdgeMaterial = Resources.FindObjectsOfTypeAll<Material>().First(m => m.name == "UINoGlowRoundEdge");
            return _roundEdgeMaterial;
        }
    }

    public class Pool : MonoMemoryPool<EmoterImage>
    {
        public const string Id = $"{nameof(Emoter)}.{nameof(EmoterImage)}";
    }
}