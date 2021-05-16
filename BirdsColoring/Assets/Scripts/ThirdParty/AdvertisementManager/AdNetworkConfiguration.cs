using UnityEngine;
using System.Collections;

[System.Serializable]
public class AdNetworkConfiguration : System.Object {
	
    public eAdsLoop showAdAt;
	public eAdsCategory adCategory;
    public eAdsType adType;
	public int adPriority;
	private bool adReady = false;
	private bool adFailedToLoad = false;

	public bool AdReady {
		get { 
			return adReady;
		} 
		set {
			adReady = value;
		}
	}

	public bool AdFailedToLoad {
		get { 
			return adFailedToLoad;
		} 
		set {
			adFailedToLoad = value;
		}
	}
}
