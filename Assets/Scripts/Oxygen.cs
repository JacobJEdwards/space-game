using System;
using UnityEngine;
using UnityEngine.Events;
using UnityEngine.Serialization;

public class Oxygen : MonoBehaviour
{
    public class OxygenConfigg
    {
        public float MaxOxygen = 100;
        public float OxygenRegenRate = 1;
    }

    [Header("Config")]
    [SerializeField]
    public OxygenConfig config;

    public UnityEvent<float> onOxygenChanged;

    public float CurrentOxygen { get; private set; }

    public void Start()
    {
        CurrentOxygen = config.MaxOxygen;
    }

    public void Reset()
    {
        CurrentOxygen = config.MaxOxygen;
        onOxygenChanged.Invoke(CurrentOxygen);
    }

    public void TakeDamage(float damage)
    {
        CurrentOxygen = Mathf.Clamp(CurrentOxygen - damage, 0, config.MaxOxygen);
        onOxygenChanged.Invoke(CurrentOxygen);
    }
}