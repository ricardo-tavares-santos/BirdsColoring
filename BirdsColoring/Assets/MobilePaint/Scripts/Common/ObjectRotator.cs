using UnityEngine;
using System.Collections;

namespace unitycoder_MobilePaint_samples
{

	public class ObjectRotator : MonoBehaviour 
	{
		// just a simple object rotation test

		public float rotateSpeed = 10;

		void Update () 
		{
			transform.Rotate(0,rotateSpeed * Time.deltaTime,0);
		}
	}
}