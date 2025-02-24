﻿using System;
using UnityEngine;

namespace PlanetarySystem.Planet
{
    [CreateAssetMenu]
    public class ColourSettings : ScriptableObject
    {
        public Material planetMaterial;
        public BiomeColourSettings biomeColourSettings;
        public Gradient oceanColour;

        [Serializable]
        public class BiomeColourSettings
        {
            public Biome[] biomes;
            public NoiseSettings noise;
            public float noiseOffset;
            public float noiseStrength;
            [Range(0, 1)] public float blendAmount;

            [Serializable]
            public class Biome
            {
                public Gradient gradient;
                public Color tint;
                [Range(0, 1)] public float startHeight;
                [Range(0, 1)] public float tintPercent;
            }
        }
    }
}