using UnityEngine;

namespace PlanetarySystem.Planet
{
    public class ShapeGenerator
    {
        private INoiseFilter[] _noiseFilters;
        private ShapeSettings _settings;
        public MinMax ElevationMinMax;

        public void UpdateSettings(ShapeSettings settings)
        {
            _settings = settings;
            _noiseFilters = new INoiseFilter[settings.noiseLayers.Length];
            for (var i = 0; i < _noiseFilters.Length; i++)
                _noiseFilters[i] = NoiseFilterFactory.CreateNoiseFilter(settings.noiseLayers[i].noiseSettings);

            ElevationMinMax = new MinMax();
        }

        public float CalculateUnscaledElevation(Vector3 pointOnUnitSphere)
        {
            float firstLayerValue = 0;
            float elevation = 0;

            if (_noiseFilters.Length > 0)
            {
                firstLayerValue = _noiseFilters[0].Evaluate(pointOnUnitSphere);
                if (_settings.noiseLayers[0].enabled) elevation = firstLayerValue;
            }

            for (var i = 1; i < _noiseFilters.Length; i++)
            {
                if (!_settings.noiseLayers[i].enabled) continue;

                var mask = _settings.noiseLayers[i].useFirstLayerAsMask ? firstLayerValue : 1;
                elevation += _noiseFilters[i].Evaluate(pointOnUnitSphere) * mask;
            }

            ElevationMinMax.AddValue(elevation);
            return elevation;
        }

        public float GetScaledElevation(float unscaledElevation)
        {
            var elevation = Mathf.Max(0, unscaledElevation);
            elevation = _settings.planetRadius * (1 + elevation);
            return elevation;
        }
    }
}