using UnityEngine;

namespace PlanetarySystem
{
    public class SolarSystem : MonoBehaviour
    {
        private static readonly int ColorA = Shader.PropertyToID("_ColorA");
        private static readonly int ColorB = Shader.PropertyToID("_ColorB");

        [SerializeField] private ParticleSystem starsBackground;
        [SerializeField] private Transform player;
        [SerializeField] public GameObject sun;
        [SerializeField] public Material skyMaterial;
        [SerializeField] public Transform starBackground;
        [SerializeField] public GameObject planetPrefab;
        [SerializeField] public PlanetGenerationSettings planetGenerationSettings;
        [SerializeField] public Planet.Planet[] planets;

        [Header("Planet Generation")]
        [SerializeField] public int planetAmount = 5;
        [SerializeField] public int seed = 12345;
        [SerializeField] public float minOrbitRadius = 800f;
        [SerializeField] public float orbitRadiusIncrement = 100f;

        private PlanetGenerator _planetGenerator;

        private void Awake()
        {
            skyMaterial.SetColor(ColorA, Color.HSVToRGB(0.39f, 0.24f, 0.29f));
            skyMaterial.SetColor(ColorB, Color.HSVToRGB(0.5f, 0.27f, 0.3f));
            RenderSettings.fogColor = Color.HSVToRGB(0.5f, 0.35f, 0.47f);

            _planetGenerator = new PlanetGenerator(planetGenerationSettings, seed);
        }

        private void Start()
        {
            starsBackground.transform.parent = player;
            starsBackground.Play();

            GeneratePlanetSystem();
        }

        private void LateUpdate()
        {
            // starsBackground.transform.position = player.position;
        }

        private void GeneratePlanetSystem()
        {
            planets = new Planet.Planet[planetAmount];

            for (var i = 0; i < planetAmount; i++)
            {
                var orbitRadius = minOrbitRadius + i * orbitRadiusIncrement;
                var angle = Random.Range(0f, 360f);

                var planetObj = Instantiate(planetPrefab, transform);
                planetObj.name = $"Planet {i}";
                var planet = planetObj.GetComponent<Planet.Planet>();

                if (!planet)
                {
                    planet = planetObj.AddComponent<Planet.Planet>();
                }
                _planetGenerator.GeneratePlanet(planet);

                var position = new Vector3(
                    Mathf.Cos(angle * Mathf.Deg2Rad) * orbitRadius,
                    Random.Range(-10f, 10f),
                    Mathf.Sin(angle * Mathf.Deg2Rad) * orbitRadius
                );

                planetObj.transform.position = position + sun.transform.position;

                planets[i] = planet;
            }
        }
    }
}