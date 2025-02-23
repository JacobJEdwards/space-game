using System;
using PlanetarySystem.Planet;
using Unity.VisualScripting.Dependencies.NCalc;
using UnityEngine;

namespace PlanetarySystem
{
    public enum Biome
    {
        Ocean,
        Volcano,
        Ice,
        Desert,
        Forest
    }

    [System.Serializable]
    public class PlanetGenerationSettings
    {
        public float minRadius = 50f;
        public float maxRadius = 120f;
        public float minNoiseScale = 0.3f;
        public float maxNoiseScale = 2f;
        public float waterChance = 0.5f;
        public Material planetMaterial;
    }

    public class PlanetGenerator
    {
        private readonly PlanetGenerationSettings _settings;
        private readonly System.Random _random;
        private readonly Transform _player;

        public PlanetGenerator(PlanetGenerationSettings settings, int seed, Transform player)
        {
            _settings = settings;
            _player = player;
            _random = new System.Random(seed);
        }

        public void GeneratePlanet(Planet.Planet planet)
        {
            var shapeSettings = ScriptableObject.CreateInstance<ShapeSettings>();
            shapeSettings.noiseLayers = new ShapeSettings.NoiseLayer[2];

            shapeSettings.planetRadius = GetRandomFloat(_settings.minRadius, _settings.maxRadius);
            shapeSettings.noiseLayers[0] = GenerateBaseNoiseLayer();
            shapeSettings.noiseLayers[1] = GenerateDetailNoiseLayer();

            var colourSettings = ScriptableObject.CreateInstance<ColourSettings>();
            colourSettings.oceanColour = CreateGradient(Color.blue, Color.cyan, Color.white);
            colourSettings.planetMaterial = new Material(_settings.planetMaterial);

            var biome = GetBiome();
            planet.biome = biome;
            SetPlanetBiome(colourSettings, biome);

            colourSettings.biomeColourSettings.noise = new NoiseSettings
            {
                filterType = NoiseSettings.FilterType.Simple,
                simpleNoiseSettings = new NoiseSettings.SimpleNoiseSettings
                {
                    strength = 1,
                    baseRoughness = 1,
                    numLayers = 1,
                    centre = Vector3.zero,
                    roughness = 1,
                    persistence = 0.5f,
                    minValue = 0.8f
                }
            };

            planet.playerTransform = _player;
            planet.shapeSettings = shapeSettings;
            planet.colourSettings = colourSettings;

            planet.hasWater = _random.NextDouble() < _settings.waterChance;
            planet.numRocks = _random.Next(0, 40);
            planet.hasLife = _random.NextDouble() < 0.5f;

            planet.GeneratePlanet();
        }

        private ShapeSettings.NoiseLayer GenerateBaseNoiseLayer()
        {
            var settings = new NoiseSettings
            {
                filterType = NoiseSettings.FilterType.Simple,
                simpleNoiseSettings = new NoiseSettings.SimpleNoiseSettings
                {
                    strength = GetRandomFloat(0.8f, 1.2f),
                    numLayers = _random.Next(1, 5),
                    baseRoughness = GetRandomFloat(_settings.minNoiseScale, _settings.maxNoiseScale),
                    roughness = GetRandomFloat(1.5f, 3f),
                    persistence = GetRandomFloat(0.3f, 0.6f),
                    centre = Vector3.zero,
                    minValue = 0.8f
                }
            };

            return new ShapeSettings.NoiseLayer
            {
                enabled = true,
                noiseSettings = settings
            };
        }

        private ShapeSettings.NoiseLayer GenerateDetailNoiseLayer()
        {
            var settings = new NoiseSettings
            {
                filterType = NoiseSettings.FilterType.Simple,
                simpleNoiseSettings = new NoiseSettings.RidgidNoiseSettings
                {
                    strength = GetRandomFloat(0.1f, 0.3f),
                    numLayers = _random.Next(1, 5),
                    baseRoughness = GetRandomFloat(0.1f, 0.3f),
                    roughness = GetRandomFloat(1.5f, 3f),
                    persistence = GetRandomFloat(0.3f, 0.6f),
                    centre = Vector3.zero,
                    minValue = 0.8f
                }
            };

            return new ShapeSettings.NoiseLayer
            {
                enabled = true,
                noiseSettings = settings,
                useFirstLayerAsMask = true
            };
        }

        private Biome GetBiome()
        {
            var biomeRoll = _random.Next(0, Enum.GetValues(typeof(Biome)).Length);
            return (Biome)biomeRoll;
        }

        private void SetPlanetBiome(ColourSettings colourSettings, Biome biome = Biome.Forest)
        {
            colourSettings.biomeColourSettings = biome switch
            {
                Biome.Volcano => new ColourSettings.BiomeColourSettings
                {
                    biomes = new ColourSettings.BiomeColourSettings.Biome[]
                    {
                        new()
                        {
                            gradient = CreateGradient(Color.black, Color.red, Color.yellow),
                            startHeight = 0.1f,
                            tint = Color.red,
                            tintPercent = 0.5f
                        }
                    }
                },
                Biome.Ice => new ColourSettings.BiomeColourSettings
                {
                    biomes = new ColourSettings.BiomeColourSettings.Biome[]
                    {
                        new()
                        {
                            gradient = CreateGradient(Color.white, Color.blue, Color.cyan),
                            startHeight = 0.1f,
                            tint = Color.white,
                            tintPercent = 0.5f
                        }
                    }
                },
                Biome.Desert => new ColourSettings.BiomeColourSettings
                {
                    biomes = new ColourSettings.BiomeColourSettings.Biome[]
                    {
                        new()
                        {
                            gradient = CreateGradient(Color.yellow, Color.red, Color.black),
                            startHeight = 0.1f,
                            tint = Color.yellow,
                            tintPercent = 0.5f
                        }
                    }
                },
                Biome.Forest => new ColourSettings.BiomeColourSettings
                {
                    biomes = new ColourSettings.BiomeColourSettings.Biome[]
                    {
                        new()
                        {
                            gradient = CreateGradient(Color.green, Color.blue, Color.white),
                            startHeight = 0.1f,
                            tint = Color.green,
                            tintPercent = 0.5f
                        }
                    }
                },
                Biome.Ocean => new ColourSettings.BiomeColourSettings
                {
                    biomes = new ColourSettings.BiomeColourSettings.Biome[]
                    {
                        new()
                        {
                            gradient = CreateGradient(Color.blue, Color.cyan, Color.white),
                            startHeight = 0.1f,
                            tint = Color.blue,
                            tintPercent = 0.5f
                        }
                    }
                },
                _ => colourSettings.biomeColourSettings
            };
        }

        private float GetRandomFloat(float min, float max)
        {
            return (float)_random.NextDouble() * (max - min) + min;
        }

        private static Gradient CreateGradient(Color lowColor, Color midColor, Color highColor)
        {
            var gradient = new Gradient();
            var colorKeys = new GradientColorKey[3];
            var alphaKeys = new GradientAlphaKey[2];

            colorKeys[0].color = lowColor;
            colorKeys[0].time = 0f;
            colorKeys[1].color = midColor;
            colorKeys[1].time = 0.5f;
            colorKeys[2].color = highColor;
            colorKeys[2].time = 1f;

            alphaKeys[0].alpha = 1f;
            alphaKeys[0].time = 0f;
            alphaKeys[1].alpha = 1f;
            alphaKeys[1].time = 1f;

            gradient.SetKeys(colorKeys, alphaKeys);
            return gradient;
        }
    }
}