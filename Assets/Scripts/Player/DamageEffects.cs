using Unity.Cinemachine;
using UnityEngine;
using UnityEngine.Rendering;
using UnityEngine.Rendering.PostProcessing;

namespace Player
{
    [RequireComponent(typeof(Health), typeof(Oxygen))]
    public class DamageEffects : MonoBehaviour
    {
        [SerializeField] private Vignette vignette = null!;
        [SerializeField] private CinemachinePostProcessing postProcessing = null!;

        [SerializeField] private Oxygen oxygen = null!;
        [SerializeField] private Health health = null!;

        private void Start()
        {
            postProcessing.Profile.TryGetSettings(out vignette);
            oxygen = GetComponent<Oxygen>();
            health = GetComponent<Health>();

            oxygen.onOxygenChanged.AddListener(OnOxygenChanged);
            health.onHealthChanged.AddListener(OnHealthChanged);
        }

        private void OnOxygenChanged(float o)
        {
            // doesn't work
            vignette.intensity.value = 1 - (oxygen.CurrentOxygen / 100);
        }

        private void OnHealthChanged(float h)
        {
            vignette.intensity.value = 1 - (h / health.MaxHealth);
        }

    }
}