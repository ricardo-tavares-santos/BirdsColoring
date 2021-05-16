using UnityEngine;
using System.Collections;

public class BaloonController : MonoBehaviour
{
	public Texture[] baloonUnFilledTextures;
	public Texture[] baloonFilledTextures;
	public Transform baloonParent;
	public Transform baloonChild;
	public Vector3 offset = new Vector3 (0, 0, 0);
	public Vector3 initialPosition = new Vector3 (0, 0, 0);
	public Transform pump;
	public Transform currentParent;
	public GameObject handPointer;
	public GameObject handPointer2;
	public GameObject holdOnTxt;
	bool isIntact = false;
	bool baloonFilled = false;
	bool isCollided = false;
	bool attachedToStick = false;
	GameObject stickParent;

	void Start ()
	{
		int index = Random.Range(0,baloonUnFilledTextures.Length);
		baloonChild.GetComponent<UITexture>().mainTexture = baloonUnFilledTextures[index];
		baloonChild.GetChild(0).GetComponent<UITexture>().mainTexture = baloonFilledTextures[index];
	}

	void Update ()
	{
		if (isIntact && baloonChild.GetChild (0).localScale.x < 1) 
		{
			baloonChild.GetChild (0).localScale += Vector3.one * 0.005f;
			holdOnTxt.SetActive(true);
		} 
		else if (!baloonFilled && baloonChild.GetChild (0).localScale.x > 1) 
		{
			holdOnTxt.SetActive(false);
			//isIntact = false;
			//handPointer2.SetActive(true);
			handPointer2.GetComponent<TweenAlpha>().PlayForward();
			SoundManager.Instance.StopOneShotSound();
			baloonFilled = true;
			AssignParentAgain ();
		}
	}

	void OnTriggerEnter (Collider col)
	{
		if (col.transform.tag.Equals ("Pump")) {
			if (baloonChild.GetChild (0).localScale.x < 1) {
				isIntact = true;
				baloonChild.GetComponent<UITexture> ().enabled = false;
				baloonChild.GetChild (0).GetComponent<UITexture> ().enabled = true;
				baloonChild.parent = baloonParent;
				baloonChild.transform.localPosition = Vector3.zero;
				SoundManager.Instance.PlayBloonInSound();
			}
		} else if (col.transform.tag.Equals ("BaloonStick")) 
		{
			if (isIntact && baloonFilled && !attachedToStick) 
			{
				holdOnTxt.SetActive(false);
				attachedToStick = true;
				this.transform.parent = col.transform;
				this.transform.localPosition = Vector3.zero;
				this.GetComponent<UIDragObject> ().enabled = false;
				this.GetComponent<Collider> ().enabled = false;
				baloonChild.GetComponent<UITexture> ().enabled = false;
				col.GetComponent<Collider>().enabled = false;
				stickParent = col.gameObject;
				handPointer2.SetActive(false);
			}
		}
	}

	void AssignParentAgain ()
	{
		baloonChild.GetComponent<UITexture> ().enabled = false;
		baloonChild.parent = this.transform;
		baloonChild.localPosition = Vector3.zero;
		baloonChild.localScale = Vector3.one;
		baloonChild.localEulerAngles = Vector3.zero;
		baloonChild.GetChild (0).GetComponent<UITexture> ().enabled = true;
		baloonChild.GetChild (0).localScale = Vector3.one;
		baloonChild.GetChild (0).localEulerAngles = Vector3.zero;
	}

	void OnPress (bool isPressed)
	{
		if (isPressed) 
		{
			handPointer.SetActive(false);
			baloonChild.gameObject.GetComponent<UITexture>().depth = 22;
			baloonChild.GetChild (0).GetComponent<UITexture>().depth = 22;
			if (!isIntact && !attachedToStick) {
				baloonFilled = false;
				baloonChild.gameObject.SetActive (true);
			} else{

			}
		} else 
		{
			baloonChild.gameObject.GetComponent<UITexture>().depth = 10;
			baloonChild.GetChild (0).GetComponent<UITexture>().depth = 10;

			if (isIntact && !attachedToStick) 
			{
				holdOnTxt.SetActive(false);
				handPointer2.GetComponent<TweenAlpha>().PlayReverse();
				this.GetComponent<UIDragObject> ().enabled = false;
				isIntact = false;
				baloonFilled = false;
				baloonChild.GetComponent<Animation> ().Play ("BaloonAnimation");
				SoundManager.Instance.StopOneShotSound();
				SoundManager.Instance.PlayBloonOutSound();
			} 
			else if (!attachedToStick && !isIntact) 
			{
				holdOnTxt.SetActive(false);
				handPointer2.GetComponent<TweenAlpha>().PlayReverse();
				this.GetComponent<UIDragObject> ().enabled = false;
				TweenPosition.Begin (this.gameObject, 1f, initialPosition + offset);
				Invoke ("RepositionElement", 1f);
			}
		}
	}

	public void RepositionElement ()
	{
			this.GetComponent<UIDragObject> ().enabled = true;
			this.transform.parent = currentParent;
			baloonChild.parent = this.transform;
			baloonChild.localPosition = Vector3.zero;
			baloonChild.localScale = Vector3.one;
			baloonChild.localEulerAngles = Vector3.zero;
			baloonChild.GetComponent<UITexture> ().enabled = true;
			baloonChild.GetChild (0).GetComponent<UITexture> ().enabled = false;
			baloonChild.GetChild (0).localScale = Vector3.one * 0.5f;
			baloonChild.GetChild (0).localEulerAngles = Vector3.forward * -90f;
			baloonChild.gameObject.SetActive (false);
			this.transform.localPosition = initialPosition;
	}
}
