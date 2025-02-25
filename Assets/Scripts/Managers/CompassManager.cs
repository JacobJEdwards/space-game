#nullable enable

using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using Player;
using UI;
using Unity.Assertions;
using UnityEngine;
using UnityEngine.UI;


namespace Managers
{
    public class CompassManager : MonoBehaviour
    {
        public RawImage compassImage = null!;
        [SerializeField] private PlayerController player = null!;
        [SerializeField] public RectTransform compassMarkersParent = null!;
        public GameObject compassMarkerPrefab = null!;
        private readonly List<CompassMarker> _compassMarkers = new();

        private void Awake()
        {
            Assert.IsNotNull(compassImage, "Compass image is not set!");
            Assert.IsNotNull(player, "Player is not set!");
            Assert.IsNotNull(compassMarkersParent, "Compass markers parent is not set!");
            Assert.IsNotNull(compassMarkerPrefab, "Compass marker prefab is not set!");
        }

        private IEnumerator Start()
        {
            var updateDelay = new WaitForSeconds(0.5f);

            while (enabled)
            {
                SortCompassObjectives();
                yield return updateDelay;
            }
        }

        private void SortCompassObjectives()
        {
            var orderedMarkers = _compassMarkers.Where(o => o.worldGameObject).OrderByDescending(o => Vector3
                .Distance(player.transform.position, o.worldGameObject?.position ?? Vector3.zero)).ToList();

            for (var i = 0; i < orderedMarkers.Count; i++)
            {
                orderedMarkers[i].UpdateUIIndex(i);
            }
        }

        public void AddObjectiveForObject(GameObject obj, Color color, Sprite sprite)
        {
            var compassMarker = Instantiate(compassMarkerPrefab, compassMarkersParent, false)
                .GetComponent<CompassMarker>();
            compassMarker.compassManager = this;
            compassMarker.Configure(obj.transform, color, sprite, player.transform);
            _compassMarkers.Add(compassMarker);
        }

        private void LateUpdate() => UpdateCompassHeading();

        private void UpdateCompassHeading()
        {
            var compassUvPos = Vector2.right * (player.transform.eulerAngles.y / 360f);
            compassImage.uvRect = new Rect(compassUvPos, Vector2.one);
        }
    }
}