using UnityEngine;

namespace PlanetarySystem.Planet
{
    public interface INoiseFilter
    {
        float Evaluate(Vector3 point);
    }
}