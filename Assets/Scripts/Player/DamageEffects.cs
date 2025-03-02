using Managers;
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
        [SerializeField] private DepthOfField dof = null!;
        [SerializeField] private CinemachinePostProcessing postProcessing = null!;

        [SerializeField] private Oxygen oxygen = null!;
        [SerializeField] private Health health = null!;

        private void Start()
        {
            postProcessing.Profile.TryGetSettings(out vignette);
            postProcessing.Profile.TryGetSettings(out dof);

            dof.enabled.value = false;
            vignette.enabled.value = false;

            oxygen = GetComponent<Oxygen>();
            health = GetComponent<Health>();

            oxygen.onOxygenChanged.AddListener(OnOxygenChanged);
            health.onHealthChanged.AddListener(OnHealthChanged);
        }

        private void OnOxygenChanged(float o)
        {
            UpdateVignette();

            var warning = oxygen.MaxOxygen * 0.6f;

            if (oxygen.CurrentOxygen <= warning && oxygen.CurrentOxygen >= oxygen.MaxOxygen * 0.45f)
            {
                UiManager.Instance.SetWarning("Low Oxygen", 2f);
            }

            if (oxygen.CurrentOxygen <= oxygen.MaxOxygen * 0.45f && oxygen.CurrentOxygen > 0)
            {
                UiManager.Instance.SetWarning("Critical Oxygen", 2f);
            }
        }

        private void UpdateVignette()
        {
            if (oxygen.CurrentOxygen > oxygen.MaxOxygen * 0.7)
            {
                vignette.intensity.value = 0;
                vignette.enabled.value = false;
                return;
            }

            vignette.enabled.value = true;

            var oxygenThreshold = oxygen.MaxOxygen * 0.7f;
            var intensity = Mathf.Lerp(0, 1, 1 - oxygen.CurrentOxygen / oxygenThreshold);

            vignette.intensity.value = (intensity);
        }

        private void OnHealthChanged(float h)
        {
            UpdateDepthOfField();

            var warning = health.MaxHealth * 0.6f;

            if (health.CurrentHealth <= warning && health.CurrentHealth >= health.MaxHealth * 0.45f)
            {
                UiManager.Instance.SetWarning("Low Health", 2f);
            }

            if (health.CurrentHealth <= health.MaxHealth * 0.45f)
            {
                UiManager.Instance.SetWarning("Critical Health", 2f);
            }
        }

        private void UpdateDepthOfField()
        {
            if (health.CurrentHealth > health.MaxHealth * 0.7)
            {
                dof.enabled.value = false;
                return;
            }

            dof.enabled.value = true;

            var healthThreshold = health.MaxHealth * 0.7f;
            var focusDistance = Mathf.Lerp(0.5f, 0.1f, 1 - health.CurrentHealth / healthThreshold);

            dof.focusDistance.value = focusDistance;
        }
    }
}