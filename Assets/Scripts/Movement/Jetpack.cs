using System.Collections.Generic;
using System.Linq;
using Interfaces;
using Managers;
using Player;
using Unity.Assertions;
using UnityEngine;

namespace Movement
{
    [RequireComponent(typeof(Rigidbody))]
    public class Jetpack : MonoBehaviour, IUpgradeable, IRepairable
    {
        [SerializeField] private JetpackSettings settings = null!;
        [SerializeField] private AudioSource jetpackAudioSource = null!;
        [SerializeField] private AudioClip jetpackClip = null!;
        [SerializeField] private AudioClip jetpackEmptyClip = null!;

        public bool isJetpacking;

        public List<JetpackUpgrade> appliedUpgrades = new();
        private bool _isRepaired;

        private float _jetpackFuel;
        private Rigidbody _rb = null!;

        private void Start()
        {
            _rb = GetComponent<Rigidbody>();

            Assert.IsNotNull(settings, "JetpackSettings is missing");
            Assert.IsNotNull(jetpackAudioSource, "JetpackAudioSource is missing");
            Assert.IsNotNull(jetpackClip, "JetpackClip is missing");
            Assert.IsNotNull(jetpackEmptyClip, "JetpackEmptyClip is missing");

            _jetpackFuel = FuelCapacity();
        }

        private void FixedUpdate()
        {
            if (!isJetpacking)
                _jetpackFuel = Mathf.Min(_jetpackFuel + FuelRegenerationRate() * Time.deltaTime,
                    FuelCapacity());
        }

        public bool CanApplyRepair(BaseRepair repair)
        {
            return repair is JetpackRepair;
        }

        public void ApplyRepair(BaseRepair repair)
        {
            if (repair is not JetpackRepair) return;

            _isRepaired = true;
            _jetpackFuel = FuelCapacity();
        }

        public RepairType GetRepairType()
        {
            return RepairType.Jetpack;
        }

        public bool IsRepaired()
        {
            return _isRepaired;
        }

        public bool CanApplyUpgrade(BaseUpgrade upgrade)
        {
            return upgrade is JetpackUpgrade;
        }

        public void ApplyUpgrade(BaseUpgrade upgrade)
        {
            if (upgrade is JetpackUpgrade jetpackUpgrade) appliedUpgrades.Add(jetpackUpgrade);
        }

        public UpgradeType GetUpgradeType()
        {
            return UpgradeType.Player;
        }

        private float FuelConsumptionRate()
        {
            return appliedUpgrades.Aggregate(settings.jetpackFuelConsumptionRate,
                (current, upgrade) => current * upgrade.jetpackFuelConsumptionBonus);
        }

        private float FuelRegenerationRate()
        {
            return appliedUpgrades.Aggregate(settings.jetpackFuelRegenerationRate,
                (current, upgrade) => current * upgrade.jetpackFuelRegenerationBonus);
        }

        private float FuelCapacity()
        {
            return appliedUpgrades.Aggregate(settings.jetpackFuel,
                (current, upgrade) => current * upgrade.jetpackFuelCapacityBonus);
        }

        private float Force()
        {
            return appliedUpgrades.Aggregate(settings.jetpackForce,
                (current, upgrade) => current * upgrade.jetpackForceBonus);
        }

        public void Handle(float forward, float strafe)
        {
            if (_jetpackFuel <= 0)
            {
                AudioManager.Instance.PlaySound(jetpackAudioSource, jetpackEmptyClip);
                isJetpacking = false;
                return;
            }

            _jetpackFuel = Mathf.Max(_jetpackFuel - FuelConsumptionRate() * Time.deltaTime, 0);

            var moveDirection = transform.forward * forward + transform.right * strafe;
            _rb.AddForce(transform.up * Force(), ForceMode.Acceleration);
            _rb.AddForce(moveDirection * Force() / 2f, ForceMode.Acceleration);

            AudioManager.Instance.PlaySound(jetpackAudioSource, jetpackClip);
        }

        public bool CanJetpack()
        {
            return _isRepaired && _jetpackFuel > 0;
        }

        [CreateAssetMenu(fileName = "JetpackSettings", menuName = "JetpackSettings")]
        public class JetpackSettings : ScriptableObject
        {
            public float jetpackForce = 2.0f;
            public float jetpackFuel = 100.0f;
            public float jetpackFuelConsumptionRate = 10.0f;
            public float jetpackFuelRegenerationRate = 10.0f;
        }
    }
}