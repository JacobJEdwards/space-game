using UnityEngine;

namespace PlanetarySystem.Planet
{
    public class RidgidNoiseFilter : INoiseFilter
    {
        private readonly Noise _noise = new();
        private readonly NoiseSettings.RidgidNoiseSettings _settings;

        public RidgidNoiseFilter(NoiseSettings.RidgidNoiseSettings settings)
        {
            _settings = settings;
        }

        public float Evaluate(Vector3 point)
        {
            float noiseValue = 0;
            var frequency = _settings.baseRoughness;
            float amplitude = 1;
            float weight = 1;

            for (var i = 0; i < _settings.numLayers; i++)
            {
                var v = 1 - Mathf.Abs(_noise.Evaluate(point * frequency + _settings.centre));
                v *= v;
                v *= weight;
                weight = Mathf.Clamp01(v * _settings.weightMultiplier);

                noiseValue += v * amplitude;
                frequency *= _settings.roughness;
                amplitude *= _settings.persistence;
            }

            noiseValue = noiseValue - _settings.minValue;
            return noiseValue * _settings.strength;
        }
    }
}