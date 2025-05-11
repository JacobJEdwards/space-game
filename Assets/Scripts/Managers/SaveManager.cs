using System;
using System.Collections;
using System.Linq;
using CollectableResources;
using Player;
using Spaceship;
using UnityEngine;

namespace Managers
{
    public class SaveManager : MonoBehaviour
    {
        [SerializeField] private PlayerController player = null!;
        [SerializeField] private ShipController ship = null!;
        [SerializeField] private Inventory inventory = null!;
        [SerializeField] private UpgradeManager upgradeManager = null!;
        [SerializeField] private GameManager gameManager = null!;

        public static SaveManager Instance { get; private set; } = null!;


        private void Awake()
        {
            if (!Instance)
                Instance = this;
            else
                Destroy(gameObject);
        }

        private void Start()
        {
            upgradeManager = UpgradeManager.Instance;
            gameManager = GameManager.Instance;

            if (!SharedData.Instance.newGame)
                LoadGame(SharedData.Instance.savePath);

            StartCoroutine(SaveGameSlow());
        }


        private IEnumerator SaveGameSlow()
        {
            while (Application.isPlaying)
            {
                yield return new WaitForSeconds(5f);
                SaveGame();
            }
        }

        public void SaveGame()
        {
            var upgrades = upgradeManager.AppliedUpgrades.Select(t => t.Value).Select(t => t.Select(u => u.upgradeName))
                .SelectMany(t => t).ToArray();
            var repairs = upgradeManager.CompletedRepairs.Select(t => t.Value).Select(t => t.Select(u => u.upgradeName))
                .SelectMany(t => t).ToArray();

            var playerPosition = player.transform.position;
            var playerRotation = player.transform.rotation;

            var shipPosition = ship.transform.position;
            var shipRotation = ship.transform.rotation;

            var shipState = ship.currentState;
            var playerState = player.playerState;

            var resources = inventory.resources.Select(t => $"{t.resourceName}:{t.resourceAmount}").ToArray();

            var saveState = new SaveState
            {
                appliedUpgrades = upgrades,
                appliedRepairs = repairs,
                playerPosition = playerPosition,
                playerRotation = playerRotation,
                resources = resources,
                introStep = gameManager.introStep,
                shipPosition = shipPosition,
                shipRotation = shipRotation,
                shipState = shipState,
                playerState = playerState
            };

            var json = JsonUtility.ToJson(saveState);

            FileManager.WriteToFile(SharedData.Instance.savePath, json);
        }

        private void LoadGame(string filename)
        {
            if (!FileManager.FileExists(filename)) return;

            var success = FileManager.LoadFromFile(filename, out var json);

            if (!success) Debug.LogError("Failed to load save file");
            ;

            var saveState = JsonUtility.FromJson<SaveState>(json);

            Debug.Log($"Loaded save file: {filename}");

            gameManager.introStep = saveState.introStep;

            Debug.Log($"Intro step: {gameManager.introStep}");

            foreach (var upgrade in saveState.appliedRepairs) upgradeManager.ForceApplyRepair(upgrade);
            foreach (var repair in saveState.appliedUpgrades) upgradeManager.ForceApplyUpgrade(repair);

            foreach (var resource in saveState.resources)
            {
                var split = resource.Split(':');
                var resourceName = split[0];
                var resourceAmount = int.Parse(split[1]);
                var cp = Resources.FindObjectsOfTypeAll<ResourceObject>().First(t => t.resourceName == resourceName);
                var res = ScriptableObject.CreateInstance<ResourceObject>();
                res.resourceName = cp.resourceName;
                res.resourceAmount = resourceAmount;
                res.resourceDescription = cp.resourceDescription;
                res.resourceSprite = cp.resourceSprite;

                inventory.AddResource(res);
            }

            player.transform.position = saveState.playerPosition;
            player.transform.rotation = saveState.playerRotation;

            ship.transform.position = saveState.shipPosition;
            ship.transform.rotation = saveState.shipRotation;

            ship.SetCurrentState(saveState.shipState, true);
            player.playerState = saveState.playerState;

            if (player.playerState == PlayerState.OnShip) player.shipToEnter = ship;
        }

        [Serializable]
        public class SaveState
        {
            public string[] appliedUpgrades;
            public string[] appliedRepairs;

            public Vector3 playerPosition;
            public Quaternion playerRotation;

            public Vector3 shipPosition;
            public Quaternion shipRotation;

            public ShipState shipState;
            public PlayerState playerState;

            public string[] resources;

            public int introStep;
        }
    }
}