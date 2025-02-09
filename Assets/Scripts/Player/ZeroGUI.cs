using Microlight.MicroBar;
using Unity.Assertions;
using UnityEngine;
using UnityEngine.Serialization;

public class ZeroGUI : MonoBehaviour
{
    [FormerlySerializedAs("playerOxygen")]
    [Header("ZeroG Settings")]
    [SerializeField] private Oxygen oxygen;

    [SerializeField] private OxygenConfig playerOxygenConfig;

    [Header("UI Settings")]
    [SerializeField]
    private MicroBar oxygenUI;

    private bool _zeroGUIActive;

    private void Start()
    {
        oxygenUI.Initialize(playerOxygenConfig.MaxOxygen);
    }

    private void Update()
    {
        if (!_zeroGUIActive) return;

        oxygenUI.UpdateBar(oxygen.CurrentOxygen);
    }


    public void Hide()
    {
        _zeroGUIActive = false;
    }

    public void Show()
    {
        _zeroGUIActive = true;
    }
}