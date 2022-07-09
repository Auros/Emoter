using BeatSaberMarkupLanguage.Attributes;
using BeatSaberMarkupLanguage.Parser;
using Emoter.Models;
using Emoter.Services;
using HMUI;
using IPA.Utilities.Async;
using SiraUtil.Attributes;
using SiraUtil.Zenject;
using System;
using System.ComponentModel;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Threading.Tasks;
using Tweening;
using UnityEngine;

namespace Emoter.UI.Main.Contexts;

[Bind(Location.Menu)]
internal class SelectedEmoteContext : INotifyPropertyChanged
{
    public event PropertyChangedEventHandler? PropertyChanged;
    [UIParams] protected readonly BSMLParserParams _parserParams = null!;
    [UIComponent("fav-button")] protected readonly NoTransitionsButton _favButton = null!;
    [UIComponent("selected-emote-image")] protected readonly ImageView _selectedEmoteImage = null!; 

    private bool _isFavorited;
    private Emote? _lastSelectedEmote = null;
    private string _selectedEmoteName = "Not set";

    private readonly IFavoritesTracker _favoritesTracker;
    private readonly TimeTweeningManager _timeTweeningManager;
    private readonly ISpriteSourceBuilder _spriteSourceBuilder;
    private static readonly Color _unfavColor = new(0.86f, 0.13f, 0.13f, 1f);

    private Color? _activeColor;
    private ImageView _favButtonBorder = null!;
    private ImageView _favButtonOutline = null!;
    private ImageView _favButtonBackground = null!;
    private Color _defaultFavButtonBorderColor = Color.blue;
    private Color _defaultFavButtonOutlineColor = Color.blue;
    private Color _defaultFavButtonBGColorLeftTop = Color.blue;
    private Color _defaultFavButtonBGColorLeftBottom = Color.blue;

    public SelectedEmoteContext(IFavoritesTracker favoritesTracker, TimeTweeningManager timeTweeningManager, ISpriteSourceBuilder spriteSourceBuilder)
    {
        _favoritesTracker = favoritesTracker;
        _timeTweeningManager = timeTweeningManager;
        _spriteSourceBuilder = spriteSourceBuilder;
    }

    protected bool SelectedIsFavorited
    {
        get => _isFavorited;
        set
        {
            _isFavorited = value;
            NotifyPropertyChanged(nameof(FavoritedText));
        }
    }

    [UIValue("selected-emote-name")]
    protected string SelectedEmoteName
    {
        get => _selectedEmoteName;
        set
        {
            _selectedEmoteName = value;
            NotifyPropertyChanged(nameof(SelectedEmoteName));
        }
    }

    [UIValue("favorited-text")]
    protected string FavoritedText => SelectedIsFavorited ? "Unfavorite" : "Favorite";

    public void Show()
    {
        _parserParams.EmitEvent("show-emote-info");
    }

    public void Hide()
    {
        _parserParams.EmitEvent("hide-emote-info");
    }

    public void SetData(Emote emote)
    {
        _lastSelectedEmote = emote;
        SelectedEmoteName = emote.Name;
        _ = Task.Run(SetupImage);
    }

    private async Task SetupImage()
    {
        if (_lastSelectedEmote is null)
            return;


        var sprite = await _spriteSourceBuilder.BuildSpriteAsync(_lastSelectedEmote.Source);
        await UnityMainThreadTaskScheduler.Factory.StartNew(() => { _selectedEmoteImage.sprite = sprite; });
        await UpdateFavoriteAsync();
    }

    [UIAction("toggle-favorite")]
    protected void ToggleFavorite()
    {
        _ = Task.Run(ToggleFavoriteAsync);
    }

    private async Task ToggleFavoriteAsync()
    {
        if (_lastSelectedEmote is null)
            return;

        var emote = _lastSelectedEmote;
        var isFavorited = await IsFavorited(_lastSelectedEmote.Id);
        Task favTask = isFavorited ? _favoritesTracker.RemoveAsync(emote) : _favoritesTracker.AddAsync(emote);
        await favTask;

        await UpdateFavoriteAsync();
    }

    private async Task<bool> IsFavorited(Guid id)
    {
        var favorites = await _favoritesTracker.GetFavoritesAsync();
        var favorite = favorites.FirstOrDefault(f => f.Id == id);
        return favorite != null;
    }

    private async Task UpdateFavoriteAsync()
    {
        if (_lastSelectedEmote is null)
            return;

        var isFavorited = await IsFavorited(_lastSelectedEmote.Id);
        await UnityMainThreadTaskScheduler.Factory.StartNew(() =>
        {
            if (_lastSelectedEmote is null)
                return;

            SelectedIsFavorited = isFavorited;
            SetFavButtonColor(isFavorited ? _unfavColor : null);
        });
    }

    private void NotifyPropertyChanged([CallerMemberName] string propertyName = "")
    {
        PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
    }

    public void SetFavButtonColor(Color? color)
    {
        if (_favButtonBorder == null)
        {
            _favButtonBorder = _favButton.transform.Find("Border").GetComponent<ImageView>();
            _favButtonBackground = _favButton.transform.Find("BG").GetComponent<ImageView>();
            _favButtonOutline = _favButton.transform.Find("OutlineWrapper/Outline").GetComponent<ImageView>();
            _defaultFavButtonBorderColor = _favButtonBorder.color;
            _defaultFavButtonOutlineColor = _favButtonOutline.color;
            _defaultFavButtonBGColorLeftTop = _favButtonBackground.color0;
            _defaultFavButtonBGColorLeftBottom = _favButtonBackground.color1;
        }

        if (!color.HasValue)
        {
            _activeColor = null;
            _favButtonBorder.color = _defaultFavButtonBorderColor;
            _favButtonOutline.color = _defaultFavButtonOutlineColor;
            _favButtonBackground.color0 = _defaultFavButtonBGColorLeftTop;
            _favButtonBackground.color1 = _defaultFavButtonBGColorLeftBottom;
            _favButton.GetComponent<ButtonStaticAnimations>().enabled = true;
        }
        else
        {
            _favButton.GetComponent<ButtonStaticAnimations>().enabled = false;
            _activeColor = color.Value;
            var activeColor = color.Value;

            _timeTweeningManager.AddTween(new ColorTween(_favButtonBackground.color0, activeColor, c =>
            {
                _favButtonBackground.color0 = c;
                _favButtonBorder.color = c.ColorWithAlpha(0.65f * c.a);
                _favButtonOutline.color = c.ColorWithAlpha(0.25f * c.a);
                _favButtonBackground.color1 = new Color(c.r * 0.5f, c.g * 0.5f, c.b * 0.5f, c.a);

                _favButtonBackground.SetVerticesDirty();
                _favButtonOutline.SetVerticesDirty();
                _favButtonBorder.SetVerticesDirty();

            }, 0.25f, EaseType.OutCubic, 0.025f), _favButtonBorder);
        }
    }
}