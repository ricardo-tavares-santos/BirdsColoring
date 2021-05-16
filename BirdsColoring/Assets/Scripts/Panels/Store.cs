using UnityEngine;
using System.Collections;

public class Store : BasePanel {



	public void UnlockEverythingBtnClicked()
	{
		SoundManager.Instance.PlayButtonClickSound ();
		InAppPurchases.Instance.purchaseEverything ();
		GameNavigationController.Instance.PopMenu();
	}

	public void RestoreBtnClicked()
	{
		SoundManager.Instance.PlayButtonClickSound ();
		InAppPurchases.Instance.restorePurchases();
	}

	public void CloseButtonClicked()
	{
		SoundManager.Instance.PlayButtonClickSound();
		GameNavigationController.Instance.PopMenu();
	}
}
