using Interfaces;
using JetBrains.Annotations;
using Unity.Assertions;
using UnityEngine;

namespace Weapons
{
    [RequireComponent(typeof(LineRenderer))]
    public class LaserFire : MonoBehaviour, IFireable
    {
        [System.Serializable]
        public class LaserSettings
        {
            public ParticleSystem laserHitEffect;

            public LayerMask mask;

            public float range = 100f;

            public float damage = 50f;
        }

        [SerializeField] private LaserSettings settings;

        private LineRenderer _laser;
        private Camera _mainCam;

        private void Start()
        {
            _mainCam = Camera.main;
            _laser = GetComponent<LineRenderer>();
            _laser.gameObject.SetActive(false);
            settings.mask = LayerMask.GetMask("Shootable", "PlanetSurface", "Water", "Rock");
            ValidateComponents();
        }

        private void ValidateComponents()
        {
            Assert.IsNotNull(_mainCam);
            Assert.IsNotNull(_laser);
        }

        public void StopFire()
        {
            _laser.gameObject.SetActive(false);
        }

        public void Fire()
        {
            if (IsInRange(out var hit))
            {
                if (hit.collider.gameObject.layer == (int)Layers.Water) return;

                var localHitPosition = _laser.transform.InverseTransformPoint(hit.point);
                SetPosition(localHitPosition);
                var effect = Instantiate(settings.laserHitEffect, hit.point, Quaternion.identity);
                Destroy(effect.gameObject, effect.main.duration);
                MaybeDamageTarget(hit);
            }
            else
            {
                var hitPos = Vector3.forward * settings.range;
                SetPosition(hitPos);
            }

            _laser.gameObject.SetActive(true);
        }

        private bool IsInRange(out RaycastHit hit)
        {
            return TargetInfo.IsTargetInRange(_mainCam, out hit, settings.range, settings.mask);
        }

        private void MaybeDamageTarget(RaycastHit hit)
        {
            if (hit.collider.gameObject.layer != (int)Layers.Shootable) return;

            if (hit.collider.transform.GetComponent<IDamageable>() is { } damageable)
            {
                damageable.TakeDamage(settings.damage * Time.deltaTime);
            }
        }

        private void SetPosition(Vector3 hitPos)
        {
            _laser.SetPosition(1, hitPos);
        }
    }
}