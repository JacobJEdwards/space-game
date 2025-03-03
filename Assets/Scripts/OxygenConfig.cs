#nullable enable

using UnityEngine;

[CreateAssetMenu(fileName = "OxygenConfig", menuName = "Scriptable Objects/OxygenConfig")]
public class OxygenConfig : ScriptableObject
{
    [Header("Oxygen Settings")] [SerializeField]
    private float maxOxygen;

    [SerializeField] private float oxygenRegenRate;
    [SerializeField] private float oxygenConsumptionRate = 1f;

    public float MaxOxygen => maxOxygen;
    public float OxygenRegenRate => oxygenRegenRate;
    public float OxygenConsumptionRate => oxygenConsumptionRate;
}