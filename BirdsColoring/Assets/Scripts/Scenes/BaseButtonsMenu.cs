using UnityEngine;
using System.Collections;

public class BaseButtonsMenu : MonoBehaviour {


	private bool isMenuExpanded;
	private bool isEnable;

	public bool isNextButtonVisible;
	public bool isBackButtonVisible;

	public GameObject MenuButton;
	public GameObject HomeButton;
	public GameObject ReplayButton;
	public GameObject SoundButton;
	public GameObject backButton;
	public GameObject nextButton;


	// Use this for initialization
	void Start () {
		isMenuExpanded = false;
		isEnable = true;
		if (!isBackButtonVisible) {
			backButton.SetActive(false);		
		}
		if (!isNextButtonVisible) {
			nextButton.SetActive(false);	
		}
		//setting initial position
//		MenuButton.transform.position = new Vector3 ((-(Screen.width) + (MenuButton.GetComponent<UISprite>().width * 0.6f)),350.0f,0f);
//		HomeButton.transform.position = MenuButton.transform.position;
//		ReplayButton.transform.position = MenuButton.transform.position;
//		SoundButton.transform.position = MenuButton.transform.position;

		//checking the sound state and applying sprite accordingly
		if (GameManager.Instance.isSoundPaused) {
			SoundButton.GetComponent<UIButton>().normalSprite = SoundButton.GetComponent<UIButton>().hoverSprite = SoundButton.GetComponent<UIButton>().pressedSprite =
				SoundButton.GetComponent<UIButton>().disabledSprite = SoundButton.GetComponent<UISprite>().spriteName = "button_mute";		
		}
	}
	
	// Update is called once per frame
	void Update () {
	
	}
	void OnDisable()
	{
		if (isMenuExpanded) {
			OnMenuButtonClicked();	
		}
		if (!isBackButtonVisible) {
			backButton.SetActive(false);		
		}
		if (!isNextButtonVisible) {
			nextButton.SetActive(false);	
		}
	}

	private void runScaleTween(GameObject parent)
	{
		parent.GetComponent<TweenScale> ().ResetToBeginning();
		parent.GetComponent<TweenScale> ().PlayForward();
	}

	public void OnMenuButtonClicked()
	{
		if (!isEnable)
			return;
		toogleEnable ();
		runScaleTween (MenuButton);
		if (!isMenuExpanded) {
			isMenuExpanded = true;
			HomeButton.GetComponent<TweenPosition> ().PlayForward ();		
			ReplayButton.GetComponent<TweenPosition> ().PlayForward ();		
			SoundButton.GetComponent<TweenPosition> ().PlayForward ();		
		} 
		else {
			isMenuExpanded = false;
			HomeButton.GetComponent<TweenPosition> ().PlayReverse ();				
			ReplayButton.GetComponent<TweenPosition> ().PlayReverse ();				
			SoundButton.GetComponent<TweenPosition> ().PlayReverse ();				
		}
		Debug.Log ("Menu Button Clicked");
	}
	public void OnHomeButtonClicked()
	{

		runScaleTween (HomeButton);
		Debug.Log ("Home Button Clicked");
	}
	public void OnReplayButtonClicked()
	{
		runScaleTween (ReplayButton);
		Debug.Log ("Replay Button Clicked");
	}
	public void OnSoundButtonClicked()
	{
		if (GameManager.Instance.isSoundPaused) {
				GameManager.Instance.isSoundPaused = false;
				SoundButton.GetComponent<UIButton> ().normalSprite = SoundButton.GetComponent<UIButton> ().hoverSprite = SoundButton.GetComponent<UIButton> ().pressedSprite =
				SoundButton.GetComponent<UIButton> ().disabledSprite = SoundButton.GetComponent<UISprite> ().spriteName = "button_sound";		
		} 
		else {
			GameManager.Instance.isSoundPaused = true;
			SoundButton.GetComponent<UIButton> ().normalSprite = SoundButton.GetComponent<UIButton> ().hoverSprite = SoundButton.GetComponent<UIButton> ().pressedSprite =
			SoundButton.GetComponent<UIButton> ().disabledSprite = SoundButton.GetComponent<UISprite> ().spriteName = "button_mute";		
		}
		runScaleTween (SoundButton);
		Debug.Log ("Replay Button Clicked");
	}
	public void toogleEnable()
	{
		if (isEnable) {
			isEnable = false;		
		} else {
			isEnable = true;		
		}
	}
	public void showNextButton()
	{
		nextButton.SetActive (true);
	}
}
