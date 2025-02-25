#nullable enable

using System;
using UnityEngine;

namespace PlanetarySystem.Planet
{
    [CreateAssetMenu]
    public class ColourSettings : ScriptableObject
    {
        public Material planetMaterial = null!;
        public BiomeColourSettings biomeColourSettings = new();
        public Gradient oceanColour = null!;

        [Serializable]
        public class BiomeColourSettings
        {
            public Biome[] biomes = Array.Empty<Biome>();
            public NoiseSettings noise = null!;
            public float noiseOffset;
            public float noiseStrength;
            [Range(0, 1)] public float blendAmount;

            [Serializable]
            public class Biome
            {
                public Gradient gradient = null!;
                public Color tint;
                [Range(0, 1)] public float startHeight;
                [Range(0, 1)] public float tintPercent;
            }
        }
    }
}