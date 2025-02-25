#nullable enable

using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Assertions;
using Random = UnityEngine.Random;

namespace PlanetarySystem
{
    public class SolarSystem : MonoBehaviour
    {
        private static readonly int ColorA = Shader.PropertyToID("_ColorA");
        private static readonly int ColorB = Shader.PropertyToID("_ColorB");

        [SerializeField] private ParticleSystem starsBackground = null!;
        [SerializeField] private Transform player = null!;
        [SerializeField] public GameObject sun = null!;
        [SerializeField] public Material skyMaterial = null!;
        [SerializeField] public Transform starBackground = null!;
        [SerializeField] public GameObject planetPrefab = null!;
        [SerializeField] public PlanetGenerationSettings planetGenerationSettings = null!;
        [SerializeField] public List<Planet.Planet> planets = new();

        [Header("Planet Generation")]
        [SerializeField] public bool randomisePosition = true;
        [SerializeField] public int planetAmount = 5;
        [SerializeField] public int seed = 12345;
        [SerializeField] public float minOrbitRadius = 800f;
        [SerializeField] public float orbitRadiusIncrement = 100f;

        private PlanetGenerator _planetGenerator = null!;

        private void Awake()
        {
            Assert.IsNotNull(starsBackground);
            Assert.IsNotNull(player);
            Assert.IsNotNull(sun);
            Assert.IsNotNull(skyMaterial);
            Assert.IsNotNull(starBackground);
            Assert.IsNotNull(planetPrefab);
            Assert.IsNotNull(planetGenerationSettings);

            skyMaterial.SetColor(ColorA, Color.HSVToRGB(0.39f, 0.24f, 0.29f));
            skyMaterial.SetColor(ColorB, Color.HSVToRGB(0.5f, 0.27f, 0.3f));
            RenderSettings.fogColor = Color.HSVToRGB(0.5f, 0.35f, 0.47f);

            _planetGenerator = new PlanetGenerator(planetGenerationSettings, seed);
        }

        private void Start()
        {
            starsBackground.Play();

            GeneratePlanetSystem();
        }

        private void LateUpdate()
        {
            starsBackground.transform.position = player.position;
        }

        private void GeneratePlanetSystem()
        {
            for (var i = 0; i < planetAmount; i++)
            {
                var orbitRadius = minOrbitRadius + i * orbitRadiusIncrement;
                var angle = randomisePosition ? Random.Range(0f, 360f) : i * 360f / planetAmount;

                var planetObj = Instantiate(planetPrefab, transform);

                planetObj.name = $"Planet {i}";
                var planet = planetObj.GetComponent<Planet.Planet>();

                if (!planet)
                {
                    planet = planetObj.AddComponent<Planet.Planet>();
                }

                _planetGenerator.GeneratePlanet(planet);

                var y = randomisePosition ? Random.Range(-10f, 10f) : 0f;

                var position = new Vector3(
                    Mathf.Cos(angle * Mathf.Deg2Rad) * orbitRadius,
                    y,
                    Mathf.Sin(angle * Mathf.Deg2Rad) * orbitRadius
                );

                planetObj.transform.position = position + (sun?.transform.position ?? Vector3.zero);

                planets.Add(planet);
            }
        }
    }
}