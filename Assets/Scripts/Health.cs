#nullable enable

using System;
using System.Collections.Generic;
using System.Linq;
using DG.Tweening;
using Interfaces;
using Player.Upgrades;
using Unity.Assertions;
using UnityEngine;
using UnityEngine.Events;

public class Health : MonoBehaviour, IDamageable
{
    [Header("Config")] [SerializeField] public HealthConfig config = null!;

    public UnityEvent<float> onHealthChanged = new();
    public UnityEvent onDeath = new();
    public UnityEvent onDamage = new();

    private readonly List<PlayerHealthUpgrade> _appliedUpgrades = new();
    private float _timeSinceLastDamage;
    private Tweener? _tween;

    public float CurrentHealth { get; private set; }
    public float MaxHealth => GetMaxHealth();

    public void Reset()
    {
        CurrentHealth = config.MaxHealth;
        onHealthChanged.Invoke(CurrentHealth);
    }

    private void Start()
    {
        Assert.IsNotNull(config, "Health config is not set!");
        CurrentHealth = config.MaxHealth;
    }

    private void FixedUpdate()
    {
        if (CurrentHealth <= 0) Die();

        if (_timeSinceLastDamage >= config.TimeToHeal)
        {
            CurrentHealth = Mathf.Clamp(CurrentHealth + config.HealRate, 0, config.MaxHealth);
            _timeSinceLastDamage = 0;
        }
        else
        {
            _timeSinceLastDamage += Time.deltaTime;
        }
    }

    public void TakeDamage(float damage)
    {
        CurrentHealth = Mathf.Clamp(CurrentHealth - damage, 0, GetMaxHealth());
        _timeSinceLastDamage = 0;
        onHealthChanged.Invoke(CurrentHealth);
        onDamage.Invoke();

        _tween ??= transform.DOShakePosition(0.5f, 0.5f).OnComplete(() => _tween = null)
            .OnKill(() => _tween = null);
    }

    public void AddUpgrade(PlayerHealthUpgrade upgrade)
    {
        _appliedUpgrades.Add(upgrade);
    }

    private float GetMaxHealth()
    {
        return _appliedUpgrades.Aggregate(config.MaxHealth, (current, upgrade) => current * upgrade.healthBonus);
    }

    public void Heal(float healAmount)
    {
        var heal = _appliedUpgrades.Aggregate(healAmount,
            (current, upgrade) => current * upgrade.healthRegenerationBonus);
        CurrentHealth = Mathf.Clamp(CurrentHealth + heal, 0, GetMaxHealth());
        onHealthChanged.Invoke(CurrentHealth);
    }

    private void Die()
    {
        _tween?.Kill();
        onDeath.Invoke();
    }

    [Serializable]
    public class HealthConfigg
    {
        [Header("Health Settings")] [SerializeField]
        private float maxHealth = 100f;

        [SerializeField] private float healRate = 1f;

        [SerializeField] private float timeToHeal = 10f;

        public float MaxHealth => maxHealth;
        public float HealRate => healRate;
        public float TimeToHeal => timeToHeal;
    }
}