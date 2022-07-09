using UnityEngine;

namespace Emoter.Models;

internal struct EmoteDisplayOptions
{
    public readonly float duration;
    public readonly float distance;
    public readonly Vector3 position;
    public readonly Vector3? direction;

    public EmoteDisplayOptions(float duration, float distance, Vector3 position, Vector3? direction)
    {
        this.duration = duration;
        this.distance = distance;
        this.position = position;
        this.direction = direction;
    }
}