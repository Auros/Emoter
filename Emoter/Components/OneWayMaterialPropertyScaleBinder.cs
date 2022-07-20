using UnityEngine;

namespace Emoter.Components;

internal class OneWayMaterialPropertyScaleBinder : MonoBehaviour
{
    private Material _material = null!;
    private static readonly string _scalePropertyName = "_Scale";

    public void SetMaterial(Material material)
    {
        _material = material;
    }

    protected void Update()
    {
        var scale = _material.GetFloat(_scalePropertyName);
        if (scale != transform.localScale.x)
            _material.SetFloat(_scalePropertyName, transform.localScale.x);
    }
}