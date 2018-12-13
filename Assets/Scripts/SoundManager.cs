using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class SoundManager : MonoBehaviour {
    public AudioClip crashSound;
    public AudioSource efxSource;
    public static SoundManager instance = null;

    private void Awake()
    {
        if (instance == null)
            instance = this;
        else if (instance != this)
            Destroy(gameObject);
    }

    public void playCrashSound(){
        PlaySingle(crashSound);
    }

    // Use this for initialization
    void Start () {


	}

    public void PlaySingle(AudioClip clip)
    {
        efxSource.clip = clip;
        efxSource.Play();
    }
	
	// Update is called once per frame
	void Update () {
		
	}
}
