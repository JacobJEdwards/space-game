using Microlight.MicroBar;
using UnityEngine;

public class ShipUI : MonoBehaviour
{
    [Header("Spaceship Settings")] [SerializeField]
    private SpaceMovement shipMovement;

    [SerializeField] private ShipShooting shipShooting;
    [SerializeField] private SpaceMovementConfig shipMovementConfig;

    [Header("UI Settings")] [SerializeField]
    private MicroBar overheatedUI;

    [SerializeField] private MicroBar boostUI;

    private bool _shipUIActive;

    private void Start()
    {
        overheatedUI.Initialize(shipShooting.LaserMaxCharge);
        boostUI.Initialize(shipMovementConfig.MaxBoostAmount);
    }

    private void Update()
    {
        if (!_shipUIActive) return;

        overheatedUI.UpdateBar(shipShooting.LaserCharge);
        boostUI.UpdateBar(shipMovement.CurrentBoostAmount);
    }

    public void Hide()
    {
        _shipUIActive = false;
    }

    public void Show()
    {
        _shipUIActive = true;
    }
}