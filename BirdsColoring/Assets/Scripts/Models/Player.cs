using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using System;

public class Player : MonoBehaviour
{
	public int score = 0;
	public int stars = 0; 
	public int maxStars = 25;

	//public Dictionary<string, int> levelScores = new Dictionary<string, int>();
	//public Dictionary<string, int> levelStars = new Dictionary<string, int>();


	private const string GAME_LAST_OPENED = "VAOpened";				//for video ad 
	private const string Video_Bonus_Index = "VAIndex";				

	void Start()
	{
		getData ();
		if(PlayerPrefs.GetString("IsPlayerScoreSet") !="True")
		{
			SavePlayerScore ();
			PlayerPrefs.SetString("IsPlayerScoreSet","True");
			PlayerPrefs.Save();
		}
		else
			score = PlayerPrefs.GetInt ("PlayerScore");

		initializeDailyVideoAdCount();
		IsBonusReady ();
	}
	
	void initializeDailyVideoAdCount()
	{
		DateTime currentTime = DateTime.Now;
		DateTime lastOpened = Convert.ToDateTime(PlayerPrefs.GetString (GAME_LAST_OPENED, currentTime.ToString ()));
		
		if (lastOpened.ToString().CompareTo(currentTime.ToString()) == 0) 
		{
			saveCurrentTime ();
		} 
	}
	private void saveCurrentTime()
	{
		PlayerPrefs.SetString (GAME_LAST_OPENED, DateTime.Now.ToString ());
		PlayerPrefs.SetInt (Video_Bonus_Index, 2);	
		PlayerPrefs.Save ();
	}
	
	public void IsBonusReady()
	{
		DateTime currentTime = DateTime.Now;
		
		DateTime lastOpened = Convert.ToDateTime(PlayerPrefs.GetString (GAME_LAST_OPENED,currentTime.ToString ()));
		
		TimeSpan timeDiff = currentTime.Subtract(lastOpened);
		print (timeDiff.Days);
		
		if(timeDiff.Days >= 1)
		{
			PlayerPrefs.SetString (GAME_LAST_OPENED,currentTime.ToString ());
			PlayerPrefs.SetInt (Video_Bonus_Index,2);
		}
		
	}
	void getData()
	{

	}
	
	void initArray(ref BaseItem[] baseArray, ArrayList list)
	{
		if (baseArray != null && list != null) 
		{
			baseArray = new BaseItem[list.Count];
			for(int index = 0; index < list.Count; index++)		
			{
				baseArray[index] =(BaseItem)list[index];
			}
		}
	}
	
	public int GetPlayerScore()
	{
		return PlayerPrefs.GetInt ("PlayerScore");
	}
	public void SetPlayerScore(int value)
	{
		score = value;
		SavePlayerScore ();
	
	}
	public void UpdatePlayerScore(int value)
	{
		score += value;
		SavePlayerScore ();

	}
	public void SavePlayerScore()
	{
		PlayerPrefs.SetInt ("PlayerScore",score);
		PlayerPrefs.Save ();
	}
	public bool DeductPlayerCoins(int value)
	{
		print (score);

		if (score - value >= 0) 
		{
			score = score - value;
			SavePlayerScore ();
		
			return true;
		}
		else
			return false;
	}

}
