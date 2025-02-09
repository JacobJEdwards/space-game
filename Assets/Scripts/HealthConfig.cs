using UnityEngine;

[CreateAssetMenu(fileName = "HealthConfig", menuName = "Scriptable Objects/HealthConfig")]
public class HealthConfig : ScriptableObject
{
    [Header("Health Settings")] [SerializeField]
    private float maxHealth = 100f;

    [SerializeField]
    private float healRate = 1f;

    [SerializeField]
    private float timeToHeal = 10f;


    public float MaxHealth => maxHealth;
    public float HealRate => healRate;
    public float TimeToHeal => timeToHeal;
}
