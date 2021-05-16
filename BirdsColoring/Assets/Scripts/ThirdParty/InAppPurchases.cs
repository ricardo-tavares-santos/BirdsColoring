//-----------------------------------------------------------------
//  Copyright 2013 Alex McAusland and Ballater Creations
//  All rights reserved
//  www.outlinegames.com
//-----------------------------------------------------------------
using UnityEngine;
using System;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using Unibill;
using Unibill.Demo;
using Unibill.Impl;

public class InAppPurchases : Singleton<InAppPurchases>
{
	#region Variables, Constants & Initializers

	// Use these for ids
	public const string SKU_PURCHASE_EVERYTHING = "iap_all_birdscoloring";

	public bool EverythingUnlocked {
		get {
			return (PlayerPrefs.GetInt ("EverythingUnlocked", 0) == 0) ? false : true;
		}
		set {
			PlayerPrefs.SetInt ("EverythingUnlocked", (value == true) ? 1 : 0);
		}
	}

	#endregion

	
	void Start ()
	{	
		if (UnityEngine.Resources.Load ("unibillInventory.json") == null) {
			Debug.LogError ("You must define your purchasable inventory within the inventory editor!");
			this.gameObject.SetActive (false);
			return;
		}
		
		// set callbacks
		Unibiller.onBillerReady += OnBillerReady;
		Unibiller.onTransactionsRestored += OnTransactionsRestored;
		Unibiller.onPurchaseCompleteEvent += OnPurchaseComplete;
		//Unibiller.onPurchaseCancelled += OnPurchaseFailed;
		Unibiller.onPurchaseFailed += OnPurchaseFailed;
		Unibiller.onPurchaseRefunded += OnPurchaseRefunded;
		
		Unibiller.Initialise ();
		// for testing
		//Unibiller.clearTransactions ();
	}
	
	//	void OnEnable ()
	//	{
	//		// set callbacks
	//		Unibiller.onBillerReady += OnBillerReady;
	//		Unibiller.onTransactionsRestored += OnTransactionsRestored;
	//		Unibiller.onPurchaseComplete += OnPurchaseComplete;
	//		Unibiller.onPurchaseCancelled += OnPurchaseFailed;
	//		Unibiller.onPurchaseFailed += OnPurchaseFailed;
	//		Unibiller.onPurchaseRefunded += OnPurchaseRefunded;
	//	}
	
	void OnDisable ()
	{
		
	}
	
	/***************** UniBill Callbacks START *******************/
	
	private void OnBillerReady (UnibillState state)
	{
		switch (state) {
		case UnibillState.SUCCESS:
			Debug.Log ("Unibill Init SUCCESS");
			Unibiller.restoreTransactions ();
			break;
		case UnibillState.SUCCESS_WITH_ERRORS:
			Debug.Log ("Unibill Init SUCCESS with ERRORS");
			Unibiller.restoreTransactions ();
			break;
		case UnibillState.CRITICAL_ERROR:
			Debug.Log ("Unibill Init ERROR");
			break;
		default:
			break;
		}
	}

	private void OnPurchaseCancelled (PurchasableItem purchase)
	{
		Debug.Log ("Unibill OnPurchaseCancelled : " + purchase.Id);
	}

	private void OnPurchaseFailed (PurchaseFailedEvent purchase)
	{
		Debug.Log ("Unibill OnPurchaseFailed : " + purchase.PurchasedItem.Id);
	}

	private void OnPurchaseRefunded (PurchasableItem purchase)
	{
		Debug.Log ("Unibill OnPurchaseRefunded : " + purchase.Id);
	}

	private void OnTransactionsRestored (bool isRestored)
	{
		switch (isRestored) {
		case true:
			Debug.Log ("Unibill OnTransactionsRestored SUCCESS");
			break;
		case false:
			Debug.Log ("Unibill OnTransactionsRestored FAILED");
			break;
		default:
			break;
		}
	}

	private void OnPurchaseComplete (PurchaseEvent purchase)
	{
		Debug.Log ("Unibill OnPurchaseComplete : " + purchase.PurchasedItem.Id);

		switch (purchase.PurchasedItem.Id) {

		case SKU_PURCHASE_EVERYTHING:
			GoogleAnalytics.LogEvent (Constants.GA_CATEGORY_IAP, Constants.GA_ACTION_TYPE_IAP_SUCCESS, SKU_PURCHASE_EVERYTHING);

			foreach (var key in GameManager.Instance.imagesList.Keys) 
			{
				GameManager.Instance.imagesList [key].isLocked = false;	
			}

			foreach (var key in GameManager.Instance.colorsList.Keys) 
			{
				GameManager.Instance.colorsList [key].isLocked = false;	
			}

			break;
										
		default:
			break;
		}
		NGUITools.Broadcast ("InitializeImagesArray");
		NGUITools.Broadcast ("InitializeColorsArray");
		AdvertisementManager.Instance.SetAdsFreeModeEnabled ();
		AdvertisementManager.Instance.HideBannerAd ();
	
		EverythingUnlocked = true;
		PlayerPrefs.Save ();
	}
		
	#region PurchaseMethods

	public void purchaseEverything()
	{
		Unibiller.initiatePurchase (Unibiller.GetPurchasableItemById (SKU_PURCHASE_EVERYTHING));
	}

	public void restorePurchases()
	{
		Unibiller.restoreTransactions ();
	}

	#endregion PurchaseMethods


}
