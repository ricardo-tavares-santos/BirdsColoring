using UnityEngine;
using System.Collections;
using System.Collections.Generic;

public class GoogleAnalytics : MonoBehaviour
{
	#region Variables
	public GoogleAnalyticsV4 GAComponent;
	public static GoogleAnalyticsV4 googleAnalytics;

	// persistant singleton
	private static GoogleAnalytics _instance;

	#endregion

	#region Lifecycle methods

	private static GoogleAnalytics instance {
		get {
			if (_instance == null) {
				_instance = GameObject.FindObjectOfType<GoogleAnalytics> ();
				DontDestroyOnLoad (_instance.gameObject);
			}
			
			return _instance;
		}
	}

	void Awake ()
	{
		if (_instance == null) {
			//If I am the first instance, make me the Singleton
			_instance = this;
			DontDestroyOnLoad (this.gameObject);
		} else {
			//If a Singleton already exists and you find
			//another reference in scene, destroy it!
			if (this != _instance)
				Destroy (gameObject);
		}
		googleAnalytics = GAComponent;

	}

	void Start ()
	{
		googleAnalytics.StartSession();
	}

	#endregion

	#region Callback Methods
    
	public static void LogScreenView (string screenName)
	{
		googleAnalytics.LogScreen (screenName);
	}
	
	public static void LogEvent (string category, string action, string label, int value = 0)
	{
		googleAnalytics.LogEvent (category, action, label, value);
	}

	void OnApplicationQuit() {
		googleAnalytics.StopSession();
	}

	#endregion
}