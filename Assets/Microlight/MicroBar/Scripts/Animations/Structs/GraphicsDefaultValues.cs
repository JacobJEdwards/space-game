using UnityEngine;
using UnityEngine.UI;

namespace Microlight.MicroBar
{
    // ****************************************************************************************************
    // Stores default values for a graphics component
    // ****************************************************************************************************
    internal readonly struct GraphicsDefaultValues
    {
        internal Color Color { get; }

        internal float Fade { get; }

        internal float Fill { get; }

        internal Vector3 Position { get; }

        internal float Rotation { get; }

        internal Vector3 Scale { get; }

        internal Vector2 AnchorPosition { get; }

        internal GraphicsDefaultValues(Image image)
        {
            Color = image.color;
            Fade = image.color.a;
            Fill = image.fillAmount;
            Position = image.rectTransform.localPosition;
            Rotation = image.rectTransform.localRotation.eulerAngles.z;
            Scale = image.rectTransform.localScale;
            AnchorPosition = image.rectTransform.anchoredPosition;
        }

        internal GraphicsDefaultValues(SpriteRenderer sprite)
        {
            Color = sprite.color;
            Fade = sprite.color.a;
            Fill = sprite.transform.localScale.x;
            Position = sprite.transform.localPosition;
            Rotation = sprite.transform.localRotation.eulerAngles.z;
            Scale = sprite.transform.localScale;
            AnchorPosition = Vector2.zero;
        }
    }
}