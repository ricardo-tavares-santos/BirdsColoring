using UnityEngine;
using System.Collections;
using UnityEngine.Advertisements;

public class ImageSelection : BasePanel {

	private BaseItem itemName;
	public GameObject[] images;
	public GameObject tick;

	void Start()
	{
		GoogleAnalytics.LogScreenView("ImageSelection_BirdsColoring");

		InitializeImagesArray();
	}
		
	public void InitializeImagesArray ()
	{
		int i = 0;
		foreach (var Key in GameManager.Instance.imagesList.Keys) {
			if (!GameManager.Instance.imagesList [Key].isLocked) {
				images [i].transform.GetChild (0).gameObject.SetActive (false);
			}
			i++;
		}
	}

	public void ImageClicked(GameObject image)
	{
		itemName = GameManager.Instance.imagesList[image.name];
		if(itemName.isLocked)
		{
			AdvertisementManager.Instance.ShowAd(eAdsLoop.INCENTIVIZED);
		}
		else
		{
			SoundManager.Instance.PlayButtonClickSound();
			tick.transform.parent = image.transform;
			tick.transform.localPosition = new Vector3(144f,-157f,0f);
			tick.SetActive(true);
			GameManager.Instance.isImageClicked=true;
			GameManager.Instance.imageIndex = int.Parse(image.name);
			//Invoke("ShowNextPanel",0.2f);
			GameNavigationController.Instance.PushPanel(GameNavigationController.GameState.ColoringScene);
		}
	}

	void ShowNextPanel()
	{
		GameNavigationController.Instance.PushPanel(GameNavigationController.GameState.ColoringScene);
	}

	public void HomeBtnClicked()
	{
		SoundManager.Instance.PlayButtonClickSound();
		GameNavigationController.Instance.PopMenuToState(GameNavigationController.GameState.MainMenu);
	}	

	void RewardUser()
	{
		itemName.isLocked = false;
		InitializeImagesArray();
	}

	public void FacebookBtnClicked()
	{
		SoundManager.Instance.PlayButtonClickSound();
		NativeShare.Instance.ShareScreenshotWithText("Awesome! Best Coloring game ever https://play.google.com/store/apps/details?id=com.pumplum.birdscoloring");
	}
}
