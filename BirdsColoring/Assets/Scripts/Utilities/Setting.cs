using UnityEngine;
using System.Collections;

public class Setting : MonoBehaviour {

	public GameObject[] buttons;
	private bool flag=false;
	public GameObject musicOn;
	public GameObject musicOff;
	public GameObject volumeOn;
	
	public void Start()
	{
		if (AudioListener.volume == 0.0f)
		{
			musicOn.GetComponent<UITexture>().mainTexture = musicOff.GetComponent<UITexture>().mainTexture;
		}
		else
		{
			musicOn.GetComponent<UITexture>().mainTexture = volumeOn.GetComponent<UITexture>().mainTexture;
		}
	}

	void OnClick()
	{
		if(flag)
		{
			foreach(GameObject obj in buttons)
			{
				obj.GetComponent<TweenPosition>().PlayReverse();
				obj.GetComponent<Collider>().enabled=false;
				flag=false;
			}
		}
		else
		{
			foreach(GameObject obj in buttons)
			{
				obj.GetComponent<TweenPosition>().PlayForward();
				obj.GetComponent<Collider>().enabled=true;
				flag=true;
			}
		}
		SoundManager.Instance.PlayButtonClickSound ();

	}

	public void HomeBtnCallBack()
	{
		SoundManager.Instance.PlayButtonClickSound ();
		GameNavigationController.Instance.PopMenuToState (GameNavigationController.GameState.MainMenu);
	}

	public void RedoBtnCallBack()
	{
		SoundManager.Instance.PlayButtonClickSound ();
		GameNavigationController.Instance.PopMenuToState (GameNavigationController.Instance.NavigationStackPeek());
	}
	public void SoundOnBtnCallBack()
	{
		if (AudioListener.volume == 0.0f)
		{
			AudioListener.volume = 1.0f;
			musicOn.GetComponent<UITexture>().mainTexture = volumeOn.GetComponent<UITexture>().mainTexture;
		}
		else
		{
			AudioListener.volume = 0.0f;
			musicOn.GetComponent<UITexture>().mainTexture = musicOff.GetComponent<UITexture>().mainTexture;
		}
		
	}

	public void StoreBtnClicked()
	{
		SoundManager.Instance.PlayButtonClickSound();
		GameNavigationController.Instance.PushPanel (GameNavigationController.GameState.Store);

	}
	
}
