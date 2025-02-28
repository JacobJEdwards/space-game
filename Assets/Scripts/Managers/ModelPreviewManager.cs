#nullable enable

using System.Collections.Generic;
using UnityEngine;

namespace Managers
{
    public class ModelPreviewManager : MonoBehaviour
    {
        public static ModelPreviewManager Instance { get; private set; } = null!;

        [SerializeField] private Camera previewCamera = null!;
        private readonly Dictionary<GameObject, (GameObject, RenderTexture)> _renderTextures = new();

        private void Awake()
        {
            if (!Instance)
            {
                Instance = this;
                DontDestroyOnLoad(gameObject);
            }
            else
            {
                Destroy(gameObject);
            }
        }

        private void SetModel(GameObject model)
        {
            var newModel = Instantiate(model, previewCamera.transform);

            newModel.transform.localPosition = Vector3.zero;
            newModel.transform.localRotation = Quaternion.identity;
            newModel.transform.Rotate(Vector3.up, 90f);

            SetLayerRecursively(newModel, LayerMask.NameToLayer("Preview"));

            PositionModelInView(newModel);

            var texture = RecreateRenderTexture(newModel);

            _renderTextures[model] = (newModel, texture);
        }

        private static RenderTexture RecreateRenderTexture(GameObject model)
        {
            var renderTexture = new RenderTexture(512, 512, 24, RenderTextureFormat.ARGB32);
            renderTexture.Create();

            return renderTexture;
        }

        private void PositionModelInView(GameObject model)
        {
            var bounds = new Bounds();
            var boundsInitialized = false;

            var renderers = model.GetComponentsInChildren<Renderer>();

            if (renderers.Length == 0) return;

            foreach (var renderer in renderers)
            {
                if (!boundsInitialized)
                {
                    bounds = renderer.bounds;
                    boundsInitialized = true;
                }
                else
                {
                    bounds.Encapsulate(renderer.bounds);
                }
            }

            if (!boundsInitialized) return;

            var modelCenter = bounds.center;

            var maxSize = Mathf.Max(bounds.size.x, bounds.size.y, bounds.size.z);
            var distance = (maxSize / 2.0f) / Mathf.Tan(previewCamera.fieldOfView * 0.5f * Mathf.Deg2Rad);

            model.transform.position = previewCamera.transform.position + previewCamera.transform.forward * distance;

            var offsetToCenter = modelCenter - model.transform.position;
            model.transform.position -= offsetToCenter;

            model.transform.position += previewCamera.transform.forward * (-maxSize * 0.1f);
        }

        private static void CloneVisualHierarchy(GameObject source, Transform parent)
        {
            var newObj = new GameObject(source.name);
            newObj.transform.SetParent(parent);

            newObj.transform.localPosition = source.transform.localPosition;
            newObj.transform.localRotation = source.transform.localRotation;
            newObj.transform.localScale = source.transform.localScale;

            var sourceRenderer = source.GetComponent<Renderer>();
            if (sourceRenderer)
            {
                // Clone the renderer and its materials
                var newRenderer = newObj.AddComponent(sourceRenderer.GetType()) as Renderer;

                if (newRenderer)
                {
                    Material[] sharedMaterials = sourceRenderer.sharedMaterials;
                    newRenderer.sharedMaterials = sharedMaterials;
                    newRenderer.shadowCastingMode = sourceRenderer.shadowCastingMode;
                    newRenderer.receiveShadows = sourceRenderer.receiveShadows;

                    switch (sourceRenderer)
                    {
                        // If it's a skinned mesh renderer, copy the mesh and bones
                        case SkinnedMeshRenderer sourceSmr:
                        {
                            var newSmr = newRenderer as SkinnedMeshRenderer;

                            if (newSmr) newSmr.sharedMesh = sourceSmr.sharedMesh;
                            break;
                        }
                        case MeshRenderer:
                        {
                            // For mesh renderers, add mesh filter and copy the mesh
                            var sourceMf = source.GetComponent<MeshFilter>();
                            if (sourceMf)
                            {
                                var newMf = newObj.AddComponent<MeshFilter>();
                                newMf.sharedMesh = sourceMf.sharedMesh;
                            }

                            break;
                        }
                    }
                }
            }

            // Recursively clone all children
            foreach (Transform child in source.transform)
            {
                CloneVisualHierarchy(child.gameObject, newObj.transform);
            }
        }

        private static void SetLayerRecursively(GameObject obj, int layer)
        {
            obj.layer = layer;
            foreach (Transform child in obj.transform)
            {
                SetLayerRecursively(child.gameObject, layer);
            }
        }

        public Texture2D ToTexture2D(GameObject model)
        {
            if (!_renderTextures.ContainsKey(model))
                SetModel(model);

            var (previewModel, renderTexture) = _renderTextures[model];

            previewModel.SetActive(true);

            previewCamera.targetTexture = renderTexture;

            previewCamera.Render();

            var texture = new Texture2D(renderTexture.width, renderTexture.height, TextureFormat.RGBA32, false);
            RenderTexture.active = renderTexture;

            texture.ReadPixels(new Rect(0, 0, renderTexture.width, renderTexture.height), 0, 0);
            texture.Apply();

            RenderTexture.active = null;

            previewModel.SetActive(false);

            return texture;
        }
    }
}