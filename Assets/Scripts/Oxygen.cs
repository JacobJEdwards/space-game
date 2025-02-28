#nullable enable

using Unity.Assertions;
using UnityEngine;
using UnityEngine.Events;

public class Oxygen : MonoBehaviour
{
    [Header("Config")]
    [SerializeField] public OxygenConfig config = null!;

    public UnityEvent<float> onOxygenChanged = new();
    public float MaxOxygen => config.MaxOxygen;

    public float CurrentOxygen { get; private set; }

    public void Reset()
    {
        Assert.IsNotNull(config, "Config is null");

        CurrentOxygen = config.MaxOxygen;
        onOxygenChanged.Invoke(CurrentOxygen);
    }

    public void Start()
    {
        Assert.IsNotNull(config, "Config is not set!");
        CurrentOxygen = config.MaxOxygen;
    }

    public void TakeDamage(float damage)
    {
        CurrentOxygen = Mathf.Clamp(CurrentOxygen - damage, 0, config.MaxOxygen);
        onOxygenChanged.Invoke(CurrentOxygen);
    }
}