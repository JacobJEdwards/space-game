#nullable enable

using UnityEngine;
using UnityEngine.Assertions;

namespace PlanetarySystem
{
public class Sun : MonoBehaviour
{
    [SerializeField] public ParticleSystem sunCrown = null!;
    [SerializeField] public ParticleSystem sunPlasma = null!;

    private void Start()
    {
        Assert.IsNotNull(sunCrown, "SunCrown is not set!");
        Assert.IsNotNull(sunPlasma, "SunPlasma is not set!");

        var sunParticleColor = Color.yellow;
        var sunCrownMain = sunCrown.main;
        sunCrownMain.startColor =
            new ParticleSystem.MinMaxGradient(new Color(sunParticleColor.r, sunParticleColor.g, sunParticleColor.b,
                0.95f));
        var sunPlasmaMain = sunPlasma.main;
        sunPlasmaMain.startColor = new ParticleSystem.MinMaxGradient(new Color(sunParticleColor.r * 9,
            sunParticleColor.g * 9, sunParticleColor.b * 9, 0.7f));
    }

    private void FixedUpdate()
    {
        transform.Rotate(Vector3.up * 0.02f);
    }
}
}
