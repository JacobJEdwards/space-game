#nullable enable

using System.Collections.Generic;
using DG.Tweening;
using Interfaces;
using Player;
using Spaceship;
using UnityEngine;
using UnityEngine.Events;
using UnityEngine.UI;

namespace Managers
{
    public enum UIState
    {
        ZeroG,
        Ship,
        Pause,
        Inventory,
        Death,
        Final,
        None
    }


    public class UiManager : MonoBehaviour
    {
        [Header("Spaceship Settings")] [SerializeField]
        private ShipController shipController = null!;

        [Header("ZeroG Settings")] [SerializeField]
        private PlayerController playerController = null!;

        [Header("Hint Settings")] [SerializeField]
        private Text hint = null!;

        [Header("Info Settings")] [SerializeField]
        private Text info = null!;

        [Header("Warning Settings")] [SerializeField]
        private Text warning = null!;

        [Header("Quest Settings")] [SerializeField]
        private Text quest = null!;

        public UnityEvent<UIState> onStateChanged = new();

        private readonly Image _blackScreen = null!;

        private readonly Dictionary<UIState, IUIPanel> _uiPanels = new();
        private UIState _currentState = UIState.None;

        private UIState _previousState = UIState.None;
        public static UiManager Instance { get; private set; } = null!;

        private void Awake()
        {
            if (!Instance)
                Instance = this;
            else
                Destroy(gameObject);
        }

        public void FadeToBlack(float duration)
        {
            _blackScreen.gameObject.SetActive(true);
            _blackScreen.DOFade(1, duration).SetEase(Ease.OutQuad);
        }

        public void RegisterPanel(IUIPanel panel)
        {
            _uiPanels.TryAdd(panel.AssociatedState, panel);

            _uiPanels[panel.AssociatedState].Hide();
        }

        public void TransitionToState(UIState state)
        {
            if (_currentState == state) return;
            if (_currentState is UIState.Death or UIState.Final) return;

            if (_uiPanels.TryGetValue(_currentState, out var panel)) panel.Hide();

            if (_uiPanels.TryGetValue(state, out var uiPanel)) uiPanel.Show();

            _previousState = _currentState;
            _currentState = state;

            onStateChanged.Invoke(state);
        }

        public void SetHint(string text)
        {
            hint.text = text;
        }

        public void SetInfo(string text)
        {
            info.text = text;
        }

        public void SetWarning(string text)
        {
            warning.text = text;
        }

        public void SetQuest(string text)
        {
            quest.text = text;
            quest.DOFade(1, 0.5f).SetEase(Ease.OutBounce);
        }

        public void ClearQuest()
        {
            quest.DOFade(0, 0.5f).SetEase(Ease.OutBounce);
        }

        public void SetInfo(string text, float duration)
        {
            CancelInvoke(nameof(ClearInfo));
            info.text = text;
            Invoke(nameof(ClearInfo), duration);
        }

        public void SetWarning(string text, float duration)
        {
            CancelInvoke(nameof(ClearWarning));
            warning.text = text;
            Invoke(nameof(ClearWarning), duration);
        }

        public void SetHint(string text, float duration)
        {
            CancelInvoke(nameof(ClearHint));
            hint.text = text;
            Invoke(nameof(ClearHint), duration);
        }


        public void ClearHint()
        {
            hint.text = string.Empty;
        }

        public void ClearInfo()
        {
            info.text = string.Empty;
        }

        public void ClearWarning()
        {
            warning.text = string.Empty;
        }

        public void TogglePause()
        {
            TransitionToState(_currentState == UIState.Pause ? _previousState : UIState.Pause);

            if (_currentState == UIState.Pause)
            {
                Time.timeScale = 0;
                Cursor.lockState = CursorLockMode.None;
                Cursor.visible = true;
                if (_previousState == UIState.Ship)
                    shipController.gameObject.SetActive(false);
                else
                    playerController.gameObject.SetActive(false);
            }
            else
            {
                Time.timeScale = 1;
                Cursor.lockState = CursorLockMode.Locked;
                Cursor.visible = false;

                if (_currentState == UIState.Ship)
                    shipController.gameObject.SetActive(true);
                else
                    playerController.gameObject.SetActive(true);
            }
        }

        public void ToggleInventory()
        {
            TransitionToState(_currentState == UIState.Inventory ? _previousState : UIState.Inventory);

            if (_currentState == UIState.Inventory)
            {
                Time.timeScale = 0;
                Cursor.lockState = CursorLockMode.None;
                Cursor.visible = true;
                if (_previousState == UIState.Ship)
                    shipController.gameObject.SetActive(false);
                else
                    playerController.gameObject.SetActive(false);
            }
            else
            {
                Time.timeScale = 1;
                Cursor.lockState = CursorLockMode.Locked;
                Cursor.visible = false;

                if (_currentState == UIState.Ship)
                    shipController.gameObject.SetActive(true);
                else
                    playerController.gameObject.SetActive(true);
            }
        }
    }
}