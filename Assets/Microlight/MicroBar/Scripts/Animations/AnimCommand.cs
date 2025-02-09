using System;
using DG.Tweening;
using UnityEngine;

namespace Microlight.MicroBar
{
    // ****************************************************************************************************
    // Command specifying the behavior and duration of a segment of an animation.
    // ****************************************************************************************************
    [Serializable]
    public class AnimCommand
    {
        [SerializeField] private AnimExecution execution = AnimExecution.Sequence;
        [SerializeField] private AnimEffect effect = AnimEffect.Scale;
        [SerializeField] private float duration;
        [SerializeField] private float delay;
        [SerializeField] private ValueMode valueMode;
        [SerializeField] private float floatValue;
        [SerializeField] private int intValue;
        [SerializeField] private bool boolValue;
        [SerializeField] private Vector2 vector2Value;
        [SerializeField] private Vector3 vector3Value;
        [SerializeField] private Color colorValue;
        [SerializeField] [Range(0f, 1f)] private float percentValue;
        [SerializeField] private int frequency = 10;
        [SerializeField] private Ease ease = Ease.Linear;
        [SerializeField] private AnimAxis animAxis;
        [SerializeField] private TransformProperties transformProperty;
        internal AnimExecution Execution => execution;
        internal AnimEffect Effect => effect;
        internal float Duration => duration;
        internal float Delay => delay;

        // Possible end values
        internal ValueMode ValueMode => valueMode;
        internal float FloatValue => floatValue;
        internal int IntValue => intValue;
        internal bool BoolValue => boolValue;
        internal Vector2 Vector2Value => vector2Value;
        internal Vector3 Vector3Value => vector3Value;
        internal Color ColorValue => colorValue;
        internal float PercentValue => percentValue;

        // Additional settings
        internal int Frequency => frequency;
        internal Ease Ease => ease;
        internal AnimAxis AnimAxis => animAxis;
        internal TransformProperties TransformProperty => transformProperty;
    }
}