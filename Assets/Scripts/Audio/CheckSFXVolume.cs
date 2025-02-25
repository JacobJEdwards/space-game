using UnityEngine;

namespace Audio {
	public class CheckSfxVolume : MonoBehaviour {
		public void  Start (){
			GetComponent<AudioSource>().volume = PlayerPrefs.GetFloat("SFXVolume");
		}

		public void UpdateVolume (){
			GetComponent<AudioSource>().volume = PlayerPrefs.GetFloat("SFXVolume");
		}
	}
}