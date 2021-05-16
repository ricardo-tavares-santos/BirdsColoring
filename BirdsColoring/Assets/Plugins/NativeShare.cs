﻿using UnityEngine;
using System.Collections;
using System.Runtime.InteropServices;

/*
 * https://github.com/ChrisMaire/unity-native-sharing 
 */

public class NativeShare : MonoBehaviour {

	private static NativeShare instance;
	
	public static NativeShare Instance {
		get {
			if(instance == null){
				instance = FindObjectOfType<NativeShare>();
			}
			return instance;
		}
		set {
			instance = value;
		}
	}
	
	void Awake(){
		instance = this;
	}
	
	public void ShareScreenshotWithText(string text)
	{
		string ScreenshotName = Constants.GAME_NAME + " "+ System.DateTime.Now.ToString("yyyy-MM-dd_HH-mm-ss") + ".png";
		string screenShotPath = Application.persistentDataPath + "/" + ScreenshotName;
		ScreenCapture.CaptureScreenshot(ScreenshotName);
		
		Share(text,screenShotPath,"");
	}
	
	public void Share(string shareText, string imagePath, string url, string subject = "")
	{
		#if UNITY_ANDROID
		AndroidJavaClass intentClass = new AndroidJavaClass("android.content.Intent");
		AndroidJavaObject intentObject = new AndroidJavaObject("android.content.Intent");
		
		intentObject.Call<AndroidJavaObject>("setAction", intentClass.GetStatic<string>("ACTION_SEND"));
		AndroidJavaClass uriClass = new AndroidJavaClass("android.net.Uri");
		AndroidJavaObject uriObject = uriClass.CallStatic<AndroidJavaObject>("parse", "file://" + imagePath);
		intentObject.Call<AndroidJavaObject>("putExtra", intentClass.GetStatic<string>("EXTRA_STREAM"), uriObject);
		intentObject.Call<AndroidJavaObject>("setType", "image/png");
		
		intentObject.Call<AndroidJavaObject>("putExtra", intentClass.GetStatic<string>("EXTRA_TEXT"), shareText);
		
		AndroidJavaClass unity = new AndroidJavaClass("com.unity3d.player.UnityPlayer");
		AndroidJavaObject currentActivity = unity.GetStatic<AndroidJavaObject>("currentActivity");
		
		AndroidJavaObject jChooser = intentClass.CallStatic<AndroidJavaObject>("createChooser", intentObject, subject);
		currentActivity.Call("startActivity", jChooser);
		#elif UNITY_IOS
//		CallSocialShareAdvanced(shareText, subject, url, screenShotPath);
		CallSocialShareAdvanced(shareText, subject, url, imagePath);
		#else
		Debug.Log("No sharing set up for this platform.");
		#endif
	}
	
	#if UNITY_IOS
	public struct ConfigStruct
	{
		public string title;
		public string message;
	}
	
	[DllImport ("__Internal")] private static extern void showAlertMessage(ref ConfigStruct conf);
	
	public struct SocialSharingStruct
	{
		public string text;
		public string url;
		public string image;
		public string subject;
	}
	
	[DllImport ("__Internal")] private static extern void showSocialSharing(ref SocialSharingStruct conf);
	
	public static void CallSocialShare(string title, string message)
	{
		ConfigStruct conf = new ConfigStruct();
		conf.title  = title;
		conf.message = message;
		showAlertMessage(ref conf);
	}
	
	public static void CallSocialShareAdvanced(string defaultTxt, string subject, string url, string img)
	{
		SocialSharingStruct conf = new SocialSharingStruct();
		conf.text = defaultTxt; 
		conf.url = url;
		conf.image = img;
		conf.subject = subject;
		
		showSocialSharing(ref conf);
	}
	#endif
}
