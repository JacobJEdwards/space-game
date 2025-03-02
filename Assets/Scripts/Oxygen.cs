#nullable enable

using System.Collections;
using Unity.Assertions;
using Unity.Cinemachine;
using UnityEngine;
using UnityEngine.Events;
using UnityEngine.Rendering.PostProcessing;

[RequireComponent(typeof(Health))]
public class Oxygen : MonoBehaviour
{
    [Header("Config")]
    [SerializeField] public OxygenConfig config = null!;

    public UnityEvent<float> onOxygenChanged = new();
    public float MaxOxygen => config.MaxOxygen;
    private bool _isDamaging;

    [SerializeField] private CinemachineCamera playerCamera = null!;
    private PostProcessProfile _postProcessVolume = null!;
    private CinemachinePostProcessing _cinemachinePostProcessing = null!;
    private Vignette _vignette = null!;

    public float CurrentOxygen { get; private set; }

    private Health _health = null!;

    public void Reset()
    {
        CurrentOxygen = config.MaxOxygen;
        onOxygenChanged.Invoke(CurrentOxygen);
    }

    private void Awake()
    {
        _cinemachinePostProcessing = playerCamera.GetComponent<CinemachinePostProcessing>();
        _postProcessVolume = _cinemachinePostProcessing.Profile;
        _postProcessVolume.TryGetSettings(out _vignette);
    }

    public void Start()
    {
        _health = GetComponent<Health>();
        CurrentOxygen = config.MaxOxygen;
    }

    public void TakeDamage(float damage)
    {
        CurrentOxygen = Mathf.Clamp(CurrentOxygen - damage, 0, config.MaxOxygen);

        if (CurrentOxygen <= 0 && !_isDamaging)
        {
            StartCoroutine(DamageHealth());
        }

        onOxygenChanged.Invoke(CurrentOxygen);
    }

    private IEnumerator DamageHealth()
    {
        _isDamaging = true;

        while (CurrentOxygen <= 0)
        {
            yield return new WaitForSeconds(2f);

            _health.TakeDamage(20);
        }

        _isDamaging = false;
    }
}