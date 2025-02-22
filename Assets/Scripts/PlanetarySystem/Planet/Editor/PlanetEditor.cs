using System;
using UnityEditor;
using UnityEngine;
using Object = UnityEngine.Object;

namespace PlanetarySystem.Planet.Editor
{
    using Editor = UnityEditor.Editor;

    [CustomEditor(typeof(Planet))]
    public class PlanetEditor : Editor
    {
        private Editor _colourEditor;
        private Planet _planet;
        private Editor _shapeEditor;

        private void OnEnable()
        {
            _planet = (Planet)target;
        }

        public override void OnInspectorGUI()
        {
            using (var check = new EditorGUI.ChangeCheckScope())
            {
                base.OnInspectorGUI();
                if (check.changed) _planet.GeneratePlanet();
            }

            if (GUILayout.Button("Generate Planet")) _planet.GeneratePlanet();

            DrawSettingsEditor(_planet.shapeSettings, _planet.OnShapeSettingsUpdated, ref _planet.shapeSettingsFoldout,
                ref _shapeEditor);
            DrawSettingsEditor(_planet.colourSettings, _planet.OnColourSettingsUpdated,
                ref _planet.colourSettingsFoldout,
                ref _colourEditor);
        }

        private static void DrawSettingsEditor(Object settings, Action onSettingsUpdated, ref bool foldout,
            ref Editor editor)
        {
            if (!settings) return;

            foldout = EditorGUILayout.InspectorTitlebar(foldout, settings);

            using var check = new EditorGUI.ChangeCheckScope();

            if (!foldout) return;

            CreateCachedEditor(settings, null, ref editor);
            editor.OnInspectorGUI();

            if (!check.changed) return;

            onSettingsUpdated?.Invoke();
        }
    }
}