using UnityEngine;
using System.Collections;

public class BasePanel : MonoBehaviour {

	public GameNavigationController.GameState state;
	public bool isPopup = false;

	#region Button Handlers
	
	public virtual void BackBtnPressed() 
	{
		GameNavigationController.Instance.PopMenu ();
	}

	#endregion Button Handlers


	#region BaseMenu: Life Cycle

	/* 
	 * <summary>
	 * This method is called right before your view appears, good for hiding/showing fields or any operations 
	 * that you want to happen every time before the view is visible. Because you might be going back and 
	 * forth between views, this will be called every time your view is about to appear on the screen.
	 * </summary>
	 */
	public void MenuWillAppear()
	{
	//	transform.GetComponent<TweenAlpha> ().PlayForward ();
	}
	
	/* 
	 * <summary>
	 * This method is called after the view appears - great place to start an animations or the loading of 
	 * external data from an API.
	 * </summary>
	 */
	public void MenuDidAppear()
	{

	}

	/* 
	 * <summary>
	 * This method is called right before the view disappears - its a good place to stop all animations, API calls
	 * etc
	 * </summary>
	 */
	public void MenuWillDisappear()
	{
	
	}

	/* 
	 * <summary>
	 * This method is called after the view disappears
	 * </summary>
	 */
	public void MenuDidDisappear()
	{

	}

	#endregion BaseMenu: Life Cycle

}
