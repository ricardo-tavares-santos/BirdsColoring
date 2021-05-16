using UnityEngine;
using System.Collections;

public class MyScrollViewVerticle : MonoBehaviour
{

	public float springStrength = 8.0f;
	private UIScrollView scrollView;
	public int elementsPerPage;
	public int currentScrolledElements;
	public int noOfChild;
	private Vector3 startingScrollPosition;
	private UIGrid grid;
	public UITexture previousArrow;
	public UITexture nextArrow;
	public Movement movement = Movement.Horizontal;
	public enum Movement
	{
		Horizontal,
		Vertical,
		Unrestricted,
		Custom,
	}
	
	private Vector3 firstItemPosition;
	private Vector3 lastItemPosition;
	private Vector3 previousArrowPosition;
	private Vector3 nextArrowPosition;
	private Camera SceneCamera;
	// Use this for initialization
	void Start ()
	{
		SceneCamera = FindObjectOfType<Camera> ();
		if (scrollView == null) {
			
			scrollView = NGUITools.FindInParents<UIScrollView> (gameObject);
			if (scrollView == null) {
				Debug.LogWarning (GetType () + " requires " + typeof(UIScrollView) + " object in order to work", this);
				enabled = false;
				return;
			}
			
			grid = this.GetComponent<UIGrid> ();
			//elementsPerPage = (int)(scrollView.panel.baseClipRegion.z / grid.cellWidth);
			currentScrolledElements = 0;
			startingScrollPosition = scrollView.panel.cachedTransform.localPosition;

			//previousArrowPosition = SceneCamera.transform.TransformPoint (previousArrow.gameObject.transform.position);
			//nextArrowPosition = SceneCamera.transform.TransformPoint (nextArrow.gameObject.transform.position);
		}	
	}
	
//		// Update is called once per frame
//		void Update ()
//		{
//		
//		}
//
//		void LateUpdate ()
//		{
//				if (!Application.isPlaying)
//						return;
//				resetArrows ();
//		
//		}
	
	/// <summary>
	/// Scrolls until target position matches target panelAnchorPosition (may be the center of the panel, one of its sides, etc)
	/// </summary>	
	void MoveBy (Vector3 target)
	{
		if (scrollView != null && scrollView.panel != null) {
			// Spring the panel to this calculated position
			SpringPanel.Begin (scrollView.panel.cachedGameObject, startingScrollPosition - target, springStrength);
		}
	}
	
	public void NextPage ()
	{
		if (scrollView != null && scrollView.panel != null) {
			currentScrolledElements += elementsPerPage;
			if (currentScrolledElements > (noOfChild - elementsPerPage)) {
				currentScrolledElements = (noOfChild - elementsPerPage);
			}
			float nextScroll = grid.cellHeight * currentScrolledElements;
			Vector3 target = new Vector3 (0.0f,-nextScroll, 0.0f);				
			MoveBy (target);
		}
		
	}
	
	public void PreviousPage ()
	{
		if (scrollView != null && scrollView.panel != null) {
			currentScrolledElements -= elementsPerPage;
			if (currentScrolledElements < 0) {
				currentScrolledElements = 0;
			}
			float nextScroll = grid.cellHeight * currentScrolledElements;
			Vector3 target = new Vector3 (0.0f,-nextScroll, 0.0f);				
			MoveBy (target);
		}
	}
	
	
	/// <summary>
	/// Get current position of first and last elements and compare with the arrow positions to set the active state of arrows)
	/// </summary>	
	public void resetArrows ()
	{
//				firstItemPosition = SceneCamera.transform.TransformPoint (grid.GetChildList () [0].gameObject.transform.position);
//				lastItemPosition = SceneCamera.transform.TransformPoint (grid.GetChildList () [grid.GetChildList ().Count - 1].gameObject.transform.position);
//				//	Debug.Log(string.Format("{0} is item position and {1} is arrow position", lastItemPosition.x, nextArrowPosition.x));
//				if (movement == Movement.Horizontal) {
//			
//						if (firstItemPosition.x < previousArrowPosition.x) {
//								previousArrow.gameObject.SetActive (true);
//						} else {
//								previousArrow.gameObject.SetActive (false);
//						}
//			
//						if (lastItemPosition.x > nextArrowPosition.x) {
//								nextArrow.gameObject.SetActive (true);
//						} else {
//								nextArrow.gameObject.SetActive (false);
//						}
//				} else if (movement == Movement.Vertical) {
//						if (firstItemPosition.y > previousArrowPosition.y) {
//								previousArrow.gameObject.SetActive (true);
//						} else {
//								previousArrow.gameObject.SetActive (false);
//						}
//			
//						if (lastItemPosition.y < nextArrowPosition.y) {
//								nextArrow.gameObject.SetActive (true);
//						} else {
//								nextArrow.gameObject.SetActive (false);
//						}
//				}
	}
}