using UnityEngine;

public class Lighting : MonoBehaviour
{
    // Start is called once before the first execution of Update after the MonoBehaviour is created
    [SerializeField] public Light light1;
    [SerializeField] public Light light2;
    [SerializeField] public Light light3;

    private void Start()
    {
        var shadowCullDistances = new float[32];
        light1.layerShadowCullDistances = light2.layerShadowCullDistances = shadowCullDistances;

        light1.color = light2.color = Color.HSVToRGB(0.1f, 0.3f, 1.0f);
        RenderSettings.ambientLight = Color.HSVToRGB(0.2f, 0.1f, 0.1f);

        light3.color = Color.HSVToRGB(0.1f, 0.2f, 0.9f);
    }
}