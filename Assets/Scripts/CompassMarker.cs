using System;
using Managers;
using Player;
using UnityEngine;
using UnityEngine.Events;
using UnityEngine.UI;

public class CompassMarker : MonoBehaviour
{
    public Image markerImage;
    private bool IsActive { get; set; }
    private RectTransform _rectTransform;

    [SerializeField]
    private Transform player;

    [SerializeField] public Transform worldGameObject;

    [SerializeField]
    public CompassManager compassManager;

    [SerializeField]
    public float minVisibleDistance = 5f;
    [SerializeField]
    public float maxVisibleDistance = 1000f;

    private void Awake()
    {
        _rectTransform = GetComponent<RectTransform>();
    }

    public CompassMarker Configure(Transform obj, Color color, Sprite sprite, Transform playerTrans)
    {
        player = playerTrans;
        worldGameObject = obj.transform;
        markerImage.color = color;
        markerImage.sprite = sprite;
        markerImage.transform.localScale = Vector3.one;
        IsActive = true;

        UpdateCompassPosition();
        UpdateVisibility();

        return this;
    }

    private void LateUpdate()
    {
        UpdateCompassPosition();
    }

    private void UpdateCompassPosition()
    {
        if (!worldGameObject || !IsActive)
        {
            markerImage.transform.localScale = Vector3.zero;
            return;
        }

        var direction = (worldGameObject.position - player.position).normalized;
        var playerForward = player.forward;
        var angle = Vector3.SignedAngle(playerForward, direction, Vector3.up);
        var normalizedAngle = angle / 180f;
        var compassWidth = compassManager.compassImage.rectTransform.rect.width;
        var xPos = normalizedAngle * compassWidth / 2f;
        _rectTransform.localPosition = new Vector2(xPos, _rectTransform.localPosition.y);
    }

    private void Update()
    {
        var targetScale = IsActive && worldGameObject ? Vector3.one : Vector3.zero;
        markerImage.transform.localScale = Vector3.Lerp(markerImage.transform.localScale, targetScale, Time.deltaTime * 10f);
    }

    private float GetMarkerAngle()
    {
        return Vector3.SignedAngle(player.forward, GetMarkerDirection(), Vector3.up) / 180f;
    }

    private Vector3 GetMarkerDirection()
    {
        var direction = new Vector3(worldGameObject.position.x, player.position.y, worldGameObject.position.z) - player.position;
        return direction.normalized;
    }

    public void UpdateUIIndex(int index)
    {
        _rectTransform.SetSiblingIndex(index);
        UpdateVisibility();
    }

    private void UpdateVisibility()
    {
        if (!worldGameObject || !player)
        {
            IsActive = false;
            return;
        }

        var currentDistance = Vector3.Distance(player.position, worldGameObject.position);
        IsActive = currentDistance >= minVisibleDistance && currentDistance <= maxVisibleDistance;
    }
}