using UnityEngine;
using System.Collections;

public enum eAdsLoop
{
	DEFAULT = 0,
	MAINMENU = 1,
	GAMEPLAY = 2,
	GAMEOVER = 3,
	INCENTIVIZED = 4
}

public enum eAdsNetwork
{
	DEFAULT = 0,
	ADMOB = 1,
	CHARTBOOST = 2,
	UNITY = 3,
	VUNGLE = 4
}

public enum eAdsCategory
{
	MONETIZATION = 0,
	CROSSPROMO = 1,
	VIDEO = 2
}

public enum eAdsType
{
	BANNER = 0,
	INTERSTITIAL = 1,
	VIDEO = 2,
	REWARDEDVIDEO = 3
}

public enum eBannerAdPosition
{
	TOP = 0,
	BOTTOM = 1
}

public class AdvertisementManager : MonoBehaviour
{
	private static AdvertisementManager instance;

	public static AdvertisementManager Instance {
		get {
			if (instance == null) {
				instance = FindObjectOfType<AdvertisementManager> ();
			}
			return instance;
		}
		set {
			instance = value;
		}
	}

	#region Variables & Constants

	public bool firstSessionPlayed {
		get {
			return (PlayerPrefs.GetInt ("FirstSessionPlayed", 0) == 0) ? false : true;
		}
		set {
			PlayerPrefs.SetInt ("FirstSessionPlayed", (value == true) ? 1 : 0);
		}
	}

	public eBannerAdPosition bannerAdPosition;
	public BasicAdNetwork[] androidBannerAds;
	public BasicAdNetwork[] androidInterstitialAndVideoAds;
	public BasicAdNetwork[] iOSBannerAds;
	public BasicAdNetwork[] iOSInterstitialAndVideoAds;
	public BasicAdNetwork[] windowsBannerAds;
	public BasicAdNetwork[] windowsInterstitialAndVideoAds;
	[HideInInspector]
	public BasicAdNetwork[]
		bannerAds;
	[HideInInspector]
	public BasicAdNetwork[]
		interstitialAndVideoAds;

	[HideInInspector]
	public eAdsLoop adsLoop { get; private set; }

	[HideInInspector]
	public eAdsNetwork activeBannerAd { get; private set; }

	private bool isAdsFreeEnabled = false;

	#endregion


	#region Lifecycle methods

	void Awake ()
	{
		instance = this;
	}

	// Use this for initialization
	void Start ()
	{
		#if UNITY_ANDROID
		bannerAds = androidBannerAds;
		interstitialAndVideoAds = androidInterstitialAndVideoAds;
		#elif UNITY_IOS
		bannerAds = iOSBannerAds;
		interstitialAndVideoAds = iOSInterstitialAndVideoAds;
		#elif UNITY_WP8
		bannerAds = windowsBannerAds;
		interstitialAndVideoAds = windowsInterstitialAndVideoAds;
		#endif

		this.SortAdPriorities ();

		CheckIsAdsFreeEnabled ();

		InitAdNetworks ();
	}
	
	// Update is called once per frame
	void Update ()
	{

	}

	#endregion

	#region Utility Methods

	public string GetAdNetworkName (eAdsNetwork adNetwork)
	{
		string adNetworkName = "Default";
		switch (adNetwork) {
		case eAdsNetwork.ADMOB:
			adNetworkName = "AdMob";
			break;
		case eAdsNetwork.UNITY:
			adNetworkName = "Unity";
			break;
		case eAdsNetwork.CHARTBOOST:
			adNetworkName = "Chartboost";
			break;
		case eAdsNetwork.VUNGLE:
			adNetworkName = "Vungle";
			break;
		}

		return adNetworkName;
	}

	public string GetAdTypeName (eAdsType adType)
	{
		string adTypeName = "Default";
		switch (adType) {
		case eAdsType.BANNER:
			adTypeName = "Banner";
			break;
		case eAdsType.INTERSTITIAL:
			adTypeName = "Interstitial";
			break;
		case eAdsType.VIDEO:
			adTypeName = "Video";
			break;
		case eAdsType.REWARDEDVIDEO:
			adTypeName = "RewardedVideo";
			break;
		}
		
		return adTypeName;
	}

	public string GetAdsLoopName (eAdsLoop adsLoop)
	{
		string adsLoopName = "Default";
		switch (adsLoop) {
		case eAdsLoop.MAINMENU:
			adsLoopName = "Main Menu";
			break;
		case eAdsLoop.GAMEPLAY:
			adsLoopName = "Game Play";
			break;
		case eAdsLoop.GAMEOVER:
			adsLoopName = "Game Over";
			break;
		case eAdsLoop.INCENTIVIZED:
			adsLoopName = "Incentive";
			break;
		}
		
		return adsLoopName;
	}

	// sorting list on base of ad priorities
	private void SortAdPriorities ()
	{
		BasicAdNetwork temp;
		
		// sorting banner priorities
		if (bannerAds.Length > 1) {
			for (int i = 0; i < bannerAds.Length; i++) {
				for (int j = 0; j < bannerAds.Length - 1; j++) {
					if (bannerAds [j].configuration.adPriority > bannerAds [j + 1].configuration.adPriority) {
						temp = bannerAds [j + 1];
						bannerAds [j + 1] = bannerAds [j];
						bannerAds [j] = temp;
					}
				}
			}
		}
		
		// sorting interstitial priorities
		if (interstitialAndVideoAds.Length > 1) {
			for (int i = 0; i < interstitialAndVideoAds.Length; i++) {
				for (int j = 0; j < interstitialAndVideoAds.Length - 1; j++) {
					if (interstitialAndVideoAds [j].configuration.adPriority > interstitialAndVideoAds [j + 1].configuration.adPriority) {
						temp = interstitialAndVideoAds [j + 1];
						interstitialAndVideoAds [j + 1] = interstitialAndVideoAds [j];
						interstitialAndVideoAds [j] = temp;
					}
				}
			}
		}
	}

	public void CheckIsAdsFreeEnabled ()
	{
		if (PlayerPrefs.GetInt ("AdvertisementManager_IsAdsFree", 0) == 1) {
			IsAdsFree = true;
		} else {
			IsAdsFree = false;
		}
	}

	public bool IsAdsFree {
		get { 
			return isAdsFreeEnabled;
		} 
		set {
			isAdsFreeEnabled = value;
		}
	}

	#endregion

	#region Callback Methods

	public void ShowAd (eAdsLoop adsLoop)
	{
		this.adsLoop = adsLoop;
		//show latest ad
		ShowInterstitialAd ();
	}

	public void SetAdsFreeModeEnabled ()
	{
		if (!IsAdsFree) {
			IsAdsFree = true;
			HideBannerAd ();

			PlayerPrefs.SetInt ("AdvertisementManager_IsAdsFree", 1);
			PlayerPrefs.Save ();
		}
	}

	public void SetInterstitialAdReady (eAdsLoop adsLoop, eAdsNetwork adNetwork)
	{
		for (int i = 0; i < interstitialAndVideoAds.Length; i++) {
			if ((interstitialAndVideoAds [i].configuration.showAdAt == adsLoop) && (interstitialAndVideoAds [i].GetAdNetworkInfo () == adNetwork)) {
				interstitialAndVideoAds [i].configuration.AdReady = true;
				break;
			} 
		}
	}

	public void SetBannerAdFailed (eAdsLoop adsLoop, eAdsNetwork adNetwork)
	{
		for (int i = 0; i < bannerAds.Length; i++) {
			if ((bannerAds [i].configuration.showAdAt == adsLoop) && (bannerAds [i].GetAdNetworkInfo () == adNetwork)) {
				bannerAds [i].configuration.AdFailedToLoad = true;

				if ((i + 1) < bannerAds.Length) {
					i += 1;
					bannerAds [i].InitNetwork ();
					activeBannerAd = bannerAds [i].GetAdNetworkInfo ();
				}
				break;
			}
		}
	}

	private void InitAdNetworks ()
	{
		// ads free check
		if (IsAdsFree)
			return;

		//****************Initialize Banner Ads************************//
		int i = 0;
		if (i < bannerAds.Length) {
			bannerAds [i].InitNetwork ();
			activeBannerAd = bannerAds [i].GetAdNetworkInfo ();
		}
		//***********************Initialize Interstitial and Video Ads******************//
		for (i = 0; i < interstitialAndVideoAds.Length; i++) {
			interstitialAndVideoAds [i].InitNetwork ();
		}
		//*********************************************************************//
	}

	private void ShowInterstitialAd ()
	{
		bool videoNotAvailable = false;
		// ads free check
		if (IsAdsFree)
			return;

		for (int i = 0; i < interstitialAndVideoAds.Length; i++) {
			if (interstitialAndVideoAds [i].configuration.showAdAt == adsLoop) {
				if (interstitialAndVideoAds [i].configuration.adCategory != eAdsCategory.VIDEO) {
					if (interstitialAndVideoAds [i].configuration.AdReady) {
						interstitialAndVideoAds [i].ShowAd ();
						interstitialAndVideoAds [i].configuration.AdReady = false;
						break;
					} else {
						continue;
					}
				}
				if (interstitialAndVideoAds [i].configuration.adCategory == eAdsCategory.VIDEO) {
					if (interstitialAndVideoAds [i].isAdReady ()) {
						interstitialAndVideoAds [i].ShowAd ();
						videoNotAvailable = false;
						break;
					} else {
						videoNotAvailable = true;
						continue;
					}
				} 
			}          
		}

		if (videoNotAvailable) {
			NGUITools.Broadcast ("ToggleSurprizeBoxClicked");
//			GameNavigationController.Instance.PushPanel (GameNavigationController.GameState.TryLaterVideoPopup);
		}
	}

	public void ShowBannerAd ()
	{
		// ads free check
		if (IsAdsFree)
			return;

		// hide previous active banner ad
		HideBannerAd ();

		for (int i = 0; i < bannerAds.Length; i++) {
			if (bannerAds [i].GetAdNetworkInfo () == activeBannerAd) {
				bannerAds [i].ShowAd ();
				break;
			}
		}
	}

	public void HideBannerAd ()
	{
		for (int i = 0; i < bannerAds.Length; i++) {
			if (bannerAds [i].GetAdNetworkInfo () == activeBannerAd) {
				bannerAds [i].HideAd ();
				break;
			}
		}
	}

	#endregion

	void OnDestroy ()
	{
		firstSessionPlayed = true;
	}

	void OnApplicationQuit ()
	{
		firstSessionPlayed = true;
	}
}
