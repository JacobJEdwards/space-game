#nullable enable

using CollectableResources;
using Player;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;

namespace UI
{
    public class InventorySlot : MonoBehaviour, IPointerEnterHandler, IPointerExitHandler, IPointerClickHandler
        //IBeginDragHandler, IDragHandler, IEndDragHandler, IDropHandler
    {
        [SerializeField] private Text amountLabel = null!;
        [SerializeField] private Image icon = null!;
        [SerializeField] private Text nameLabel = null!;
        [SerializeField] private GameObject focus = null!;

        private Canvas _canvas;
        private RectTransform _rectTransform;
        private CanvasGroup _canvasGroup;
        private Vector2 _originalPosition;

        private ResourceObject? _resource;
        private InventoryUI _inventoryUI = null!;

        private void Awake()
        {
            _inventoryUI = GetComponentInParent<InventoryUI>(true);
            _canvas = GetComponentInParent<Canvas>();
            _rectTransform = GetComponent<RectTransform>();
            _canvasGroup = GetComponent<CanvasGroup>();
        }

        public void SetResource(ResourceObject resource)
        {
            _resource = resource;

            if (!resource)
            {
                ClearSlot();
                return;
            }

            icon.enabled = true;
            icon.sprite = resource.resourceSprite;
            amountLabel.text = resource.resourceAmount.ToString();
            nameLabel.text = resource.resourceName;
        }

        public void ClearSlot()
        {
            _resource = null;

            icon.enabled = false;
            icon.sprite = null;
            amountLabel.text = string.Empty;
            nameLabel.text = string.Empty;
        }

        public ResourceObject? GetResource()
        {
            return _resource;
        }

        public void OnPointerEnter(PointerEventData eventData)
        {
            focus.SetActive(true);
        }

        public void OnPointerExit(PointerEventData eventData)
        {
            focus.SetActive(false);
        }

        public void OnPointerClick(PointerEventData eventData)
        {
            if (!_resource)
            {
                _inventoryUI.ClearSelectedResource();
                return;
            }

            _inventoryUI.SetSelectedResource(_resource);
        }

        public void OnBeginDrag(PointerEventData eventData)
        {
            if (!_resource)
            {
                return;
            }

            _originalPosition = _rectTransform.anchoredPosition;
            _canvasGroup.alpha = 0.6f;
            _canvasGroup.blocksRaycasts = false;

            _inventoryUI.SetDraggedSlot(this);
        }

        public void OnDrag(PointerEventData eventData)
        {
            if (!_resource)
            {
                return;
            }

            RectTransformUtility.ScreenPointToLocalPointInRectangle(
                _canvas.transform as RectTransform,
                eventData.position,
                _canvas.worldCamera,
                out var localPoint);

            _rectTransform.position = _canvas.transform.TransformPoint(localPoint);
        }

        public void OnEndDrag(PointerEventData eventData)
        {
            if (!_resource) return;

            _canvasGroup.alpha = 1f;
            _canvasGroup.blocksRaycasts = true;
            _rectTransform.anchoredPosition = _originalPosition;

            _inventoryUI.ClearDraggedSlot();
        }

        public void OnDrop(PointerEventData eventData)
        {
            var draggedSlot = _inventoryUI.GetDraggedSlot();
            if (draggedSlot && draggedSlot != this)
            {
                InventoryUI.SwapItems(draggedSlot, this);
            }
        }
    }
}