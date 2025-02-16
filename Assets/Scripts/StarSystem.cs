using System;
using UnityEngine;

public class StarSystem : MonoBehaviour
{
    private static readonly int ColorA = Shader.PropertyToID("_ColorA");
    private static readonly int ColorB = Shader.PropertyToID("_ColorB");

    // Start is called once before the first execution of Update after the MonoBehaviour is created
    [SerializeField] private ParticleSystem starsBackground;
    [SerializeField] private Transform player;
    [SerializeField] public GameObject sun;
    [SerializeField] public Material skyMaterial;
    [SerializeField] public Transform starBackground;

    private void Awake()
    {
        skyMaterial.SetColor(ColorA, Color.HSVToRGB(0.39f, 0.24f, 0.29f));
        skyMaterial.SetColor(ColorB, Color.HSVToRGB(0.5f, 0.27f, 0.3f));
        RenderSettings.fogColor = Color.HSVToRGB(0.5f, 0.35f, 0.47f);
    }

    void Start()
    {
        starsBackground.transform.position = player.position;
        starsBackground.Play();
    }

    // Update is called once per frame
    private void LateUpdate()
    {
        starsBackground.transform.position = player.position;
    }
}