#nullable enable

using System;
using Interfaces;
using Managers;
using Unity.Assertions;
using UnityEngine;

namespace Weapons
{
    [RequireComponent(typeof(LineRenderer))]
    public class LaserFire : MonoBehaviour, IFireable
    {
        [SerializeField] private LaserSettings settings = new();

        private float _clipTime;

        private LineRenderer _laser = null!;
        private Camera _mainCam = null!;

        private void Start()
        {
            _mainCam = Camera.main!;
            _laser = GetComponent<LineRenderer>();
            _laser.gameObject.SetActive(false);
            settings.mask = LayerMask.GetMask("Shootable", "PlanetSurface", "Water", "Rock", "NPC");
            settings.audioSource.transform.parent = transform;
            settings.hitAudioSource.transform.parent = transform;
            ValidateComponents();
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
                // settings.hitAudioSource.transform.position = hit.point;
                AudioManager.Instance.PlaySound(settings.hitAudioSource, settings.hitSound);
            }
            else
            {
                var hitPos = Vector3.forward * settings.range;
                SetPosition(hitPos);
            }

            _laser.gameObject.SetActive(true);

            AudioManager.Instance.PlaySound(settings.audioSource, settings.laserSound);
        }

        private void ValidateComponents()
        {
            Assert.IsNotNull(_mainCam);
            Assert.IsNotNull(_laser);
            Assert.IsNotNull(settings.laserHitEffect);
        }

        private bool IsInRange(out RaycastHit hit)
        {
            return TargetInfo.IsTargetInRange(_mainCam, out hit, settings.range, settings.mask);
        }

        private void MaybeDamageTarget(RaycastHit hit)
        {
            if (hit.collider.transform.GetComponent<IDamageable>() is { } damageable)
                damageable.TakeDamage(settings.damage * Time.deltaTime);
        }

        private void SetPosition(Vector3 hitPos)
        {
            _laser.SetPosition(1, hitPos);
        }

        [Serializable]
        public class LaserSettings
        {
            public ParticleSystem laserHitEffect = null!;
            public LayerMask mask;
            public float range = 100f;
            public float damage = 50f;

            public AudioSource audioSource = null!;
            public AudioClip laserSound = null!;

            public AudioSource hitAudioSource = null!;
            public AudioClip hitSound = null!;
        }
    }
}