using UnityEngine;
using System.Collections;

public class BaloonPositionReset : MonoBehaviour {

	public BaloonController bController;

	public void ResetTheBaloonPosition(){
		bController.RepositionElement();
	}
}
