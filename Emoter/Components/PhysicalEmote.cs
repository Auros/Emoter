using Emoter.Services.Other;
using SiraUtil.Logging;
using System.Threading.Tasks;
using UnityEngine;
using Zenject;

namespace Emoter.Components;

internal class PhysicalEmote : MonoBehaviour
{
    public Material Material { get; internal set; } = null!;

    private MeshRenderer? _renderer;

    public void SetTexture(Texture2D texture)
    {
        Material.mainTexture = texture;
        if (_renderer == null)
            _renderer = GetComponent<MeshRenderer>();
        _renderer.material = Material;
    }

    protected void OnDestroy()
    {
        Destroy(Material);
    }

    public class Pool : MonoMemoryPool<PhysicalEmote>
    {
        private Shader? _shader;
        private readonly SiraLog _siraLog;
        private readonly IEmoterResourceService _emoterResourceService;

        public Pool(SiraLog siraLog, IEmoterResourceService emoterResourceService)
        {
            _siraLog = siraLog;
            _emoterResourceService = emoterResourceService;
            Task.Run(async () =>
            {
                _shader = await _emoterResourceService.LoadEmoteShaderAsync();
            });
        }

        protected override void OnCreated(PhysicalEmote item)
        {
            base.OnCreated(item);
            if (_shader == null)
            {
                _siraLog.Warn("Emote shader hasn't warmed up.");
                return;
            }

            item.gameObject.SetActive(true);
            item.Material = new(_shader);


            var owmpsb = item.gameObject.AddComponent<OneWayMaterialPropertyScaleBinder>();
            owmpsb.SetMaterial(item.Material);
        }
    }
}