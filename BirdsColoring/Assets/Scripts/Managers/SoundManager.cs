using UnityEngine;
using System.Collections;

[RequireComponent(typeof(AudioSource))]

public class SoundManager : Singleton<SoundManager> {

	public AudioSource inGameLoopAudioSource;
	public bool isMultipleSounds = false;
	public bool isSound = true;
	
	#region OneShotSoundClips 
	
	/*
	 * Audio Clip Files for One Shot
	 */
	
	public AudioClip buttonClickSound;
	public AudioClip popUpSound;
	public AudioClip shimmer;
	public AudioClip longSparkle;
	public AudioClip celebration;
	public AudioClip gameName;
	public AudioClip[] expression;
	public AudioClip useSoap;
	public AudioClip useShower;
	public AudioClip useSyrup;
	public AudioClip useToothBrush;
	public AudioClip bloonIn;
	public AudioClip bloonOut;
	
	
	
	#endregion
	
	#region InGameLoopSoundClips
	/*
	 * Audio Clip Files for In Game Loop Sound
	 */    
	public AudioClip blenderSound;
	public AudioClip showerSound;
	public AudioClip bubblePushSound;
	public AudioClip babyGigglingSound;
	public AudioClip shampooSound;
	
	#endregion
	
	#region BackGroundSoundClips
	/*
	 * Audio Clip Files for BG Loop Sound
	 */
	public AudioClip menuBGSound;
	public AudioClip gameBGSound;
	public AudioClip endGameBGSound;
	
	#endregion
	
	
	
	#region DefaultMethods
	
	//TODO: Make sure you override your awake by override keyword
	/*override*/ void Awake()
	{
		DontDestroyOnLoad (this.gameObject);
	}
	
	void Start(){
		
		
		/*
         * If Sound is mute previously Mute the sound.
         */
		//TODO : Need to change issound with your player prefrence
		//        if (!isSound)
		//        {
		//            GetComponent<AudioSource>().mute = true;
		//        }
		//        else
		//        {
		//            GetComponent<AudioSource>().mute = false;
		//        }
		
		/*
		 * Checking whether dual sound enable or not.
		 * If Enable setting MenuBGSound and GameBGSound Accourding.
		 * Else always set GameBGSound.
		 */
		
		if (isMultipleSounds) 
		{
			this.PlayMenuBackgroundSound();
		} 
		else 
		{
			this.PlayBackgroundSound();
		}
		
		
		/*
		 * If Sound is not playing Play the musicAudioSource
		 */
		
		if (!GetComponents<AudioSource>()[1].isPlaying)
		{
			GetComponents<AudioSource>()[1].Play();
		}
	}
	
	#endregion
	
	
	
	#region InGameLoopSoundMethods
	
	/*
	 * Stop Playing previous in game loop sound
     * checking is new in game loop sound available playing it.
	 */
	
	public void PlayBlenderSound()
	{
		StopInGameLoop();
		
		if(blenderSound)
		{
			inGameLoopAudioSource.clip = blenderSound;
			inGameLoopAudioSource.Play();
		}
	}
	
	public void PlayShampooSound()
	{
		if (!inGameLoopAudioSource.isPlaying) 
		{
			if (shampooSound) {
				inGameLoopAudioSource.clip = shampooSound;
				inGameLoopAudioSource.Play ();
			}
		}
	}
	
	public void PlayShowerSound()
	{
		StopInGameLoop();
		
		if(showerSound)
		{
			inGameLoopAudioSource.clip = showerSound;
			inGameLoopAudioSource.Play();
		}
	}
	
	public void PlayBubblePushSound()
	{
		StopInGameLoop();
		
		if(bubblePushSound)
		{
			inGameLoopAudioSource.clip = bubblePushSound;
			inGameLoopAudioSource.Play();
		}
	}
	
	public void PlayBabyGiggleSound()
	{
		//StopInGameLoop();
		
		if(babyGigglingSound)
		{
			inGameLoopAudioSource.clip = babyGigglingSound;
			inGameLoopAudioSource.Play();
		}
	}
	
	public void StopInGameLoop()
	{
		inGameLoopAudioSource.Stop();
	}
	
	public void StopOneShotSound()
	{
		GetComponents<AudioSource> () [0].Stop ();
	}
	
	#endregion
	
	#region BGSoundMethods
	
	/*
	 * Playing Different Background Sounds
	 */
	
	public void PlayBackgroundSound() 
	{
		if (gameBGSound)
		{
			GetComponents<AudioSource>()[1].clip = gameBGSound;
		}
	}
	
	public void PlayEndGameSound()
	{
		if (endGameBGSound)
		{
			GetComponents<AudioSource>()[1].clip = endGameBGSound;
		}
	}
	
	public void PlayMenuBackgroundSound()
	{
		if (menuBGSound)
		{
			GetComponents<AudioSource>()[1].clip = menuBGSound;
		}
	}
	
	#endregion
	
	#region OneShotSoundMethods
	
	/*
	 * Playing one shot for each OneShotSound.
     * Muting and UnMuting sound Accordingly.
	 */
	public void PlayButtonClickSound()
	{
		if(buttonClickSound)
		{
			GetComponents<AudioSource>()[0].PlayOneShot(buttonClickSound);
		}
	}
	
	public void PlayPopUpSound()
	{
		if (popUpSound)
		{
			GetComponents<AudioSource>()[0].PlayOneShot(popUpSound);
		}
	}
	
	public void PlayShimmerSound()
	{
		if (shimmer)
		{
			GetComponents<AudioSource>()[0].PlayOneShot(shimmer);
		}
	}
	
	public void PlayLongSparkleSound()
	{
		if(!GetComponents<AudioSource>()[2].isPlaying)
		{
			if (longSparkle)
			{
				GetComponents<AudioSource>()[2].clip = longSparkle;
				GetComponents<AudioSource>()[2].Play();
			}
		}
	}

	public void PlayCelebrationSound()
	{
		if (celebration)
		{
			GetComponents<AudioSource>()[0].PlayOneShot(celebration);
		}
	}
	
	public void PlayGameNameSound()
	{
		if (gameName)
		{
			GetComponents<AudioSource>()[0].PlayOneShot(gameName);
		}
	}
	
	public void PlayUseSoapSound()
	{
		if (useSoap)
		{
			GetComponents<AudioSource>()[0].PlayOneShot(useSoap);
		}
	}
	
	public void PlayUseShowerSound()
	{
		if (useShower)
		{
			GetComponents<AudioSource>()[0].PlayOneShot(useShower);
		}
	}
	
	public void PlayUseSyrupSound()
	{
		if (useSyrup)
		{
			GetComponents<AudioSource>()[0].PlayOneShot(useSyrup);
		}
	}
	
	public void PlayUseToothBrushSound()
	{
		if (useToothBrush)
		{
			GetComponents<AudioSource>()[0].PlayOneShot(useToothBrush);
		}
	}
	
	public void PlayBloonInSound()
	{
		if (bloonIn)
		{
			GetComponents<AudioSource>()[0].PlayOneShot(bloonIn);
		}
	}
	
	public void PlayBloonOutSound()
	{
		if (bloonOut)
		{
			GetComponents<AudioSource>()[0].PlayOneShot(bloonOut);
		}
	}
	
	public void PlayExpressionSound()
	{
		GetComponents<AudioSource>()[0].Stop ();
		if (expression.Length != 0)
		{
			GetComponents<AudioSource>()[0].PlayOneShot(expression[Random.Range(0,expression.Length)]);
		}
		
	}
	
	public void PlayMuteSound()
	{
		isSound = false;
		GetComponent<AudioSource>().mute = true;
		//TODO : Need to save the sound states 
	}
	
	public void PlayUnMuteSound()
	{
		isSound = true;
		GetComponent<AudioSource>().mute = false;
		//TODO : Need to save the sound states 
	}
	
	#endregion
}