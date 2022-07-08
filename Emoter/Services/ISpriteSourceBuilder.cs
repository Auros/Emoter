using System.Threading.Tasks;
using UnityEngine;

namespace Emoter.Services;

internal interface ISpriteSourceBuilder
{
    Task<Sprite> BuildSpriteAsync(string source);
}