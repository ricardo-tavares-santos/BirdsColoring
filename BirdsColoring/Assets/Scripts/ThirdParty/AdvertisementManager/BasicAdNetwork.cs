using UnityEngine;
using System.Collections;

public class BasicAdNetwork : MonoBehaviour
{
	// The public properties of the network
	public AdNetworkConfiguration configuration;

	public virtual void InitNetwork ()
	{
		//Override if necessary
	}

	public virtual void ShowAd ()
	{
		//Override if necessary
	}

	public virtual void HideAd ()
	{
		//Override if necessary
	}

	public virtual bool isAdReady(){
		return false;
	}


	public virtual eAdsNetwork GetAdNetworkInfo ()
	{ 
		return eAdsNetwork.DEFAULT; 
	}
}
