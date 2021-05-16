using UnityEngine;
using System.Collections;
using unitycoder_MobilePaint;

namespace unitycoder_MobilePaint_samples
{

	public class RandomPainter : MonoBehaviour {

		public MobilePaint mobilePaint;

		void Update () {
		
			// set random paint color
			mobilePaint.paintColor = new Color(Random.value,Random.value, Random.value, Random.value);

			// set random brush size
			mobilePaint.brushSize = (int)(Random.value*64);

			// draw random single pixel point
			mobilePaint.DrawCircle((int)(Random.value*Screen.width),(int)(Random.value*Screen.height));

			// random 1 pixel lines
			mobilePaint.brushSize = 1;
			mobilePaint.DrawLine(new Vector2((int)(Random.value*Screen.width),(int)(Random.value*Screen.height)), new Vector2((int)(Random.value*Screen.width),(int)(Random.value*Screen.height)));

			// set texture dirty, so it needs to be applied
			mobilePaint.textureNeedsUpdate = true;

		}
	}

}