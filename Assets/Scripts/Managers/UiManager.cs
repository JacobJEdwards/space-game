using UnityEngine;
using UnityEngine.Serialization;

public class UiManager : MonoBehaviour
{
    [Header("UI Settings")] [SerializeField]
    private ShipUI shipUI;

    [SerializeField] private ZeroGUI zeroGUI;

    [FormerlySerializedAs("shipMovement")] [Header("Spaceship Settings")] [SerializeField]
    private ShipController shipController;

    [SerializeField] private ShipShooting shipShooting;

    [FormerlySerializedAs("playerMovement")] [Header("ZeroG Settings")] [SerializeField]
    private PlayerController playerController;

    [Header("Interaction Zone Settings")] [SerializeField]
    private ShipInteractionZone shipInteractionZone;

    [Header("Hint Settings")] [SerializeField]
    private GameObject hint;

    private void Start()
    {
        shipUI = FindFirstObjectByType<ShipUI>();
        zeroGUI = FindFirstObjectByType<ZeroGUI>();

        shipUI.gameObject.SetActive(false);
        shipUI.Hide();
        zeroGUI.Show();

        shipInteractionZone.onPlayerEnterZone.AddListener(OnPlayerEnterZone);
        shipInteractionZone.onPlayerExitZone.AddListener(OnPlayerExitZone);

        shipController.onRequestExitShip.AddListener(OnRequestExitShip);
        playerController.onRequestEnterShip.AddListener(OnRequestEnterShip);
    }

    private void OnPlayerEnterZone(PlayerController player)
    {
        hint.SetActive(true);
    }

    private void OnPlayerExitZone(PlayerController player)
    {
        hint.SetActive(false);
    }

    private void OnRequestExitShip()
    {
        zeroGUI.gameObject.SetActive(true);
        zeroGUI.Show();
        shipUI.gameObject.SetActive(false);
        shipUI.Hide();
        hint.SetActive(false);
    }

    private void OnRequestEnterShip()
    {
        zeroGUI.gameObject.SetActive(false);
        zeroGUI.Hide();
        shipUI.gameObject.SetActive(true);
        shipUI.Show();
        hint.SetActive(false);
    }
}