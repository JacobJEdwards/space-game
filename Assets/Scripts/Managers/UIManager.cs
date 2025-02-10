using System.Collections.Generic;
using UnityEngine;
using Spaceship;
using Player;
using TMPro;
using Interfaces;

namespace Managers
{

public enum UIState
{
    ZeroG,
    Ship,
    Pause,
    Inventory,
    None
}


public class UiManager : MonoBehaviour
{
    [Header("Spaceship Settings")] [SerializeField]
    private ShipController shipController;

    [SerializeField] private ShipShooting shipShooting;

    [Header("ZeroG Settings")] [SerializeField]
    private PlayerController playerController;

    [Header("Hint Settings")] [SerializeField]
    private TextMeshProUGUI hint;

    private readonly Dictionary<UIState, IUIPanel> _uiPanels = new ();
    private UIState _currentState = UIState.None;

    public void RegisterPanel(IUIPanel panel)
    {
        _uiPanels.Add(panel.AssociatedState, panel);
        _uiPanels[panel.AssociatedState].Hide();
    }

    public void TransitionToState(UIState state)
    {
        if (_currentState == state) return;

        if (_uiPanels.TryGetValue(_currentState, out var panel))
            panel.Hide();

        if (_uiPanels.TryGetValue(state, out var uiPanel))
            uiPanel.Show();

        _currentState = state;
    }

    public void SetHint(string text)
    {
        // use the hint object to display the text, find component in children
        hint.text = text;
    }
}
}
