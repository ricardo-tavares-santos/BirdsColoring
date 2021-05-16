using UnityEngine;
using System.Collections;
using System.IO;

public class CaptureGalleryScreenshot : Singleton<CaptureGalleryScreenshot> {
	
	// Use this for initialization
	void Start () {
		ScreenshotManager.ScreenshotFinishedSaving += ScreenshotSaved;	
		ScreenshotManager.ImageFinishedSaving += ImageSaved;
	}

	public void TakeScreenshot()
	{
		StartCoroutine(ScreenshotManager.Save(Constants.GAME_NAME + " "+ System.DateTime.Now.ToString("yyyy-MM-dd_HH-mm-ss"), Constants.GAME_NAME, true));
	}

	void ScreenshotSaved()
	{
		Debug.Log ("Screenshot finished saving");
	}
	
	void ImageSaved()
	{
		Debug.Log ("Finished saving Image");
	}
}
