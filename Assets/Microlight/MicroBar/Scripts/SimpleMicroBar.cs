using System;
using DG.Tweening;
using UnityEngine;
using UnityEngine.Rendering;
using UnityEngine.UI;

namespace Microlight.MicroBar
{
    // ****************************************************************************************************
    // Class for storing data about MicroBar animation in simple mode
    // ****************************************************************************************************

    [Serializable]
    public class SimpleMicroBar
    {
        // General
        [SerializeField] private RenderType _renderType;
        [SerializeField] private bool _isAnimated = true; // Will the bar be animated

        // SpriteRenderer
        [SerializeField] private SpriteRenderer _srBackground;
        [SerializeField] private SpriteRenderer _srPrimaryBar;
        [SerializeField] private SpriteRenderer _srGhostBar;

        // Image
        [SerializeField] private Image _uiBackground;
        [SerializeField] private Image _uiPrimaryBar;
        [SerializeField] private Image _uiGhostBar;

        // Colors
        [SerializeField] private bool _adaptiveColor = true; // Does health bar uses adaptive color based on current hp

        [SerializeField]
        private Color
            _barPrimaryColor =
                new(1f, 1f, 1f); // Color of the main health bar, also used as a color for full health in adaptive color

        [SerializeField]
        private Color _barAdaptiveColor = new(1f, 0f, 0f); // Color that health changes to as it gets lower

        // Ghost bar
        [SerializeField] private bool _useGhostBar = true; // Is ghost bar used

        [SerializeField]
        private bool _dualGhostBars; // Are ghost bars two separate bars for healing and damaging or single bar for both

        [SerializeField]
        private Color
            _ghostBarDamageColor = new(1f, 0f, 0f); // Color of ghost bar in single mode and when hurt in dual mode

        [SerializeField] private Color _ghostBarHealColor = new(1f, 1f, 1f); // Color of ghost bar when healed

        // Duplicate fields are here to have more control over how they are displayed in editor
        // Putting them in separate class/field only makes things uglier
        // Damage Animation
        [SerializeField]
        private SimpleAnim _damageAnim; // Type of animation that will be played when the bar is damaged

        [SerializeField] [Range(0.01f, 1f)] private float _damageAnimDuration = 0.5f; // Duration of the animation

        [SerializeField] [Range(0f, 1f)]
        private float _damageAnimDelay; // How long will animation wait before following ghost bar

        [SerializeField] private Color _damageFlashColor = new(1f, 1f, 1f, 1f);
        [SerializeField] [Range(0f, 1f)] private float _damageAnimStrength = 0.5f;

        // Heal Animation
        [SerializeField] private SimpleAnim _healAnim; // Type of animation that will be played when the bar is healed
        [SerializeField] [Range(0.01f, 1f)] private float _healAnimDuration = 0.5f; // Duration of the animation
        [SerializeField] [Range(0f, 1f)] private float _healAnimDelay; // Delay before animation starts playing
        [SerializeField] private Color _healFlashColor = new(1f, 1f, 1f, 1f);
        [SerializeField] [Range(0f, 1f)] private float _healAnimStrength = 0.5f;
        internal RenderType RenderType => _renderType;
        internal bool IsAnimated => _isAnimated;
        internal SpriteRenderer SRBackground => _srBackground;
        internal SpriteRenderer SRPrimaryBar => _srPrimaryBar;
        internal SpriteMask SRPrimaryBarMask { get; private set; }
        internal SpriteRenderer SRGhostBar => _srGhostBar;
        internal SpriteMask SRGhostBarMask { get; private set; }
        internal Image UIBackground => _uiBackground;
        internal Image UIPrimaryBar => _uiPrimaryBar;
        internal Image UIGhostBar => _uiGhostBar;
        internal bool AdaptiveColor => _adaptiveColor;
        internal Color BarPrimaryColor => _barPrimaryColor;
        internal Color BarAdaptiveColor => _barAdaptiveColor;
        internal bool UseGhostBar => _useGhostBar;
        internal bool DualGhostBars => _dualGhostBars;
        internal Color GhostBarDamageColor => _ghostBarDamageColor;
        internal Color GhostBarHealColor => _ghostBarHealColor;
        internal SimpleAnim DamageAnim => _damageAnim;
        internal float DamageAnimDuration => _damageAnimDuration;
        internal float DamageAnimDelay => _damageAnimDelay;
        internal Color DamageFlashColor => _damageFlashColor;
        internal float DamageAnimStrength => _damageAnimStrength;
        internal SimpleAnim HealAnim => _healAnim;
        internal float HealAnimDuration => _healAnimDuration;
        internal float HealAnimDelay => _healAnimDelay;
        internal Color HealFlashColor => _healFlashColor;
        internal float HealAnimStrength => _healAnimStrength;

        // Variables
        internal MicroBar ParentBar { get; private set; }
        public Sequence Sequence { get; private set; }

        internal bool Initialize(MicroBar bar)
        {
            if (RenderType == RenderType.Image)
            {
                if (UIBackground == null)
                {
                    Debug.LogError("[MicroBar] RenderType set to 'Image' but 'UIBackground' is null");
                    return false;
                }

                if (UIPrimaryBar == null)
                {
                    Debug.LogError("[MicroBar] RenderType set to 'Image' but 'UIPrimaryBar' is null");
                    return false;
                }

                if (IsAnimated && UseGhostBar && UIGhostBar == null)
                {
                    Debug.LogError("[MicroBar] RenderType set to 'Image' but 'UIGhostBar' is null");
                    return false;
                }
            }

            if (RenderType == RenderType.Sprite)
            {
                if (SRBackground == null)
                {
                    Debug.LogError("[MicroBar] RenderType set to 'Sprite' but 'SRBackground' is null");
                    return false;
                }

                if (SRPrimaryBar == null)
                {
                    Debug.LogError("[MicroBar] RenderType set to 'Sprite' but 'SRPrimaryBar' is null");
                    return false;
                }

                if (SRPrimaryBar.GetComponent<SortingGroup>() == null)
                {
                    Debug.LogError("[MicroBar] Couldn't find the 'SortingGroup' for the 'SRPrimaryBar'");
                    return false;
                }

                SRPrimaryBarMask = SRPrimaryBar.GetComponentInChildren<SpriteMask>();
                if (SRPrimaryBarMask == null)
                {
                    Debug.LogError("[MicroBar] Couldn't find the 'SpriteMask' for the 'SRPrimaryBar'");
                    return false;
                }

                if (IsAnimated && UseGhostBar && SRGhostBar == null)
                {
                    Debug.LogError("[MicroBar] RenderType set to 'Sprite' but 'SRGhostBar' is null");
                    return false;
                }

                if (IsAnimated && UseGhostBar)
                {
                    if (SRGhostBar.GetComponent<SortingGroup>() == null)
                    {
                        Debug.LogError("[MicroBar] Couldn't find the 'SortingGroup' for the 'SRPrimaryBar'");
                        return false;
                    }

                    SRGhostBarMask = SRGhostBar.GetComponentInChildren<SpriteMask>();
                    if (SRGhostBarMask == null)
                    {
                        Debug.LogError("[MicroBar] Couldn't find the 'SpriteMask' for the 'SRGhostBar'");
                        return false;
                    }
                }
            }

            ParentBar = bar;
            InitializeValues();
            bar.OnBarUpdate += Update;
            bar.BarDestroyed += Destroy;

            return true;
        }

        // Sets some starting values
        private void InitializeValues()
        {
            var isSprite = RenderType == RenderType.Sprite;

            SilentUpdate();

            // Update colors
            if (AdaptiveColor)
            {
                SimpleAnimBuilder.SetAdaptiveBarColor(this, false);
            }
            else
            {
                if (isSprite)
                    SRPrimaryBar.color = BarPrimaryColor;
                else
                    UIPrimaryBar.color = BarPrimaryColor;
            }

            // Ghost bar
            if ((IsAnimated && !UseGhostBar) || !IsAnimated)
            {
                if (SRGhostBar != null) SRGhostBar.gameObject.SetActive(false);
                if (UIGhostBar != null) UIGhostBar.gameObject.SetActive(false);
            }
            else if (IsAnimated && UseGhostBar)
            {
                if (isSprite)
                    SRGhostBar.color = GhostBarDamageColor;
                else
                    UIGhostBar.color = GhostBarDamageColor;
            }
        }

        // animationType is there only to connect to the event, but is not actually connected
        private void Update(bool skipAnimation, UpdateAnim animationType)
        {
            // Always kill when bar is updating, because we dont want to have for example active damage animation if heal animation is active
            if (Sequence.IsActive())
            {
                Sequence.Kill();
                Sequence = null;
            }

            // Decide if animation should be skipped
            var isHealAnimation = ParentBar.CurrentValue > ParentBar.PreviousValue;
            var animToBePlayed = isHealAnimation ? HealAnim : DamageAnim;
            if (skipAnimation || !IsAnimated || animToBePlayed == SimpleAnim.None)
            {
                SilentUpdate();
                return;
            }

            Sequence = SimpleAnimBuilder.BuildAnimation(this);
        }

        // Silently updates bars to the values without animating
        private void SilentUpdate()
        {
            if (ParentBar == null)
            {
                Debug.LogError("[MicroBar] Missing reference to the 'ParentBar'");
                return;
            }

            if (RenderType == RenderType.Image)
            {
                UIPrimaryBar.fillAmount = ParentBar.HPPercent;
                if (UseGhostBar) UIGhostBar.fillAmount = ParentBar.HPPercent;
            }
            else if (RenderType == RenderType.Sprite)
            {
                SRPrimaryBarMask.transform.localScale = new Vector3(ParentBar.HPPercent,
                    SRPrimaryBarMask.transform.localScale.y, SRPrimaryBarMask.transform.localScale.z);
                if (UseGhostBar)
                    SRGhostBarMask.transform.localScale = new Vector3(ParentBar.HPPercent,
                        SRGhostBarMask.transform.localScale.y, SRGhostBarMask.transform.localScale.z);
            }

            if (AdaptiveColor) SimpleAnimBuilder.SetAdaptiveBarColor(this, false);
        }

        private void Destroy()
        {
            if (Sequence.IsActive())
            {
                Sequence.Kill();
                Sequence = null;
            }
        }
    }
}