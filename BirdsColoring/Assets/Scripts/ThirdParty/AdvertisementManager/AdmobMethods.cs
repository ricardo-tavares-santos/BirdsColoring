using UnityEngine;
using System;
using System.Collections;
using GoogleMobileAds;
using GoogleMobileAds.Api;

public class AdmobMethods : BasicAdNetwork
{

	#region Variables & Constants

	[System.Serializable]
	public class AdMobKeys : System.Object
	{
		public string bannerID;
		public string interstiaialID;
	}

	// name of Ad Network
	public const eAdsNetwork adNetwork = eAdsNetwork.ADMOB;

	// key for android
	public string androidID;

	// key for iOS
	public string iOSID;

	// key for Windows
	public string windowsID;

	// Private methods for banner and interstitial view;
	BannerView bannerView;
	InterstitialAd interstitialAd;

	AdRequest request = new AdRequest.Builder ().Build ();
	AdPosition bannerPosition = AdPosition.TopLeft;
	private bool bannerAdLoaded = false;

	#endregion

	#region Lifecycle methods

	// Use this for initialization
	void Start ()
	{
		bannerAdLoaded = false;
	}
	
	// Update is called once per frame
	void Update ()
	{
	
	}

	#endregion

	#region Overriden Callback Methods

	public override void InitNetwork ()
	{

#if UNITY_ANDROID
		string id = androidID;
#elif UNITY_IPHONE
        string id = iOSID;
#elif UNITY_WP8
		string id = windowsID;
#endif



		if (AdvertisementManager.Instance.bannerAdPosition == eBannerAdPosition.BOTTOM) {
			bannerPosition = AdPosition.TopLeft;
		}

		switch (configuration.adType) {
		case eAdsType.BANNER:

			bannerView = new BannerView (id, AdSize.Banner, bannerPosition);
				// Called when an ad request has successfully loaded.
			bannerView.AdLoaded += BannerAdLoaded;
				// Called when an ad request failed to load.
			bannerView.AdFailedToLoad += BannerAdFailedToLoad;
				// Called when an ad is clicked.
			bannerView.AdOpened += BannerAdOpened;
				// Called when the user is about to return to the app after an ad click.
			bannerView.AdClosing += BannerAdClosing;
				// Called when the user returned from the app after an ad click.
			bannerView.AdClosed += BannerAdClosed;
				// Called when the ad click caused the user to leave the application.
			bannerView.AdLeftApplication += BannerAdLeftApplication;
                
			//bannerView.LoadAd (request);
			GoogleAnalytics.LogEvent (Constants.GA_CATEGORY_ADMOB, Constants.GA_ACTION_BANNER, Constants.GA_LABEL_ADMOB_BANNER_Requested);
			break;
		case eAdsType.INTERSTITIAL:

			interstitialAd = new InterstitialAd (id);
				// Called when an ad request has successfully loaded.
			interstitialAd.AdLoaded += InterstitialAdLoaded;
				// Called when an ad request failed to load.
			interstitialAd.AdFailedToLoad += InterstitialAdFailedToLoad;
				// Called when an ad is clicked.
			interstitialAd.AdOpened += InterstitialAdOpened;
				// Called when the user is about to return to the app after an ad click.
			interstitialAd.AdClosing += InterstitialAdClosing;
				// Called when the user returned from the app after an ad click.
			interstitialAd.AdClosed += InterstitialAdClosed;
				// Called when the ad click caused the user to leave the application.
			interstitialAd.AdLeftApplication += InterstitialAdLeftApplication;

			interstitialAd.LoadAd (request);
			GoogleAnalytics.LogEvent (Constants.GA_CATEGORY_ADMOB, Constants.GA_ACTION_INTERSTITIAL, Constants.GA_LABEL_ADMOB_INT_Requested);
			break;
		case eAdsType.VIDEO:

			break;
		default:
			break;
		}        
	}

	public override void ShowAd ()
	{
		switch (configuration.adType) {
		case eAdsType.BANNER:
			if (!bannerAdLoaded) {
				bannerView.LoadAd (request);
			}
			bannerView.Show ();
			break;
		case eAdsType.INTERSTITIAL:
			interstitialAd.Show ();
			break;
		case eAdsType.VIDEO:

			break;
		default:
			break;
		}
	}

	public override void HideAd ()
	{
		switch (configuration.adType) {
		case eAdsType.BANNER:

			bannerView.Hide ();
			break;                
		case eAdsType.INTERSTITIAL:

			break;                
		case eAdsType.VIDEO:

			break;
		default:
			break;
		}
	}

	public override eAdsNetwork GetAdNetworkInfo ()
	{ 
		return eAdsNetwork.ADMOB;
	}

	#endregion

	#region Banner callback handlers

	public void BannerAdLeftApplication (object sender, EventArgs args)
	{

	}

	public void BannerAdClosed (object sender, EventArgs args)
	{

	}

	public void BannerAdClosing (object sender, EventArgs args)
	{


	}

	public void BannerAdOpened (object sender, EventArgs args)
	{
		// log analytics
		GoogleAnalytics.LogEvent (Constants.GA_CATEGORY_ADMOB, Constants.GA_ACTION_BANNER, Constants.GA_LABEL_ADMOB_BANNER_Clicked);
	}

	public void BannerAdLoaded (object sender, EventArgs args)
	{
		bannerAdLoaded = true;
		// Handle the ad loaded event.
		GoogleAnalytics.LogEvent (Constants.GA_CATEGORY_ADMOB, Constants.GA_ACTION_BANNER, Constants.GA_LABEL_ADMOB_BANNER_Shown);
	}

	public void BannerAdFailedToLoad (object sender, AdFailedToLoadEventArgs args)
	{
		bannerAdLoaded = false;
		// Handle the ad failed to load event.
		AdvertisementManager.Instance.SetBannerAdFailed (configuration.showAdAt, this.GetAdNetworkInfo ());

		// log analytics
		GoogleAnalytics.LogEvent (Constants.GA_CATEGORY_ADMOB, Constants.GA_ACTION_BANNER, Constants.GA_LABEL_ADMOB_BANNER_Failed);
	}

	#endregion

	#region Interstitial callback handlers

	public void InterstitialAdLeftApplication (object sender, EventArgs args)
	{
		GoogleAnalytics.LogEvent (Constants.GA_CATEGORY_ADMOB, Constants.GA_ACTION_INTERSTITIAL, Constants.GA_LABEL_ADMOB_INT_Clicked);
	}

	public void InterstitialAdClosed (object sender, EventArgs args)
	{
		interstitialAd.LoadAd (new AdRequest.Builder ().Build ());
		GoogleAnalytics.LogEvent (Constants.GA_CATEGORY_ADMOB, Constants.GA_ACTION_INTERSTITIAL, Constants.GA_LABEL_ADMOB_INT_Requested);
	}

	public void InterstitialAdClosing (object sender, EventArgs args)
	{
		// Handle the ad loaded event.
	}

	public void InterstitialAdOpened (object sender, EventArgs args)
	{

	}

	public void InterstitialAdLoaded (object sender, EventArgs args)
	{
		// Handle the ad loaded event.
		AdvertisementManager.Instance.SetInterstitialAdReady (configuration.showAdAt, this.GetAdNetworkInfo ());
		GoogleAnalytics.LogEvent (Constants.GA_CATEGORY_ADMOB, Constants.GA_ACTION_INTERSTITIAL, Constants.GA_LABEL_ADMOB_INT_Shown);
	}

	public void InterstitialAdFailedToLoad (object sender, AdFailedToLoadEventArgs args)
	{
		// log analytics
		GoogleAnalytics.LogEvent (Constants.GA_CATEGORY_ADMOB, Constants.GA_ACTION_INTERSTITIAL, Constants.GA_LABEL_ADMOB_INT_Failed);
	}

	#endregion
}
