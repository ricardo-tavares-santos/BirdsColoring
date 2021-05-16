using UnityEngine;
using System.Collections;
using UnityEngine.UI;

namespace unitycoder_MobilePaint
{
	
	public class ToggleCustomPatternModeUI : MonoBehaviour 
	{
		public MobilePaint mobilePaint;
		public GameObject customPanel;

		void Start () 
		{

			if (mobilePaint==null) Debug.LogError("No MobilePaint assigned at "+transform.name);
			if (customPanel==null) Debug.LogError("No customPanel assigned at "+transform.name);

			GetComponent<Toggle>().onValueChanged.AddListener(delegate {this.SetMode();});
		}


		public void SetMode()
		{
			if (GetComponent<Toggle>().isOn)
			{
				customPanel.SetActive(true);
				mobilePaint.SetDrawModePattern();
			}else{
				customPanel.SetActive(false);
			}

		}

	}
}