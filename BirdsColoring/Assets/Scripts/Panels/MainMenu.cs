using UnityEngine;
using System.Collections;

public class MainMenu : BasePanel {

	public GameObject musicOn;
	public GameObject musicOff;
	
	void Start()
	{
		GoogleAnalytics.LogScreenView("MainMenu_BirdsColoring");

		GameObject.FindGameObjectWithTag("Camera").GetComponent<Camera>().orthographicSize = 1f;

		GameManager.Instance.isImageClicked = false;
		GameManager.Instance.colorIndex = 0;
		GameManager.Instance.categoryIndex = 0;
	

		if (AudioListener.volume == 0.0f)
		{
			musicOn.SetActive(false);
			musicOff.SetActive(true);
		}
		else
		{
			musicOn.SetActive(true);
			musicOff.SetActive(false);		
		}
	}
	
	public void PlayBtnClicked()
	{
		SoundManager.Instance.PlayButtonClickSound();
		GameNavigationController.Instance.PushPanel (GameNavigationController.GameState.ImageSelection);
	}
	
	public void RateUsBtnClicked()
	{
		SoundManager.Instance.PlayButtonClickSound();
		Application.OpenURL("market://details?id=com.pumplum.birdscoloring");
	}
	
	public void MorefunBtnClicked()
	{
		SoundManager.Instance.PlayButtonClickSound();
		Application.OpenURL("market://search?q=pub:Pumplum+Games");
	}

	public void StoreBtnClicked()
	{
		SoundManager.Instance.PlayButtonClickSound();
		GameNavigationController.Instance.PushPanel (GameNavigationController.GameState.Store);
	}

	public void FacebookBtnClicked()
	{
		SoundManager.Instance.PlayButtonClickSound();
		NativeShare.Instance.ShareScreenshotWithText("Awesome! Best Coloring game ever https://play.google.com/store/apps/details?id=com.pumplum.birdscoloring");
	}
		
	public void SoundOnBtnClicked()
	{
		AudioListener.volume = 0f;
		musicOn.SetActive(false);
		musicOff.SetActive(true);
	}
	
	public void SoundOffBtnClicked()
	{
		AudioListener.volume = 1.0f;
		musicOn.SetActive(true);
		musicOff.SetActive(false);
	}
}
