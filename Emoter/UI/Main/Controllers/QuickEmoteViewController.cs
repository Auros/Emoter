using BeatSaberMarkupLanguage.Attributes;
using BeatSaberMarkupLanguage.ViewControllers;
using HMUI;
using IPA.Utilities;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using UnityEngine.UI;

namespace Emoter.UI.Main.Controllers;

[ViewDefinition("Emoter.UI.Main.Views.quick-emote-view.bsml")]
[HotReload(RelativePathToLayout = @"..\Views\quick-emote-view.bsml")]
internal class QuickEmoteViewController : BSMLAutomaticViewController
{
    [UIValue("categories")]
    protected List<object> Categories = new()
    {
        "Favorites",
        "Built In"
    };

    [UIComponent("left-nav")]
    protected Button _leftNav = null!;

    [UIComponent("right-nav")]
    protected Button _rightNav = null!;

    [UIComponent("grid")]
    private RectTransform _grid = null!;

    [UIAction("#post-parse")]
    protected void Parsed()
    {
        SetSkew(_leftNav.gameObject);
        SetSkew(_rightNav.gameObject);

        var roundEdgeMat = Resources.FindObjectsOfTypeAll<Material>().First(m => m.name == "UINoGlowRoundEdge");
        var images = _grid.GetComponentsInChildren<Image>();
        foreach (var image in images)
            image.material = roundEdgeMat;
    }

    private void SetSkew(GameObject go)
    {
        var imageViews = go.GetComponentsInChildren<ImageView>();
        foreach (var imageView in imageViews)
        {
            imageView.SetField("_skew", 0f);
            imageView.SetAllDirty();
        }    
    }
}