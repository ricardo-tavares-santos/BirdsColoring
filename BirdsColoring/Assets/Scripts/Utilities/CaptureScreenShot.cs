using UnityEngine;
using System.Collections;

public class CaptureScreenShot : MonoBehaviour
{
	//public GameObject cameraFlashObject;
	public GameObject[] buttons;

	public void CameraBtnClick ()
	{
		//cameraFlashObject.SetActive (true);
		foreach (GameObject btn in buttons)
			btn.SetActive (false);
		Invoke("SetCameraFlashOff",0.2f);
		Invoke("EnableButtons",0.35f);
	}
	
	void EnableButtons()
	{
		foreach (GameObject btn in buttons)
			btn.SetActive (true);
	}

	void SetCameraFlashOff(){
		//cameraFlashObject.SetActive(false);
		CaptureGalleryScreenshot.Instance.TakeScreenshot();
	}
}
