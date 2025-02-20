using UnityEngine;

public class Sun : MonoBehaviour
{
    [SerializeField]public ParticleSystem sunCrown;
    [SerializeField]public ParticleSystem sunPlasma;

    private void Start()
    {

        var sunParticleColor = Color.HSVToRGB(0.4f, 1, 0.1f);
        var sunCrownMain = sunCrown.main;
        sunCrownMain.startColor = new ParticleSystem.MinMaxGradient(new Color(sunParticleColor.r, sunParticleColor.g, sunParticleColor.b, 0.95f));
        var sunPlasmaMain = sunPlasma.main;
        sunPlasmaMain.startColor = new ParticleSystem.MinMaxGradient(new Color(sunParticleColor.r * 9, sunParticleColor.g * 9, sunParticleColor.b * 9, 0.7f));
    }

    private void FixedUpdate()
    {
        transform.Rotate(Vector3.up * 0.02f);
    }
}