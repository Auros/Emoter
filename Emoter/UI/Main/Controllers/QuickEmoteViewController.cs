using BeatSaberMarkupLanguage.Attributes;
using BeatSaberMarkupLanguage.ViewControllers;
using Emoter.Components;
using Emoter.Models;
using Emoter.Services;
using Emoter.Services.Other;
using Emoter.UI.Main.Contexts;
using IPA.Utilities;
using IPA.Utilities.Async;
using SiraUtil.Logging;
using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
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
    protected MemoryPoolContainer<EmoterImage>? _imagePoolContainer;
    private Signal _fixedSignal = null!;

    [Inject] protected readonly Config _config = null!;
    [Inject] protected readonly SiraLog _siraLog = null!;
    [Inject] protected readonly IEmoteService _emoteService = null!;
    [Inject] protected readonly MenuShockwave _menuShockwave = null!;
    [Inject] protected readonly IEmoteDispatcher _emoteDispatcher = null!;
    [Inject] protected readonly IFavoritesTracker _favoritesTracker = null!;
    [Inject] protected readonly IEmoterInputService _emoterInputService = null!;
    [Inject] protected readonly ISpriteSourceBuilder _spriteSourceBuilder = null!;
    [Inject(Id = EmoterImage.Pool.Id)] protected readonly EmoterImage.Pool _imageMemoryPool = null!;
    [Inject, UIValue("selected-emote-context")] protected readonly SelectedEmoteContext _selectedEmoteContext = null!;

    private static readonly FieldAccessor<Signal, Action>.Accessor Event = FieldAccessor<Signal, Action>.GetAccessor("_event");
    private static readonly FieldAccessor<SignalOnUIButtonClick, Signal>.Accessor Signal = FieldAccessor<SignalOnUIButtonClick, Signal>.GetAccessor("_buttonClickedSignal");

    #region BSML

    [UIValue("hide-content")] protected bool HideContent => !_showContent;
    [UIValue("hide-category-list")] protected bool HideCategoryList => !_showCategoryList;

    [UIComponent("grid")] protected readonly RectTransform _grid = null!;
    [UIValue("categories")] protected List<object> Categories = new() { new EmoteCategory() };

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

    [UIValue("duration")]
    protected float Duration
    {
        get => _config.Duration;
        set
        {
            _config.Duration = value;
            NotifyPropertyChanged(nameof(Duration));
        }
    }

    [UIValue("distance")]
    protected float Distance
    {
        get => _config.Distance;
        set
        {
            _config.Distance = value;
            NotifyPropertyChanged(nameof(Distance));
        }
    }


    #endregion

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

        // Disable the shockwave effect on the quick menu, as it's pretty annoying in VR
        var clickers = GetComponentsInChildren<SignalOnUIButtonClick>(true);
        if (_fixedSignal == null)
        {
            var firstClicker = clickers[0];
            _fixedSignal = Instantiate(Signal(ref firstClicker));
            Event(ref _fixedSignal) = Event(ref Signal(ref firstClicker));
            _fixedSignal.Unsubscribe(_menuShockwave.HandleButtonClickEvent);
        }

        foreach (var clicker in clickers)
        {
            var assignedClicker = clicker;
            Signal(ref assignedClicker) = _fixedSignal; 
        }
    }


    private void Image_EmoteClicked(EmoterImage _, Emote emote)
    {
        if (_emoterInputService.IsHoldingDownAlternateAction)
        {
            _selectedEmoteContext.SetData(emote);
            _selectedEmoteContext.Show();
        }
        else
        {
            _emoteDispatcher.Dispatch(emote);
        }
    }

    protected override void DidActivate(bool firstActivation, bool addedToHierarchy, bool screenSystemEnabling)
    {
        base.DidActivate(firstActivation, addedToHierarchy, screenSystemEnabling);
        ShowCategoryList = false;
        ShowContent = false;

        _favoritesTracker.FavoritesChanged += FavoritesTracker_FavoritesChanged;
    }

    private void FavoritesTracker_FavoritesChanged()
    {
        if (_selectedCategory != null && _selectedCategory.Id == Plugin.FavoritesCategoryId)
            Task.Run(() => LoadCategoryInfo(_selectedCategory));
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
                {
                    activeItem.EmoteClicked -= Image_EmoteClicked;
                    _imagePoolContainer.Despawn(activeItem);
                }

            ShowContent = false;
        });

        ConcurrentBag<(Emote, Sprite)> loadedBag = new();

        // https://stackoverflow.com/a/56860378
        var cd2 = Environment.ProcessorCount / 2;
        var max = cd2 > 4 ? cd2 : 4;

        using SemaphoreSlim semaphore = new(initialCount: max);
        var tasks = category.Emotes.Select(async emote =>
        {
            await semaphore.WaitAsync();
            try
            {
                var sprite = await _spriteSourceBuilder.BuildSpriteAsync(emote.Source);
                loadedBag.Add((emote, sprite));
            }
            finally
            {
                semaphore.Release();
            }
        });

        await Task.WhenAll(tasks);

        await UnityMainThreadTaskScheduler.Factory.StartNew(() =>
        {
            if (_imagePoolContainer is null)
                return;

            List<(Emote, Sprite)> loadedSprites = loadedBag.OrderBy(le => category.Emotes.IndexOf(le.Item1)).ToList();
            foreach (var emote in loadedSprites)
            {
                var image = _imagePoolContainer.Spawn();
                image.sprite = emote.Item2;
                image.SetData(emote.Item1);

                image.transform.SetParent(_grid, false);
                image.transform.localScale = Vector3.one;

                image.EmoteClicked += Image_EmoteClicked;
            }
            ShowContent = true;
        });
    }

    protected override void DidDeactivate(bool removedFromHierarchy, bool screenSystemDisabling)
    {
        _favoritesTracker.FavoritesChanged -= FavoritesTracker_FavoritesChanged;

        ShowContent = false;
        ShowCategoryList = false;
        base.DidDeactivate(removedFromHierarchy, screenSystemDisabling);
    }
}