using UnityEngine;
using UnityEngine.UI;

namespace UI
{
    public class GearUI : MonoBehaviour
    {
        [SerializeField] private Button playerButton;
        [SerializeField] private Button gunButton;

        [SerializeField] private GameObject playerUpgradePanel;
        [SerializeField] private GameObject gunUpgradePanel;
        [SerializeField] private GameObject defaultPanel;

        private void Start()
        {
            playerButton.onClick.AddListener(ShowPlayerUpgradePanel);
            gunButton.onClick.AddListener(ShowGunUpgradePanel);
        }

        private void ShowPlayerUpgradePanel()
        {
            defaultPanel.SetActive(false);
            playerUpgradePanel.SetActive(true);
            gunUpgradePanel.SetActive(false);
        }

        private void ShowGunUpgradePanel()
        {
            defaultPanel.SetActive(false);
            playerUpgradePanel.SetActive(false);
            gunUpgradePanel.SetActive(true);
        }

    }
}