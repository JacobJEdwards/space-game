#nullable enable

using System.Collections.Generic;
using Interfaces;
using Player;
using Spaceship;
using UnityEngine;
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
        None,
    }


    public class UiManager : MonoBehaviour
    {
        public static UiManager Instance { get; private set; } = null!;

        [Header("Spaceship Settings")] [SerializeField]
        private ShipController? shipController;

        [SerializeField] private ShipShooting? shipShooting;

        [Header("ZeroG Settings")] [SerializeField]
        private PlayerController? playerController;

        [Header("Hint Settings")] [SerializeField]
        private Text hint = null!;

        [Header("Info Settings")] [SerializeField]
        private Text info = null!;

        [Header("Warning Settings")] [SerializeField]
        private Text warning = null!;

        private readonly Dictionary<UIState, IUIPanel> _uiPanels = new();
        private UIState _currentState = UIState.None;

        private UIState _previousState = UIState.None;


        private void Awake()
        {
            if (!Instance)
            {
                Instance = this;
                DontDestroyOnLoad(gameObject);
            }
            else
            {
                Destroy(gameObject);
            }
        }

        public void RegisterPanel(IUIPanel panel)
        {
            _uiPanels.Add(panel.AssociatedState, panel);
            _uiPanels[panel.AssociatedState].Hide();
        }

        public void TransitionToState(UIState state)
        {
            if (_currentState == state) return;

            if (_uiPanels.TryGetValue(_currentState, out var panel))
            {
                panel.Hide();
            }

            if (_uiPanels.TryGetValue(state, out var uiPanel))
            {
                uiPanel.Show();
            }

            _previousState = _currentState;
            _currentState = state;
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

        public void ToggleInventory()
        {
            TransitionToState(_currentState == UIState.Inventory ? _previousState : UIState.Inventory);

            if (_currentState == UIState.Inventory)
            {
                Time.timeScale = 0;
                Cursor.lockState = CursorLockMode.None;
                Cursor.visible = true;
                playerController?.gameObject.SetActive(false);
            }
            else
            {
                Time.timeScale = 1;
                Cursor.lockState = CursorLockMode.Locked;
                Cursor.visible = false;
                playerController?.gameObject.SetActive(true);
            }
        }
    }
}