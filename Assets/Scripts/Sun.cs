using UnityEngine;

public class Sun : MonoBehaviour
{
    [SerializeField] public GameObject sunBody;
    [SerializeField]public ParticleSystem sunCrown;
    [SerializeField]public ParticleSystem sunPlasma;
    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Start()
    {

        Color sunParticleColor = Color.HSVToRGB(0.4f, 1, 0.1f);
        ParticleSystem.MainModule sunCrownMain = sunCrown.main;
        sunCrownMain.startColor = new ParticleSystem.MinMaxGradient(new Color(sunParticleColor.r, sunParticleColor.g, sunParticleColor.b, 0.95f));
        ParticleSystem.MainModule sunPlasmaMain = sunPlasma.main;
        sunPlasmaMain.startColor = new ParticleSystem.MinMaxGradient(new Color(sunParticleColor.r * 9, sunParticleColor.g * 9, sunParticleColor.b * 9, 0.7f));

        
    }

    // Update is called once per frame
    private void FixedUpdate()
    {
        transform.Rotate(Vector3.up * 0.02f);
    }
}