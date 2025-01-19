using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.Audio;

public class AudioManager : MonoBehaviour
{
    public static AudioManager instance;
    public AudioMixer audioMixer;
    public AudioSource musicSource, sfxSource;
    public AudioClip regularSong;
    public AudioClip secondSong;
    private float transitionDuration = 2.0f;
    [SerializeField] private AudioSource MoveableAudioSource;

    void Start()
    {

        if (instance == null)
        {
            instance = this;
            DontDestroyOnLoad(gameObject);
            //SceneManager.sceneLoaded += OnSceneLoaded;
            musicSource.Play(); //have to have this so that the main menu plays music
        }
        else
        {
            Destroy(gameObject);
        }

        float masterVolume = 0.5f;
        float musicVolume = 0.5f;
        float sfxVolume = 0.5f;

        //apply volumes, override this with settings menu in future
        audioMixer.SetFloat("Master", Mathf.Log10(masterVolume) * 20);
        audioMixer.SetFloat("Music", Mathf.Log10(musicVolume) * 20);
        audioMixer.SetFloat("SFX", Mathf.Log10(sfxVolume) * 20);
    }

    void OnSceneLoaded(Scene scene, LoadSceneMode mode)
    {
        string sceneName = scene.name;

        if (sceneName == "SampleScene")
        {
            //Debug.Log("should be playing map 1 music");
            if (musicSource.clip != regularSong)
            {
                PlayMusic(regularSong);
            }
            else if (!musicSource.isPlaying)
            {
                musicSource.clip = regularSong;
                musicSource.Play();
            }
        }
        else
        {
            //Debug.Log("should be playing regular music");
            if (musicSource.clip != secondSong)
            {
                PlayMusic(secondSong);
            }
            else if (!musicSource.isPlaying)
            {
                musicSource.clip = secondSong;
                musicSource.Play();
            }
        }
    }
    void OnDestroy()
    {
        SceneManager.sceneLoaded -= OnSceneLoaded;
    }

    public void OnMasterVolumeChanged(float volume) //for settings in future
    {
        audioMixer.SetFloat("Master", Mathf.Log10(volume) * 20);
    }

    public void OnMusicVolumeChanged(float volume) //for settings in future
    {
        audioMixer.SetFloat("Music", Mathf.Log10(volume) * 20);
    }

    public void OnSFXVolumeChanged(float volume) //for settings in future
    {
        audioMixer.SetFloat("SFX", Mathf.Log10(volume) * 20);
    }
    public void PlaySFX(AudioClip clip)
    {
        sfxSource.PlayOneShot(clip);
    }

    public void StopSFX()
    {
        sfxSource.Stop();
    }

    public void PlayMusic(AudioClip music)
    {
        //Debug.Log($"Changing music to: {music.name}. Called from: {System.Environment.StackTrace}");

        musicSource.clip = music;
        musicSource.Play();
    }

    public void StopMusic()
    {
        musicSource.Stop();
    }

    private IEnumerator TransitionToSong(AudioClip newSong)
    {
        //Debug.Log("swapping to song: "+newSong);
        float currentTime = 0f;
        float startVolume = musicSource.volume;

        // Fade out the current song
        while (currentTime < transitionDuration)
        {
            currentTime += Time.deltaTime;
            musicSource.volume = Mathf.Lerp(startVolume, 0, currentTime / transitionDuration);
            yield return null;
        }

        // Switch to the new song
        musicSource.clip = newSong;
        musicSource.Play();

        // Fade in the new song
        currentTime = 0f;
        while (currentTime < transitionDuration)
        {
            currentTime += Time.deltaTime;
            musicSource.volume = Mathf.Lerp(0, startVolume, currentTime / transitionDuration);
            yield return null;
        }

        musicSource.volume = startVolume; // Ensure the volume is set back to the original value
    }

    public void PlaySoundAtLocation(Transform location, AudioClip soundToPlay)
    {
        MoveableAudioSource.transform.position = location.position;

        MoveableAudioSource.PlayOneShot(soundToPlay); //need to use playoneshot to allow layering.
    }

    public void PlayAnySound(AudioClip soundToPlay)
    {
        sfxSource.PlayOneShot(soundToPlay);
    }
}
