#nullable enable

using System.Collections;
using System.Collections.Generic;
using System.Linq;
using Player;
using Unity.Cinemachine;
using UnityEngine;
using UnityEngine.Events;

[RequireComponent(typeof(Health))]
public class Oxygen : MonoBehaviour
{
    [Header("Config")] [SerializeField] public OxygenConfig config = null!;

    public UnityEvent<float> onOxygenChanged = new();

    [SerializeField] private CinemachineCamera playerCamera = null!;

    private readonly List<PlayerOxygenUpgrade> _appliedUpgrades = new();

    private Health _health = null!;
    private bool _isDamaging;
    public float MaxOxygen => config.MaxOxygen;


    public float CurrentOxygen { get; private set; }

    public void Reset()
    {
        CurrentOxygen = config.MaxOxygen;
        onOxygenChanged.Invoke(CurrentOxygen);
    }

    public void Start()
    {
        _health = GetComponent<Health>();
        CurrentOxygen = config.MaxOxygen;
    }

    public void AddUpgrade(PlayerOxygenUpgrade upgrade)
    {
        _appliedUpgrades.Add(upgrade);
    }

    public float GetMaxOxygen()
    {
        return _appliedUpgrades.Aggregate(config.MaxOxygen, (current, upgrade) => current * upgrade.oxygenBonus);
    }

    public float GetConsumptionRate()
    {
        return _appliedUpgrades.Aggregate(config.OxygenConsumptionRate,
            (current, upgrade) => current * upgrade.oxygenConsumptionBonus);
    }

    public float GetRegenerationRate()
    {
        return _appliedUpgrades.Aggregate(config.OxygenRegenRate,
            (current, upgrade) => current * upgrade.oxygenRegenerationBonus);
    }

    public void TakeDamage(float damage)
    {
        var dam = damage * GetConsumptionRate();

        CurrentOxygen = Mathf.Clamp(CurrentOxygen - dam, 0, GetMaxOxygen());

        if (CurrentOxygen <= 0 && !_isDamaging) StartCoroutine(DamageHealth());

        onOxygenChanged.Invoke(CurrentOxygen);
    }

    private IEnumerator DamageHealth()
    {
        _isDamaging = true;

        while (CurrentOxygen <= 0)
        {
            yield return new WaitForSeconds(2f);

            _health.TakeDamage(20);
        }

        _isDamaging = false;
    }
}