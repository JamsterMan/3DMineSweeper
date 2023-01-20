using UnityEngine.Audio;
using UnityEngine;
using System;

public class AudioManager : MonoBehaviour
{

    public Sound[] sounds;
    public AudioMixerGroup soundGroup;
    public AudioMixerGroup musicGroup;

    
    void Awake()
    {
        foreach (Sound s in sounds) {
            s.source = gameObject.AddComponent<AudioSource>();
            s.source.clip = s.clip;

            s.source.volume = s.volume;
            s.source.pitch = s.pitch;

            if (s.isMusic) {
                s.source.outputAudioMixerGroup = musicGroup;
                s.source.loop = true;
            } else {
                s.source.outputAudioMixerGroup = soundGroup;
            }
        
        }
    }

    private void Start()
    {
        Play("Music");
    }

    //plays the sound corrisponding to soundName
    public void Play(string soundName)
    {
        Sound s = Array.Find(sounds, sound => sound.name == soundName);
        if (s == null)
            return;
        s.source.Play();

    }

    public void ButtonClickSound()
    {
        Play("ButtonClick");
    }
}
