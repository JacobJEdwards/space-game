using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using Player;
using UnityEngine;
using UnityEngine.UI;

namespace Managers
{
    public class CompassManager : MonoBehaviour
    {
        public RawImage compassImage;
        [SerializeField] private PlayerController player;
        [SerializeField] public RectTransform compassMarkersParent;
        public GameObject compassMarkerPrefab;
        private readonly List<CompassMarker> _compassMarkers = new();

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
                .Distance(player.transform.position, o.worldGameObject.position)).ToList();

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