using UnityEngine;
using System.Collections;
using System.Collections.Generic;

public class GameNavigationController : MonoBehaviour
{
	private static GameNavigationController instance;

	public static GameNavigationController Instance {
		get {
			if(instance == null){
				instance = FindObjectOfType<GameNavigationController>();
			}
			return instance;
		}
		set {
			instance = value;
		}
	}

	#region Variables
	
	public enum Mode
	{
		Prefabs,
		Objects}
	;
	
	public BasePanel[] panels;
	public Mode mode = Mode.Prefabs;
	
	// TODO: change the hashtable to dictionary
	private Hashtable panelsTable = null;
	private Stack navigationStack = null;
	private Hashtable createdPanels = null;
	public bool isBackBtnPressed = false;
	
	#endregion Variables
	
	#region Game States
	
	// An enum with all the possible states of the game. NOTE: depending on the game the game states may change. Please add at the end of the screen.
	public enum GameState
	{
		MainMenu = 0,
		SelectionScene,
		CategoryScene,
		ImageSelection,
		ColoringScene,
		Store,
		PrivacyPolicy,
		DressUp,
		Baloons,
		RateUs
	};
	
	//stores the initial state to show at start of game
	public GameState initialState = GameState.MainMenu;
	
	// the delegate for state change
	public delegate void OnGameStateChange (GameState g);
	public event OnGameStateChange GameStateChanged;


	public void ChangeGameStateTo (GameState g)
	{
		GameStateChanged (g);
	}
	
	#endregion Game States
	
	#region Mono Life Cycle
	
	void Awake ()
	{
		this.GameStateChanged += HandleGameStateChanged;

		if (GameObject.FindGameObjectsWithTag ("GameNavigationController").Length > 1) {
			Destroy (gameObject);
		} else {
			DontDestroyOnLoad (gameObject);
		}
	}

	void Start ()
	{
		// 1. Initialize and populate the panels hash table
		PopulatePanelsHashTable ();
		
		// 2. Initialize the navigation stack
		InitializeNavigationStack ();

		// 3. Hide all open menus (this is needed in case the developer left some menu open)
		HideAllPanels ();
		
		// 4. Display the initial menu or display the menu at the top of the stack (returning from some other unity scene)
		if (navigationStack.Count == 0) {
			PushPanel (this.initialState);
		} else {
			ShowPanel (GetMenuForState (NavigationStackPeek ()));
		}
	}

	/* 
	 * <summary>
	 * 
	 * </summary>
	 */
	void Update ()
	{
		#if UNITY_ANDROID

		if (Input.GetKeyDown(KeyCode.Escape)) 
		{ 
			if(navigationStack.Count != 1)
			{
				PopMenu();
				SoundManager.Instance.StopInGameLoop();
				SoundManager.Instance.StopOneShotSound();
			}
			else
				Application.Quit();
		}
		
		#endif
	}

	#endregion Mono Life Cycle

	#region Initialization 
	
	/// <summary>
	/// Populates the panels hash table.
	/// </summary>
	private void PopulatePanelsHashTable ()
	{
		panelsTable = new Hashtable ();
		
		createdPanels = new Hashtable ();
		
		for (int i = 0; i < panels.Length; i++) {
			
			BasePanel basePanel = panels [i];
			Debug.Log ("Creating Menu " + basePanel.state);
			panelsTable.Add (basePanel.state, basePanel);
			basePanel.gameObject.SetActive (true);
			basePanel.gameObject.SetActive (false);
		}
		
	}
	
	/// <summary>
	/// Initializes the navigation stack.
	/// </summary>
	private void InitializeNavigationStack ()
	{
		navigationStack = new Stack ();
	}
	
	
	#endregion Initialization	



	#region GameStateChange Delegate
	
	/// <summary>
	/// Handles the game state changed.
	/// </summary>
	/// <param name="g">The green component.</param>
	void HandleGameStateChanged (GameState g)
	{
		// If we want to do anything on game change
	}
	
	#endregion GameStateChange Delegate

	#region Menu Navigation Control Logic
	
	/* 
	 * <summary>
	 * 
	 * </summary>
	 */
	public void PushPanel (GameState g)
	{	
		
		// 1. If the incoming menu is a pop-up dont hide the last menu
		if (GetMenuForState (g).isPopup == false) {
			// 1.1. Hide the menu at the top of the stack
			if (navigationStack.Count != 0) {
				HideMenuAtState (NavigationStackPeek ());
			}
		}
		
		// 2. Push the next menu
		navigationStack.Push (g);
		
		// 3. Inform the game manager about the game state change
		InformStateHandler (g);
		
		// 4. Show the new menu at the top of the stack
		ShowMenuAtState (g);
		//Debug.Log (g);
	}
	
	/* 
	 * <summary>
	 * 
	 * </summary>
	 */
	public void PopMenu ()
	{
		// 1. Hide the menu at the top of the stack
		if (navigationStack.Count != 0) {
			HideMenuAtState (NavigationStackPeek ());
		}
		
		// 2. Pop the menu from the top of the stack
		navigationStack.Pop ();
		
		// 3. Get the menu at the top of the stack
		GameState g = NavigationStackPeek ();
		
		// 4. Inform the game manager about the game state
		InformStateHandler (g);
		
		// 5. Show the menu at the top of the stack 
		ShowMenuAtState (g);
	}
	
	/* 
	 * <summary>
	 * 
	 * </summary>
	 */
	public void PopMenuToState (GameState g)
	{
		// 1. Hide the menu at the top of the stack
		if (navigationStack.Count != 0) {
			HideMenuAtState (NavigationStackPeek ());
		}
		
		// 2. Keep popping till the desired menu is reached
		while (NavigationStackPeek() != g) {
			navigationStack.Pop ();
			            
			HideMenuAtState (NavigationStackPeek ());
		}
		
		// 3. Inform the game manager about the game state
		InformStateHandler (g);
		
		// 4. Show the menu at the top of the stack 
		ShowMenuAtState (g);
	}
	
	/* 
	 * <summary>
	 * 
	 * </summary>
	 */
	private void InformStateHandler (GameState g)
	{
		ChangeGameStateTo (g);
	}
	
	/* 
	 * <summary>
	 * 
	 * </summary>
	 */
	private void ShowMenuAtState (GameState g)
	{
		switch (g) {
		default:
			ShowPanel (GetMenuForState (g));	
			break;
			
		}
	}
	
	/* 
	 * <summary>
	 * 
	 * </summary>
	 */
	private void HideMenuAtState (GameState g)
	{
		switch (g) {
		default:
			HideMenu (GetMenuForState (g));		
			break;
			
		}
	}
	
	/* 
	 * <summary>
	 * This method disables all the menues assossiated with menu manager.
	 * </summary>
	 */
	private void HideAllPanels ()
	{
		foreach (DictionaryEntry de in panelsTable) {
			BasePanel basePanel = de.Value as BasePanel;
			HideMenu (basePanel);
		}
	}
	
	
	/* 
	 * <summary>
	 * 
	 * </summary>
	 */
	private void ShowPanel (BasePanel bm)
	{
		if (mode == Mode.Prefabs) {
			BasePanel tempBaseMenu = createdPanels [bm.state] as BasePanel;
			if (tempBaseMenu != null) {
				tempBaseMenu.MenuWillAppear ();
				tempBaseMenu.gameObject.SetActive (true);
				tempBaseMenu.MenuDidAppear ();
				return; 
			}
			
			
			BasePanel newBM = BasePanel.Instantiate (bm) as BasePanel;
			
			GameObject cam = GameObject.FindGameObjectWithTag ("Camera");
			
			newBM.transform.parent = cam.transform;
			
			newBM.transform.localScale = Vector3.one;
			
			newBM.MenuWillAppear ();
			newBM.gameObject.SetActive (true);
			newBM.MenuDidAppear ();
			
			
			createdPanels.Add (bm.state, newBM);
		} else if (mode == Mode.Objects) {
			bm.MenuWillAppear ();
			bm.gameObject.SetActive (true);
			bm.MenuDidAppear ();
		}
		
	}
	
	/* 
	 * <summary>
	 * 
	 * </summary>
	 */
	private void HideMenu (BasePanel bm)
	{
		if (mode == Mode.Prefabs) {
			
			BasePanel previousBasePanel = createdPanels [bm.state] as BasePanel;
			
			if (previousBasePanel != null) {
				previousBasePanel.MenuWillDisappear ();
				previousBasePanel.gameObject.SetActive (false);
				previousBasePanel.MenuDidDisappear ();
				
				createdPanels.Remove (bm.state);
				
				Destroy (previousBasePanel.gameObject);
			}
		} else if (mode == Mode.Objects) {
			bm.MenuWillDisappear ();
			bm.gameObject.SetActive (false);
			bm.MenuDidDisappear ();
		}
		
		//Destroy(bm);
		
		
	}
	
	#endregion Menu Navigation Control Logic
	

	
	#region Utility Methods
	
	/* 
	 * <summary>
	 * 
	 * </summary>
	 */
	private BasePanel GetMenuForState (GameState g)
	{
		return panelsTable [g] as BasePanel;
	}
	
	/* 
	 * <summary>
	 * 
	 * </summary>
	 */
	public GameState NavigationStackPeek ()
	{
		return (GameState)navigationStack.Peek ();
	}
	
	public void PurgeNavigationStack ()
	{
		navigationStack.Clear ();
	}
	#endregion Utility Methods
}
