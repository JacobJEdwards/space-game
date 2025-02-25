#nullable enable

using UnityEngine;
using UnityEngine.Assertions;

public class Lighting : MonoBehaviour
{
    // Start is called once before the first execution of Update after the MonoBehaviour is created
    [SerializeField] public Light light1 = null!;
    [SerializeField] public Light light2 = null!;
    [SerializeField] public Light light3 = null!;

    private void Start()
    {
        Assert.IsNotNull(light1, "Light1 is not set!");
        Assert.IsNotNull(light2, "Light2 is not set!");
        Assert.IsNotNull(light3, "Light3 is not set!");

        var shadowCullDistances = new float[32];
        light1.layerShadowCullDistances = light2.layerShadowCullDistances = shadowCullDistances;

        light1.color = light2.color = Color.HSVToRGB(0.1f, 0.3f, 1.0f);
        RenderSettings.ambientLight = Color.HSVToRGB(0.2f, 0.1f, 0.1f);

        light3.color = Color.HSVToRGB(0.1f, 0.2f, 0.9f);
    }
}