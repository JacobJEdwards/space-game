using Interfaces;
using UnityEngine;
using UnityEngine.Events;

public class Health : MonoBehaviour, IDamageable
{
    [System.Serializable]
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

    [Header("Config")] [SerializeField] public HealthConfig config;

    private float _currentHealth;
    private float _timeSinceLastDamage;

    public UnityEvent<float> onHealthChanged;
    public UnityEvent onDeath;

    private void Start()
    {
        _currentHealth = config.MaxHealth;
    }

    private void FixedUpdate()
    {
        if (_currentHealth <= 0)
        {
            Die();
        }

        if (_timeSinceLastDamage >= config.TimeToHeal)
        {
            _currentHealth = Mathf.Clamp(_currentHealth + config.HealRate, 0, config.MaxHealth);
            _timeSinceLastDamage = 0;
        }
        else
        {
            _timeSinceLastDamage += Time.deltaTime;
        }
    }

    public void TakeDamage(float damage)
    {
        _currentHealth = Mathf.Clamp(_currentHealth - damage, 0, config.MaxHealth);
        _timeSinceLastDamage = 0;
        onHealthChanged.Invoke(_currentHealth);
    }

    public void Heal(float healAmount)
    {
        _currentHealth = Mathf.Clamp(_currentHealth + healAmount, 0, config.MaxHealth);
        onHealthChanged.Invoke(_currentHealth);
    }

    private void Die()
    {
        onDeath.Invoke();
    }

    public void Reset()
    {
        _currentHealth = config.MaxHealth;
    }
}