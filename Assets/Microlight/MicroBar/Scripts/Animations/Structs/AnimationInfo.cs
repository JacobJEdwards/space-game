using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

namespace Microlight.MicroBar
{
    // ****************************************************************************************************
    // Stores data about animation for easier passing of the data
    // ****************************************************************************************************
    internal readonly struct AnimationInfo
    {
        public IReadOnlyList<AnimCommand> Commands { get; }

        public bool IsImage { get; }

        public Image TargetImage { get; }

        public SpriteRenderer TargetSprite { get; }

        public Transform TargetSpriteMask { get; }

        public MicroBar Bar { get; }

        public MicroBarAnimation Animation { get; }

        internal AnimationInfo(IReadOnlyList<AnimCommand> commands, Image target, MicroBar bar,
            MicroBarAnimation animation)
        {
            this.Commands = commands;
            IsImage = true;
            TargetImage = target;
            TargetSprite = null;
            TargetSpriteMask = null;
            this.Bar = bar;
            this.Animation = animation;
        }

        internal AnimationInfo(IReadOnlyList<AnimCommand> commands, SpriteRenderer target, Transform targetMask,
            MicroBar bar, MicroBarAnimation animation)
        {
            this.Commands = commands;
            IsImage = false;
            TargetImage = null;
            TargetSprite = target;
            TargetSpriteMask = targetMask;
            this.Bar = bar;
            this.Animation = animation;
        }
    }
}