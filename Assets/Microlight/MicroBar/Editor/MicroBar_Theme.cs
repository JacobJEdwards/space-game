using UnityEngine;

namespace Microlight.MicroBar
{
    // ****************************************************************************************************
    // Defines colors for the MicroBar custom editors and drawers
    // ****************************************************************************************************
    internal static class MicroBar_Theme
    {
        // Tooltips
        internal const string ExecutionTooltip =
            "Order of command execution" +
            "\nSequence - Command runs when the above command finishes" +
            "\nParallel - Command runs at the same time as the command above" +
            "\nWait - Command to wait for the specified amount of time in seconds";

        internal const string EffectTooltip =
            "Property of the target image that will be affected by animation command";

        internal const string DurationTooltip =
            "How long should this animation command last";

        internal const string DelayTooltip =
            "Amount of time in seconds command will wait before starting it's duration";

        internal const string ValueModeTooltip =
            "How will specified value affect current value of the property" +
            "\nAbsolute - New value will replace the current value" +
            "\nAdditive - New value will be added to the current value" +
            "\nMultiplicative - New value will be multiplied with the current value" +
            "\nStarting Value - Value at the start of the animation will be taken";

        internal const string EaseTooltip =
            "How will property changes behave over the time of the animation" +
            "\nCheck documentation or online on how each option behaves";

        internal const string AxisTooltip =
            "What axis will be affected by this command" +
            "\nUniform - All axis will be affected by the same value" +
            "\nXY - Affect both X and Y values";

        internal const string TransformTooltip =
            "Which transform property will be affected by the command";

        // Button colors
        internal static Color RemoveButtonColor = new(1f, 0.65f, 0.65f); // Color tint for the remove button
        internal static Color AddButtonColor = new(0.65f, 1f, 0.65f); // Color tint for the add anim command button

        internal static Color
            SelectedModeButtonColor = new(0.8f, 0.8f, 0.8f); // Color tint for the button of the selected mode

        // Anim commands colors
        internal static Color DamageAnimColorMultiplier = new(1.00f, 0.85f, 0.85f);
        internal static Color HealAnimColorMultiplier = new(0.85f, 1.00f, 0.85f);
        internal static Color ArmorAnimColorMultiplier = new(1.00f, 1.00f, 1.00f);
        internal static Color DOTAnimColorMultiplier = new(0.95f, 1.00f, 0.85f);
        internal static Color HOTAnimColorMultiplier = new(1.00f, 0.75f, 0.50f);
        internal static Color MaxHPAnimColorMultiplier = new(0.75f, 1.00f, 0.75f);
        internal static Color ReviveAnimColorMultiplier = new(1.00f, 1.00f, 0.85f);
        internal static Color DeathAnimColorMultiplier = new(0.65f, 0.65f, 0.65f);
        internal static Color CustomAnimColorMultiplier = new(1.00f, 1.00f, 1.00f);
    }
}