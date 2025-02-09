#if UNITY_EDITOR
using Microlight.MicroEditor;
using UnityEditor;
using UnityEngine;

namespace Microlight.MicroBar
{
    // ****************************************************************************************************
    // For MicroBar menu in editor or right clicking in hierarchy
    // ****************************************************************************************************
    internal class MicroBar_EditorMenu : Editor
    {
        private static string GetPrefabsFolder()
        {
            return MicroEditor_AssetUtility.FindFolderRecursively("Assets", "MicroBar") + "/Prefabs";
        }

        private static void InstantiateBar(GameObject bar)
        {
            bar = Instantiate(bar); // Instantiate
            bar.name = "HealthBar"; // Change name
            if (Selection.activeGameObject != null)
                // Make child if some object is selected
                bar.transform.SetParent(Selection.activeGameObject.transform, false);
        }

        #region Image Bars

        // BLANK ##################################################
        [MenuItem("GameObject/Microlight/MicroBar/Blank (Image)", false, 100)]
        private static void AddBlankImageBar()
        {
            var go = MicroEditor_AssetUtility.GetPrefab($"{GetPrefabsFolder()}/ImageBars", "Blank_ImageMicroBar");
            if (go == null)
                return;
            InstantiateBar(go);
        }

        // SIMPLE ##################################################
        [MenuItem("GameObject/Microlight/MicroBar/Simple (Image)", false, 101)]
        private static void AddSimpleImageBar()
        {
            var go = MicroEditor_AssetUtility.GetPrefab($"{GetPrefabsFolder()}/ImageBars", "Simple_ImageMicroBar");
            if (go == null)
                return;
            InstantiateBar(go);
        }

        // DELAYED ##################################################
        [MenuItem("GameObject/Microlight/MicroBar/Delayed (Image)", false, 102)]
        private static void AddDelayedImageBar()
        {
            var go = MicroEditor_AssetUtility.GetPrefab($"{GetPrefabsFolder()}/ImageBars", "Delayed_ImageMicroBar");
            if (go == null)
                return;
            InstantiateBar(go);
        }

        // DISAPPEAR ##################################################
        [MenuItem("GameObject/Microlight/MicroBar/Disappear (Image)", false, 103)]
        private static void AddDisappearImageBar()
        {
            var go = MicroEditor_AssetUtility.GetPrefab($"{GetPrefabsFolder()}/ImageBars", "Disappear_ImageMicroBar");
            if (go == null)
                return;
            InstantiateBar(go);
        }

        // IMPACT ##################################################
        [MenuItem("GameObject/Microlight/MicroBar/Impact (Image)", false, 104)]
        private static void AddImpactImageBar()
        {
            var go = MicroEditor_AssetUtility.GetPrefab($"{GetPrefabsFolder()}/ImageBars", "Impact_ImageMicroBar");
            if (go == null)
                return;
            InstantiateBar(go);
        }

        // PUNCH ##################################################
        [MenuItem("GameObject/Microlight/MicroBar/Punch (Image)", false, 105)]
        private static void AddPunchImageBar()
        {
            var go = MicroEditor_AssetUtility.GetPrefab($"{GetPrefabsFolder()}/ImageBars", "Punch_ImageMicroBar");
            if (go == null)
                return;
            InstantiateBar(go);
        }

        // SHAKE ##################################################
        [MenuItem("GameObject/Microlight/MicroBar/Shake (Image)", false, 106)]
        private static void AddShakeImageBar()
        {
            var go = MicroEditor_AssetUtility.GetPrefab($"{GetPrefabsFolder()}/ImageBars", "Shake_ImageMicroBar");
            if (go == null)
                return;
            InstantiateBar(go);
        }

        #endregion

        #region Sprite Bars

        // BLANK ##################################################
        [MenuItem("GameObject/Microlight/MicroBar/Blank (Sprite)", false, 200)]
        private static void AddBlankSpriteBar()
        {
            var go = MicroEditor_AssetUtility.GetPrefab($"{GetPrefabsFolder()}/SpriteBars", "Blank_SpriteMicroBar");
            if (go == null)
                return;
            InstantiateBar(go);
        }

        // SIMPLE ##################################################
        [MenuItem("GameObject/Microlight/MicroBar/Simple (Sprite)", false, 201)]
        private static void AddSimpleSpriteBar()
        {
            var go = MicroEditor_AssetUtility.GetPrefab($"{GetPrefabsFolder()}/SpriteBars", "Simple_SpriteMicroBar");
            if (go == null)
                return;
            InstantiateBar(go);
        }

        // DELAYED ##################################################
        [MenuItem("GameObject/Microlight/MicroBar/Delayed (Sprite)", false, 202)]
        private static void AddDelayedSpriteBar()
        {
            var go = MicroEditor_AssetUtility.GetPrefab($"{GetPrefabsFolder()}/SpriteBars", "Delayed_SpriteMicroBar");
            if (go == null)
                return;
            InstantiateBar(go);
        }

        // DISAPPEAR ##################################################
        [MenuItem("GameObject/Microlight/MicroBar/Disappear (Sprite)", false, 203)]
        private static void AddDisappearSpriteBar()
        {
            var go = MicroEditor_AssetUtility.GetPrefab($"{GetPrefabsFolder()}/SpriteBars", "Disappear_SpriteMicroBar");
            if (go == null)
                return;
            InstantiateBar(go);
        }

        // IMPACT ##################################################
        [MenuItem("GameObject/Microlight/MicroBar/Impact (Sprite)", false, 204)]
        private static void AddImpactSpriteBar()
        {
            var go = MicroEditor_AssetUtility.GetPrefab($"{GetPrefabsFolder()}/SpriteBars", "Impact_SpriteMicroBar");
            if (go == null)
                return;
            InstantiateBar(go);
        }

        // PUNCH ##################################################
        [MenuItem("GameObject/Microlight/MicroBar/Punch (Sprite)", false, 205)]
        private static void AddPunchSpriteBar()
        {
            var go = MicroEditor_AssetUtility.GetPrefab($"{GetPrefabsFolder()}/SpriteBars", "Punch_SpriteMicroBar");
            if (go == null)
                return;
            InstantiateBar(go);
        }

        // SHAKE ##################################################
        [MenuItem("GameObject/Microlight/MicroBar/Shake (Sprite)", false, 206)]
        private static void AddShakeSpriteBar()
        {
            var go = MicroEditor_AssetUtility.GetPrefab($"{GetPrefabsFolder()}/SpriteBars", "Shake_SpriteMicroBar");
            if (go == null)
                return;
            InstantiateBar(go);
        }

        #endregion

        #region Simple Bars

        // IMAGE ##################################################
        [MenuItem("GameObject/Microlight/MicroBar/Simple Bar (Image)", false, 10)]
        private static void AddImageSimpleModeBar()
        {
            var go = MicroEditor_AssetUtility.GetPrefab($"{GetPrefabsFolder()}/SimpleBars", "Image_SimpleMicroBar");
            if (go == null)
                return;
            InstantiateBar(go);
        }

        // SPRITE ##################################################
        [MenuItem("GameObject/Microlight/MicroBar/Simple Bar (Sprite)", false, 11)]
        private static void AddSpriteSimpleModeBar()
        {
            var go = MicroEditor_AssetUtility.GetPrefab($"{GetPrefabsFolder()}/SimpleBars", "Sprite_SimpleMicroBar");
            if (go == null)
                return;
            InstantiateBar(go);
        }

        #endregion

        #region Validators

        [MenuItem("GameObject/Microlight/MicroBar/Simple Bar (Image)", true, 10)]
        [MenuItem("GameObject/Microlight/MicroBar/Blank (Image)", true, 100)]
        [MenuItem("GameObject/Microlight/MicroBar/Simple (Image)", true, 101)]
        [MenuItem("GameObject/Microlight/MicroBar/Delayed (Image)", true, 102)]
        [MenuItem("GameObject/Microlight/MicroBar/Disappear (Image)", true, 103)]
        [MenuItem("GameObject/Microlight/MicroBar/Impact (Image)", true, 104)]
        [MenuItem("GameObject/Microlight/MicroBar/Punch (Image)", true, 105)]
        [MenuItem("GameObject/Microlight/MicroBar/Shake (Image)", true, 106)]
        private static bool AddImageBar_Validate()
        {
            return Selection.activeGameObject && Selection.activeGameObject.GetComponentInParent<Canvas>();
        }

        [MenuItem("GameObject/Microlight/MicroBar/Simple Bar (Sprite)", true, 11)]
        [MenuItem("GameObject/Microlight/MicroBar/Blank (Sprite)", true, 200)]
        [MenuItem("GameObject/Microlight/MicroBar/Simple (Sprite)", true, 201)]
        [MenuItem("GameObject/Microlight/MicroBar/Delayed (Sprite)", true, 202)]
        [MenuItem("GameObject/Microlight/MicroBar/Disappear (Sprite)", true, 203)]
        [MenuItem("GameObject/Microlight/MicroBar/Impact (Sprite)", true, 204)]
        [MenuItem("GameObject/Microlight/MicroBar/Punch (Sprite)", true, 205)]
        [MenuItem("GameObject/Microlight/MicroBar/Shake (Sprite)", true, 206)]
        private static bool AddSpriteBar_Validate()
        {
            if (Selection.activeGameObject == null)
                return true;
            var hasCanvas = Selection.activeGameObject.GetComponentInParent<Canvas>() != null;
            return !hasCanvas;
        }

        #endregion
    }
}
#endif