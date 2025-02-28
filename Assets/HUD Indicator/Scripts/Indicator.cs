using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;


namespace HUDIndicator {

    public abstract class Indicator : MonoBehaviour {

        public bool visible = true;
        [SerializeField] private List<IndicatorRenderer> renderers = new List<IndicatorRenderer>();
        [SerializeField] public Camera uiCamera;
        
        protected readonly Dictionary<IndicatorRenderer, IndicatorCanvas> IndicatorsCanvas = new();

		private void Start() {
            if (renderers.Count == 0) {
                var renderersInScene = FindObjectsByType<IndicatorRenderer>(FindObjectsSortMode.None);

                if (renderersInScene.Length > 0) {
                    renderers = renderersInScene.ToList();
				}
                else {
                    Debug.LogError("No IndicatorRenderer found in scene");
                }
			}
            
            foreach (var r in renderers) {
                CreateIndicatorCanvas(r);
            }
        }

        public List<IndicatorRenderer> GetRenderers() {
            return renderers;
        }

        public void SetRenderer(IndicatorRenderer renderer) {
			renderers = new List<IndicatorRenderer> {
				renderer
			};
		}

        public void SetRenderer(List<IndicatorRenderer> renderers) {
            this.renderers = renderers;
        }

		private void Update() {
            foreach(var element in IndicatorsCanvas) {
                element.Value.Update();
            }
        }

        private void OnEnable() {
            foreach(var element in IndicatorsCanvas) {
                element.Value.OnEnable();
            }
        }

        private void OnDisable() {
            foreach(var element in IndicatorsCanvas) {
                element.Value.OnDisable();
            }
        }

		private void OnDestroy() {
            foreach(KeyValuePair<IndicatorRenderer, IndicatorCanvas> element in IndicatorsCanvas) {
                DestroyIndicatorCanvas(element.Key);
            }
		}

        protected abstract void CreateIndicatorCanvas(IndicatorRenderer renderer);

        private void DestroyIndicatorCanvas(IndicatorRenderer renderer) {
            if(IndicatorsCanvas.TryGetValue(renderer, out var canvas)) {
                canvas.Destroy();
			}
		}
    }
}
