using UnityEngine;

public class SetRenderQueueParticles : MonoBehaviour
{
	public int renderQueue;

	private Material mMat;
	private Renderer ren;

	void Start ()
	{
		ren = GetComponent<Renderer>();

		if (ren == null)
		{
			ParticleSystem sys = GetComponent<ParticleSystem>();
			if (sys != null) ren = sys.GetComponent<Renderer>();
		}
	
		if (ren != null)
		{
			mMat = new Material(ren.sharedMaterial);
			mMat.renderQueue = renderQueue;
			ren.material = mMat;
		}
	}

//	void Update(){
//		if (ren != null)
//		{
//			mMat = new Material(ren.sharedMaterial);
//			mMat.renderQueue = renderQueue;
//			ren.material = mMat;
//		}
//	}
	
	void OnDestroy () { 
		if (mMat != null) 
			Destroy(mMat);
	}
}