#nullable enable

using System;
using UnityEngine;

namespace PlanetarySystem.Planet
{
    [CreateAssetMenu]
    public class ShapeSettings : ScriptableObject
    {
        public float planetRadius = 1;
        public NoiseLayer[] noiseLayers = Array.Empty<NoiseLayer>();

        [Serializable]
        public class NoiseLayer
        {
            public bool enabled = true;
            public bool useFirstLayerAsMask;
            public NoiseSettings noiseSettings = new();
        }
    }
}