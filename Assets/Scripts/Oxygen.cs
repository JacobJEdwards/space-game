#nullable enable

using UnityEngine;
using UnityEngine.Events;

public class Oxygen : MonoBehaviour
{
    [Header("Config")] [SerializeField] public OxygenConfig? config;

    public UnityEvent<float> onOxygenChanged = new();

    public float CurrentOxygen { get; private set; }

    public void Reset()
    {
        if (!config)
        {
            Debug.LogError("Oxygen config is not set!");
            return;
        }

        CurrentOxygen = config.MaxOxygen;
        onOxygenChanged.Invoke(CurrentOxygen);
    }

    public void Start()
    {
        if (!config)
        {
            Debug.LogError("Oxygen config is not set!");
            return;
        }

        CurrentOxygen = config.MaxOxygen;
    }

    public void TakeDamage(float damage)
    {
        if (!config)
        {
            Debug.LogError("Oxygen config is not set!");
            return;
        }

        CurrentOxygen = Mathf.Clamp(CurrentOxygen - damage, 0, config.MaxOxygen);
        onOxygenChanged.Invoke(CurrentOxygen);
    }

    public class OxygenConfigg
    {
        public float MaxOxygen = 100;
        public float OxygenRegenRate = 1;
    }
}