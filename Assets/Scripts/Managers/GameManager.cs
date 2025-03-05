using System.Collections;
using DG.Tweening;
using HUDIndicator;
using Movement;
using Player;
using Player.Upgrades;
using Unity.Cinemachine;
using UnityEngine;
using UnityEngine.Rendering.PostProcessing;
using UnityEngine.UI;

namespace Managers
{
    public class GameManager : MonoBehaviour
    {
        [SerializeField] private Image blackScreen = null!;

        [SerializeField] private IndicatorOnScreen questIndicatorOnScreen = null!;
        [SerializeField] private IndicatorOffScreen questIndicatorOffScreen = null!;

        [SerializeField] private Thrusters thrusters = null!;

        [SerializeField] private DepthOfField dof = null!;
        [SerializeField] private CinemachinePostProcessing postProcessing = null!;
        [SerializeField] private DamageEffects damageEffects = null!;

        public int introStep;

        private UiManager _uiManager = null!;
        private UpgradeManager _upgradeManager = null!;

        public static GameManager Instance { get; private set; } = null!;

        private void Awake()
        {
            if (!Instance)
                Instance = this;
            else
                Destroy(gameObject);
        }

        private void Start()
        {
            Init();
            IntroStep(introStep);
        }

        private void Init()
        {
            _uiManager = UiManager.Instance;
            _upgradeManager = UpgradeManager.Instance;

            postProcessing.Profile.TryGetSettings(out dof);

            IntroStep(introStep);
        }

        private IEnumerator ClearDof()
        {
            yield return new WaitForSeconds(5f);
            dof.enabled.value = false;
            damageEffects.enabled = true;
        }

        private void OnShipEntry(UIState state)
        {
            if (state != UIState.Ship) return;
            IntroStep(1);
        }

        private void OnRepairApplied(BaseRepair repair)
        {
            if (repair is not ThrusterRepair) return;

            IntroStep(2);
        }

        public void IntroStep(int step)
        {
            if (thrusters.IsRepaired() && step != 2)
            {
                introStep = 2;
                IntroStep(introStep);
                return;
            }

            switch (step)
            {
                case 0:
                {
                    if (thrusters.IsRepaired())
                    {
                        introStep = 2;
                        IntroStep(introStep);
                    }

                    StopAllCoroutines();
                    blackScreen.DOFade(0, 1f).OnComplete(() =>
                    {
                        _uiManager.SetQuest("Get to your spaceship");
                        blackScreen.enabled = false;
                        _uiManager.TransitionToState(UIState.ZeroG);
                    });

                    if (dof) dof.enabled.value = true;

                    damageEffects.enabled = false;


                    StartCoroutine(ClearDof());

                    _uiManager.onStateChanged.AddListener(OnShipEntry);
                }
                    break;
                case 1:
                {
                    StopAllCoroutines();

                    blackScreen.enabled = false;

                    _uiManager.onStateChanged.RemoveListener(OnShipEntry);

                    dof.enabled.value = false;
                    damageEffects.enabled = true;

                    _uiManager.SetQuest("Repair the thrusters");

                    questIndicatorOffScreen.enabled = true;
                    questIndicatorOnScreen.enabled = true;

                    _upgradeManager.onRepairApplied.AddListener(OnRepairApplied);
                }
                    break;
                case 2:
                {
                    StopAllCoroutines();

                    dof.enabled.value = false;
                    damageEffects.enabled = true;
                    blackScreen.enabled = false;

                    questIndicatorOffScreen.enabled = false;
                    questIndicatorOnScreen.enabled = false;

                    _uiManager.SetQuest("Repair the hyperdrive");
                }
                    break;
            }
        }
    }
}