using UnityEngine;
using System.Collections;
using UnityEngine.Advertisements;

public class Coloring : BasePanel {

	private BaseItem itemName;
	public GameObject imageTxt;
	public GameObject baseGrid;
	public GameObject uiButtonPanel;
	public GameObject mobilePaint;
	public GameObject[] brushSizes;
	public GameObject[] colors;
	public GameObject tick;
	public GameObject tick2;
	
	void Start()
	{
		GoogleAnalytics.LogScreenView("ColoringScreen_BirdsColoring");

		AdvertisementManager.Instance.ShowAd(eAdsLoop.MAINMENU);

		GameObject.FindGameObjectWithTag("ImageCamera").GetComponent<Camera>().orthographicSize = 1f;
		GameObject.FindGameObjectWithTag("PopUpCamera").GetComponent<Camera>().orthographicSize = 1f;

		InitializeColorsArray ();
	}

	public void InitializeColorsArray ()
	{
		int i = 0;
		foreach (var Key in GameManager.Instance.colorsList.Keys) {
			if (!GameManager.Instance.colorsList [Key].isLocked) {
				colors [i].transform.GetChild (0).gameObject.SetActive (false);
			}
			i++;
		}
	}

	public void ColorClicked(GameObject item)
	{
		itemName = GameManager.Instance.colorsList [item.name];
		SoundManager.Instance.PlayButtonClickSound();

		if(itemName.isLocked)
		{
			AdvertisementManager.Instance.ShowAd(eAdsLoop.INCENTIVIZED);
		}
		else
		{
			tick.transform.parent = item.transform;
			tick.transform.localPosition = new Vector3(9f,-5f,0f);
			tick.SetActive(true);
			GameManager.Instance.SetBrushColor(int.Parse(item.name));
		}
	}
	
	public void BrushSizeClicked(GameObject brush)
	{
		SoundManager.Instance.PlayButtonClickSound();
		GameManager.Instance.SetBrushSize(int.Parse(brush.name));
		tick2.transform.parent = brush.transform;
		tick2.transform.localPosition = new Vector3(3.1f,-6f,0f);
		tick2.SetActive(true);
	}
	
	public void EraserClicked()
	{
		SoundManager.Instance.PlayButtonClickSound();
		GameManager.Instance.SetBrushColor(51);
	}
	
	public void FloodFillBtnClicked()
	{
		SoundManager.Instance.PlayButtonClickSound();
		GameManager.Instance.SetFloodFillMode();
	}

	public void BrushBtnClicked()
	{
		SoundManager.Instance.PlayButtonClickSound();
		GameManager.Instance.SetDefaultMode();
	}

	public void HomeBtnClicked()
	{
		SoundManager.Instance.PlayButtonClickSound();
		GameNavigationController.Instance.PopMenuToState(GameNavigationController.GameState.ImageSelection);
	}	
		
	void RewardUser()
	{
		itemName.isLocked = false;

		InitializeColorsArray();
	}

	public void FacebookBtnClicked()
	{
		SoundManager.Instance.PlayButtonClickSound();
		NativeShare.Instance.ShareScreenshotWithText("Awesome! Best Coloring game ever https://play.google.com/store/apps/details?id=com.pumplum.birdscoloring");
	}
}
