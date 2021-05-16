using UnityEngine;
using System.Collections;
using UnityEngine.UI;

namespace unitycoder_MobilePaint
{
	
	public class ToggleCustomShapeModeUI : MonoBehaviour 
	{
		public MobilePaint mobilePaint;
		public GameObject customBrushPanel;

		void Start () 
		{

			if (mobilePaint==null) Debug.LogError("No MobilePaint assigned at "+transform.name);
			if (customBrushPanel==null) Debug.LogError("No customBrushPanel assigned at "+transform.name);

			GetComponent<Toggle>().onValueChanged.AddListener(delegate {this.SetMode();});
		}


		public void SetMode()
		{
			if (GetComponent<Toggle>().isOn)
			{
				customBrushPanel.SetActive(true);
				mobilePaint.SetDrawModeShapes();
			}else{
				customBrushPanel.SetActive(false);
			}

		}

	}
}