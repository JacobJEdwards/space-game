using UnityEngine;
using UnityEngine.InputSystem;

namespace Spaceship
{

public class ShipShooting : MonoBehaviour
{
    private ShipController _shipController;

    [SerializeField] private float laserMaxCharge = 10f;
    [SerializeField] private float laserHeatRate = 1f;
    [SerializeField] private float laserCoolRate = 2f;

    private LaserFire[] _lasers;

    private bool _firing;

    private bool _overheated;

    private bool _targetInRange;

    public float LaserMaxCharge => laserMaxCharge;
    public float LaserCharge { get; private set; }

    private void Start()
    {
        _shipController = GetComponent<ShipController>();
        LaserCharge = laserMaxCharge;
        _lasers = GetComponentsInChildren<LaserFire>(true);
    }

    private void Update()
    {
        if (!_shipController.IsOccupied)
            StopLasers();
        else
            HandleLaserFiring();

        CoolLasers();
    }

    private void HandleLaserFiring()
    {
        if (_firing && !_overheated)
            FireLasers();
        else
            StopLasers();
    }

    private void CoolLasers()
    {
        if (_firing) return;

        var cooling = laserCoolRate * Time.deltaTime;

        LaserCharge += cooling;

        if (LaserCharge >= laserMaxCharge) _overheated = false;

        LaserCharge = Mathf.Clamp(LaserCharge, 0, laserMaxCharge);
    }

    private void FireLasers()
    {
        foreach (var laser in _lasers) laser.Fire();

        HeatLasers();
    }

    private void HeatLasers()
    {
        if (!_firing || _overheated) return;

        var heat = laserHeatRate * Time.deltaTime;
        LaserCharge -= heat;

        if (LaserCharge > 0) return;

        _overheated = true;
        _firing = false;
    }

    private void StopLasers()
    {
        foreach (var laser in _lasers) laser.StopFire();
        _firing = false;
    }

    #region Input Actions
    public void OnFire(InputAction.CallbackContext context)
    {
        if (context.performed)
            _firing = true;
        else if (context.canceled) _firing = false;
    }
    #endregion
}
}
