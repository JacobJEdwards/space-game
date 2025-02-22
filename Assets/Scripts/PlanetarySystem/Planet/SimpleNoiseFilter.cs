using UnityEngine;

namespace PlanetarySystem.Planet
{
    public class SimpleNoiseFilter : INoiseFilter
    {
        private readonly Noise _noise = new();
        private readonly NoiseSettings.SimpleNoiseSettings _settings;

        public SimpleNoiseFilter(NoiseSettings.SimpleNoiseSettings settings)
        {
            _settings = settings;
        }

        public float Evaluate(Vector3 point)
        {
            float noiseValue = 0;
            var frequency = _settings.baseRoughness;
            float amplitude = 1;

            for (var i = 0; i < _settings.numLayers; i++)
            {
                var v = _noise.Evaluate(point * frequency + _settings.centre);
                noiseValue += (v + 1) * .5f * amplitude;
                frequency *= _settings.roughness;
                amplitude *= _settings.persistence;
            }

            noiseValue -= _settings.minValue;
            return noiseValue * _settings.strength;
        }
    }
}