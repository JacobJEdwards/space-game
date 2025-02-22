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

        [SerializeField] public int planetAmount = 5;

        private void Awake()
        {
            skyMaterial.SetColor(ColorA, Color.HSVToRGB(0.39f, 0.24f, 0.29f));
            skyMaterial.SetColor(ColorB, Color.HSVToRGB(0.5f, 0.27f, 0.3f));
            RenderSettings.fogColor = Color.HSVToRGB(0.5f, 0.35f, 0.47f);
        }

        private void Start()
        {
            starsBackground.transform.position = player.position;
            starsBackground.Play();
        }

        private void LateUpdate()
        {
            starsBackground.transform.position = player.position;
        }

        private void GeneratePlanetSystem()
        {

        }
    }
}