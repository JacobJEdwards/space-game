using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Serialization;

namespace HUDIndicator {

	[AddComponentMenu("HUD Indicator/Indicator Off Screen")]
	public class IndicatorOffScreen : Indicator {

		public IndicatorIconStyle style;
		public bool showArrow = true;
		public IndicatorArrowStyle arrowStyle;

		protected override void CreateIndicatorCanvas(IndicatorRenderer renderer) {
			var indicatorCanvasOffScreen = new IndicatorCanvasOffScreen();
			indicatorCanvasOffScreen.Create(this, renderer, uiCamera);

			IndicatorsCanvas.Add(renderer, indicatorCanvasOffScreen);
		}
	}
}