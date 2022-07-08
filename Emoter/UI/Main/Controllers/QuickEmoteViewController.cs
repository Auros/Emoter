using BeatSaberMarkupLanguage;
using BeatSaberMarkupLanguage.Attributes;
using BeatSaberMarkupLanguage.ViewControllers;
using Emoter.Components;
using Emoter.Models;
using Emoter.Services;
using IPA.Utilities.Async;
using SiraUtil.Logging;
using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Threading.Tasks;
using UnityEngine;
using UnityEngine.UI;
using Zenject;

namespace Emoter.UI.Main.Controllers;

[ViewDefinition("Emoter.UI.Main.Views.quick-emote-view.bsml")]
[HotReload(RelativePathToLayout = @"..\Views\quick-emote-view.bsml")]
internal class QuickEmoteViewController : BSMLAutomaticViewController
{
    private float _gridY;
    private int _lastSelectedPage;
    private bool _showContent = true;
    private Guid? _lastSelectedCategory;
    private bool _showCategoryList = true;
    private EmoteCategory _selectedCategory = null!;

    [Inject]
    protected readonly SiraLog _siraLog = null!;

    [Inject]
    protected readonly IEmoteService _emoteService = null!;

    [Inject(Id = EmoterImage.Pool.Id)]
    protected readonly EmoterImage.Pool _imageMemoryPool = null!;

    protected MemoryPoolContainer<EmoterImage>? _imagePoolContainer;

    [UIValue("categories")]
    protected List<object> Categories = new() { new EmoteCategory(), new EmoteCategory() };

    [UIComponent("grid")]
    private readonly RectTransform _grid = null!;

    [UIValue("hide-category-list")]
    protected bool HideCategoryList => !_showCategoryList;

    [UIValue("hide-content")]
    protected bool HideContent => !_showContent;

    [UIValue("show-content")]
    protected bool ShowContent
    {
        get => _showContent;
        set
        {
            _showContent = value;
            NotifyPropertyChanged(nameof(ShowContent));
            NotifyPropertyChanged(nameof(HideContent));
        }
    }

    [UIValue("show-category-list")]
    protected bool ShowCategoryList
    {
        get => _showCategoryList;
        set
        {
            _showCategoryList = value;
            NotifyPropertyChanged(nameof(ShowCategoryList));
            NotifyPropertyChanged(nameof(HideCategoryList));
        }
    }

    [UIValue("selected-category")]
    public EmoteCategory SelectedCategory
    {
        get => _selectedCategory;
        set
        {
            _selectedCategory = value;
            _lastSelectedCategory = _selectedCategory is not null ? _selectedCategory.Id : null;
            NotifyPropertyChanged(nameof(SelectedCategory));

            if (_selectedCategory != null)
                _ = Task.Run(() => LoadCategoryInfo(_selectedCategory));
        }
    }


    [UIValue("grid-y")]
    protected float GridY
    {
        get => _gridY;
        set
        {
            _gridY = value;
            NotifyPropertyChanged(nameof(GridY));
        }
    }

    [UIAction("#post-parse")]
    protected void Parsed()
    {
        var roundEdgeMat = Resources.FindObjectsOfTypeAll<Material>().First(m => m.name == "UINoGlowRoundEdge");
        var images = _grid.GetComponentsInChildren<Image>();
        foreach (var image in images)
            image.material = roundEdgeMat;

        var gridLayout = _grid.GetComponent<GridLayoutGroup>();
        gridLayout.constraint = GridLayoutGroup.Constraint.FixedColumnCount;
        gridLayout.constraintCount = 4;

        _ = Task.Run(LoadCategories);

        _imageMemoryPool.Clear();
        _imagePoolContainer = new(_imageMemoryPool);
    }

    protected override void DidActivate(bool firstActivation, bool addedToHierarchy, bool screenSystemEnabling)
    {
        base.DidActivate(firstActivation, addedToHierarchy, screenSystemEnabling);
        ShowCategoryList = false;
        ShowContent = false;
    }

    protected void Update()
    {
        if (SelectedCategory is not null && SelectedCategory.Emotes.Count > 0)
        {
            var emoteCount = SelectedCategory.Emotes.Count;
            var size = (4 - (emoteCount % 4) + emoteCount) / 4 * 8.25f;
            GridY = size;
            return;
        }
        GridY = default;
    }

    private async Task LoadCategories()
    {
        var categories = await _emoteService.GetCategoriesAsync();
        Categories.Clear();
        for (int i = 0; i < categories.Count; i++)
            Categories.Add(categories[i]);

        await UnityMainThreadTaskScheduler.Factory.StartNew(() =>
        {
            ShowCategoryList = categories.Count == 0;
        });

        if (categories.Count == 0)
            return;

        if (_lastSelectedCategory.HasValue)
        {
            var lastCategory = categories.FirstOrDefault(c => c.Id == _lastSelectedCategory.Value);
            if (lastCategory == null)
            {
                _lastSelectedPage = 0;
                _lastSelectedCategory = null;
            }
            else
            {
                _lastSelectedPage = categories.IndexOf(lastCategory);
                if (_lastSelectedPage == -1) // This should NEVER happen, but just in case.
                    _lastSelectedPage = 0;
            }
        }

        var category = categories[_lastSelectedPage];

        // If somoene doesn't have any favorites, we chose the next category over.
        if (category.Emotes.Count == 0 && category.Id == Plugin.FavoritesCategoryId && categories.Count >= 2)
            category = categories.FirstOrDefault(c => c.Id != category.Id) ?? category;

        await UnityMainThreadTaskScheduler.Factory.StartNew(() =>
        {
            ShowCategoryList = true;
            SelectedCategory = category;
        });
    }

    private async Task LoadCategoryInfo(EmoteCategory category)
    {
        await UnityMainThreadTaskScheduler.Factory.StartNew(() =>
        {
            if (_imagePoolContainer != null)
                foreach (var activeItem in _imagePoolContainer.activeItems.ToArray()) // We copy it to an array so we don't modify the enumerable we are iterating over 
                    _imagePoolContainer.Despawn(activeItem);

            ShowContent = false;
        });

        ConcurrentBag<(Emote, byte[])> loaded = new();
        var assembly = Assembly.GetExecutingAssembly();
        var result = Parallel.ForEach(category.Emotes, emote =>
        {
            // todo: move into service

            using var str = assembly.GetManifestResourceStream(emote.Source);
            using MemoryStream ms = new();
            str.CopyTo(ms);

            loaded.Add((emote, ms.ToArray()));
        });

        while (!result.IsCompleted)
            await Task.Yield();

        await UnityMainThreadTaskScheduler.Factory.StartNew(() =>
        {
            List<(Emote, Sprite)> loadedEmotes = new();
            foreach (var e in loaded)
            {
                var tex = Utilities.LoadTextureRaw(e.Item2);
                tex.wrapMode = TextureWrapMode.Clamp;
                var sprite = Sprite.Create(tex, new(0f, 0f, tex.width, tex.height), Vector2.zero, 100f, 0, SpriteMeshType.FullRect);
                loadedEmotes.Add((e.Item1, sprite));
                tex.name = e.Item1.Name;
            }

            loadedEmotes = loadedEmotes.OrderBy(le => category.Emotes.IndexOf(le.Item1)).ToList();

            if (_imagePoolContainer != null)
            {
                foreach (var emote in loadedEmotes)
                {
                    var image = _imagePoolContainer.Spawn();
                    image.sprite = emote.Item2;

                    image.transform.SetParent(_grid, false);
                    image.transform.localScale = Vector3.one;
                }
            }
            ShowContent = true;
        });
    }

    protected override void DidDeactivate(bool removedFromHierarchy, bool screenSystemDisabling)
    {
        ShowContent = true;
        ShowCategoryList = true;
        base.DidDeactivate(removedFromHierarchy, screenSystemDisabling);
    }
}