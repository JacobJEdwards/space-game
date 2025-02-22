using UnityEngine;

[CreateAssetMenu(fileName = "OxygenConfig", menuName = "Scriptable Objects/OxygenConfig")]
public class OxygenConfig : ScriptableObject
{
    [Header("Oxygen Settings")] [SerializeField]
    private float maxOxygen;

    [SerializeField] private float oxygenRegenRate;

    public float MaxOxygen => maxOxygen;
    public float OxygenRegenRate => oxygenRegenRate;
}