using UnityEngine;
using System.Collections;

public class BaseItem
{
	
	public enum CurrencyType
	{
		Default = 0,
		XP,
		Coins,
		Gems}

	;

	public string id;
	public string itemName;
	public CurrencyType currencyType;
	public float price;

	public bool isLocked {





		get {
			return (PlayerPrefs.GetString (this.id + "_" + this.itemName, "True").CompareTo ("True") == 0);
		}
		set {
			bool status = (PlayerPrefs.GetString (this.id + "_" + this.itemName, "True").CompareTo ("True") == 0);
			if (value == true && status == true) {
				PlayerPrefs.SetString (this.id + "_" + this.itemName, "True");
			} else if(value == false){
				PlayerPrefs.SetString (this.id + "_" + this.itemName, "False");
			}
		}
	}

	public BaseItem ()
	{
		
	}

	public BaseItem (string id, string itemName, CurrencyType currencyType, float price, bool isLocked)
	{
		this.id = id;
		this.itemName = itemName;
		this.currencyType = currencyType;
		this.price = price;
		this.isLocked = isLocked;
	}
	
}
