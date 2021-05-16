using UnityEngine;
using System.Collections;
using UnityEngine.UI;

namespace unitycoder_MobilePaint
{

	public class BrushSizeUI : MonoBehaviour {

		public MobilePaint mobilePaint;
		private Slider slider;

		void Start () 
		{
			if (mobilePaint==null) Debug.LogError("No MobilePaint assigned at "+transform.name);

			slider = GetComponent<Slider>();

			slider.value = mobilePaint.brushSize;

			slider.onValueChanged.AddListener((value) => { mobilePaint.brushSize = (int)value; });
		}
	}
}