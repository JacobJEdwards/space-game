using UnityEditor;
using UnityEngine;

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
        }
    }
}