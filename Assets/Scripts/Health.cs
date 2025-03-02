#nullable enable

using System;
using DG.Tweening;
using Interfaces;
using Unity.Assertions;
using UnityEngine;
using UnityEngine.Events;

public class Health : MonoBehaviour, IDamageable
{
    [Header("Config")] [SerializeField] public HealthConfig config = null!;

    public UnityEvent<float> onHealthChanged = new();
    public UnityEvent onDeath = new();
    public UnityEvent onDamage = new();

    public float CurrentHealth { get; private set; }
    public float MaxHealth => config.MaxHealth;
    private float _timeSinceLastDamage;
    private Tweener? _tween;

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
        CurrentHealth = Mathf.Clamp(CurrentHealth - damage, 0, config.MaxHealth);
        _timeSinceLastDamage = 0;
        onHealthChanged.Invoke(CurrentHealth);
        onDamage.Invoke();

        _tween ??= transform.DOShakePosition(0.5f, 0.5f, 10, 90, false).OnComplete(() => _tween = null)
            .OnKill(() => _tween = null);
    }

    public void Heal(float healAmount)
    {
        CurrentHealth = Mathf.Clamp(CurrentHealth + healAmount, 0, config.MaxHealth);
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