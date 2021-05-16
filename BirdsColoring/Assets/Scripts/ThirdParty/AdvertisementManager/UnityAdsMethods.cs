using UnityEngine;
using UnityEngine.Advertisements;

public class UnityAdsMethods : BasicAdNetwork
{
	#region Variables & Constants
	
	// name of Ad Network
	public const eAdsNetwork adNetwork = eAdsNetwork.UNITY;
	
	// key for android    
	public string androidID;
	
	// key for iOS    
	public string iOSID;
	
	// key for WP8
	//public string windowsID;
	
	#endregion
	#region Lifecycle methods
	
	// Use this for initialization
	void Start ()
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
		#endif
		switch (configuration.adType) {
		case eAdsType.BANNER:
			
			break;                
		case eAdsType.INTERSTITIAL:
			
			break;                
		case eAdsType.VIDEO:
			Advertisement.Initialize (id);
			GoogleAnalytics.LogEvent (Constants.GA_CATEGORY_UNITY, Constants.GA_ACTION_REWARDED_VIDEO, Constants.GA_LABEL_UNITY_V_Requested);
			break;

		case eAdsType.REWARDEDVIDEO:
			Advertisement.Initialize (id);
			GoogleAnalytics.LogEvent (Constants.GA_CATEGORY_UNITY, Constants.GA_ACTION_REWARDED_VIDEO, Constants.GA_LABEL_UNITY_RV_Requested);
			break;
		default:
			break;
		}
	}
	
	public override void ShowAd ()
	{   
		switch (configuration.adType) {
		case eAdsType.BANNER:
			
			break;                
		case eAdsType.INTERSTITIAL:
			
			break;                
		case eAdsType.VIDEO:
			var vOptions = new ShowOptions { resultCallback = HandleShowResult };
			Advertisement.Show ("video", vOptions);
			break;

		case eAdsType.REWARDEDVIDEO:
			var rvOptions = new ShowOptions { resultCallback = HandleShowResult };
			Advertisement.Show ("rewardedVideoZone", rvOptions);
			break;
		default:
			break;
		}
	}

	public override bool isAdReady ()
	{
		if (Advertisement.isInitialized) {
			if (configuration.adType == eAdsType.REWARDEDVIDEO) {
				return Advertisement.IsReady ("rewardedVideoZone");
			} else {
				return Advertisement.IsReady ("video");
			}
		}
		return false;
	}

	public override eAdsNetwork GetAdNetworkInfo ()
	{ 
		return eAdsNetwork.UNITY;
	}

	private void HandleShowResult (ShowResult result)
	{
		switch (result) {
		case ShowResult.Finished:
			Debug.Log ("The ad was successfully shown.");
			//
			// YOUR CODE TO REWARD THE GAMER
			// Give coins etc.
			if (configuration.adType == eAdsType.REWARDEDVIDEO) {
				GoogleAnalytics.LogEvent (Constants.GA_CATEGORY_UNITY, Constants.GA_ACTION_REWARDED_VIDEO, Constants.GA_LABEL_UNITY_RV_Completed);
			} else {
				GoogleAnalytics.LogEvent (Constants.GA_CATEGORY_UNITY, Constants.GA_ACTION_VIDEO, Constants.GA_LABEL_UNITY_V_Completed);
			}

			NGUITools.Broadcast ("RewardUser");

			break;
		case ShowResult.Skipped:
			GoogleAnalytics.LogEvent (Constants.GA_CATEGORY_UNITY, Constants.GA_ACTION_VIDEO, Constants.GA_LABEL_UNITY_V_Skipped);
			Debug.Log ("The ad was skipped before reaching the end.");
			break;
		case ShowResult.Failed:
			if (configuration.adType == eAdsType.REWARDEDVIDEO) {
				GoogleAnalytics.LogEvent (Constants.GA_CATEGORY_UNITY, Constants.GA_ACTION_REWARDED_VIDEO, Constants.GA_LABEL_UNITY_RV_Failed);
			} else {
				GoogleAnalytics.LogEvent (Constants.GA_CATEGORY_UNITY, Constants.GA_ACTION_VIDEO, Constants.GA_LABEL_UNITY_V_Failed);
			}
			Debug.LogError ("The ad failed to be shown.");
			break;
		}
	}
	#endregion
}