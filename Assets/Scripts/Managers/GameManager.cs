using System.Collections;
using DG.Tweening;
using HUDIndicator;
using Movement;
using Player;
using Player.Upgrades;
using Spaceship;
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
        [SerializeField] private Hyperdrive hyperdrive = null!;

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

            StartCoroutine(SlowUpdate());
        }

        private void Init()
        {
            _uiManager = UiManager.Instance;
            _upgradeManager = UpgradeManager.Instance;

            postProcessing.Profile.TryGetSettings(out dof);

            IntroStep(introStep);
        }

        private IEnumerator SlowUpdate()
        {
            while (Application.isPlaying)
            {
                yield return new WaitForSeconds(1f);
                IntroStep(introStep);
            }
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
            switch (repair)
            {
                case ThrusterRepair:
                    introStep = 2;
                    IntroStep(2);
                    break;
                case HyperdriveRepair:
                    introStep = 3;
                    IntroStep(3);
                    break;
            }
        }

        private void IntroStep(int step)
        {
            if (hyperdrive.IsRepaired() && step != 3)
            {
                introStep = 3;
                IntroStep(introStep);
                return;
            }

            if (thrusters.IsRepaired() && step != 2 && !hyperdrive.IsRepaired())
            {
                introStep = 2;
                IntroStep(introStep);
                return;
            }

            // if (hyperdrive.IsRepaired() && step != 3)
            // {
            //     introStep = 3;
            //     IntroStep(introStep);
            //     return;
            // }

            switch (step)
            {
                case 0:
                {
                    if (thrusters.IsRepaired() && !hyperdrive.IsRepaired())
                    {
                        introStep = 2;
                        IntroStep(introStep);
                    }

                    if (hyperdrive.IsRepaired())
                    {
                        introStep = 3;
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

                    if (questIndicatorOffScreen)
                        questIndicatorOffScreen.enabled = true;

                    if (questIndicatorOnScreen)
                        questIndicatorOnScreen.enabled = true;

                    _upgradeManager.onRepairApplied.AddListener(OnRepairApplied);
                }
                    break;
                case 2:
                {
                    StopAllCoroutines();

                    if (hyperdrive.IsRepaired())
                    {
                        introStep = 3;
                        IntroStep(introStep);
                        return;
                    }

                    dof.enabled.value = false;
                    damageEffects.enabled = true;
                    blackScreen.enabled = false;

                    if (questIndicatorOffScreen)
                        questIndicatorOffScreen.enabled = false;

                    if (questIndicatorOnScreen)
                        questIndicatorOnScreen.enabled = false;

                    _uiManager.SetQuest("Repair the hyperdrive");
                }
                    break;
                case 3:
                {
                    StopAllCoroutines();

                    dof.enabled.value = false;
                    damageEffects.enabled = true;
                    blackScreen.enabled = false;

                    if (questIndicatorOffScreen)
                        questIndicatorOffScreen.enabled = false;

                    if (questIndicatorOnScreen)
                        questIndicatorOnScreen.enabled = false;

                    _uiManager.SetQuest("Hyperdrive repaired - Escape (hold H while flying)!");
                }
                    break;
            }
        }
    }
}