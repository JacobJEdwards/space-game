using Interfaces;
using UnityEngine;

namespace Weapons
{
public class LaserFire : MonoBehaviour, IFireable
{
    private LineRenderer _laser;

    [SerializeField] public ParticleSystem laserHitEffect;

    [SerializeField] private float laserRange = 100f;

    private Transform _aim;

    [SerializeField] private LayerMask mask;

    [SerializeField] private float range = 100f;

    [SerializeField] private float damage = 50f;

    private void Start()
    {
        _laser = GetComponent<LineRenderer>();
        _laser.gameObject.SetActive(false);
        _aim = transform.parent;
        mask = LayerMask.GetMask("Shootable", "PlanetSurface", "Interaction");
    }

    public void StopFire()
    {
        _laser.gameObject.SetActive(false);
    }

    public void Fire()
    {
        if (IsInRange(out var hit))
        {
            var localHitPosition = _laser.transform.InverseTransformPoint(hit.point);
            SetPosition(localHitPosition);
            Instantiate(laserHitEffect, hit.point, Quaternion.identity);
            MaybeDamageTarget(hit);
        }
        else
        {
            SetPosition(Vector3.forward * range);
        }

        _laser.gameObject.SetActive(true);
    }

    public bool IsInRange(out RaycastHit hit)
    {
        return TargetInfo.IsTargetInRange(_aim.transform.position, _aim.transform.forward, out hit, range, mask);
    }

    private void MaybeDamageTarget(RaycastHit hit)
    {
        if (hit.collider.transform.GetComponent<IDamageable>() is { } damageable)
        {
            damageable.TakeDamage(damage * Time.deltaTime);
        }
    }

    private void SetPosition(Vector3 hitPos)
    {
        _laser.SetPosition(1, hitPos);
    }
}
}
