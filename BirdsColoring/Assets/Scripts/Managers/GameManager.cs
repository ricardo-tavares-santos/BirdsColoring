using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using unitycoder_MobilePaint;

public class GameManager : Singleton<GameManager>
{
	public bool isFinalScreen=false;
	public bool isImageClicked = false;
	public int categoryIndex=0;
	public int imageIndex;
	public int colorIndex=0;
	public MobilePaint mobilePaint;
	public Color[] colors;
	public Dictionary<string,BaseItem> imagesList;
	public Dictionary<string,BaseItem> colorsList;


	
	#region Pause
	
	public bool isPaused = false;
	
	// The Sound Manager	
	public bool isSoundPaused;
	
	public void PauseGame ()
	{
		isPaused = true;
		Time.timeScale = 0.0f;
	}
	
	public void UnPauseGame ()
	{
		isPaused = false;
		Time.timeScale = 1.0f;
	}
	
	#endregion Pause
	
	#region Mono Life Cycle
	
	void Awake ()
	{
		GetData ();
		if (GameObject.FindGameObjectsWithTag ("GameManager").Length > 1) {
			Destroy (gameObject);
		} else {
			DontDestroyOnLoad (gameObject);
		}
		
		GameManager.Instance.isSoundPaused = false;
	}

	void GetData()
	{
		imagesList = DataProvider.GetImagesData ();
		colorsList = DataProvider.GetColorsData ();
	}

	public void OnDestroy ()
	{
		if (GameObject.FindGameObjectsWithTag ("GameManager").Length < 1) {
			applicationIsQuitting = true;
		}
	}

	public void SetBrushColor(int index)
	{
		colorIndex = index - 1;
		mobilePaint.paintColor = colors[colorIndex];
		//mobilePaint.drawMode = DrawMode.Default;
	}

	public void SetBrushSize(int size)
	{
		mobilePaint.brushSize = size;
	}

	public void ChangeDrawMode(int index)
	{
		mobilePaint.drawMode = DrawMode.CustomBrush;
		mobilePaint.selectedBrush = index-1;
		mobilePaint.ReadCurrentCustomBrush();
	}

	public void SetFloodFillMode()
	{
		mobilePaint.drawMode = DrawMode.FloodFill;
	}

	public void SetDefaultMode()
	{
		mobilePaint.drawMode = DrawMode.Default;
		mobilePaint.paintColor = colors[0];
	}

	#endregion Mono Life Cycle
}
