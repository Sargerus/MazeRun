using System;
using UnityEngine;

public class AudioManager : MonoBehaviour
{
    public Sound[] sounds;
    // Start is called before the first frame update
    void Awake()
    {
        bool isSoundsOn = Convert.ToBoolean(PlayerPrefs.GetInt(PlayerPrefsConst.SoundOn));
        float soundsVolume = PlayerPrefs.GetFloat(PlayerPrefsConst.SoundVolume);

        foreach (Sound s in sounds)
        {
            s.source = gameObject.AddComponent<AudioSource>();
            s.source.clip = s.clip;
            s.source.pitch = s.pitch;
            s.source.volume = isSoundsOn ? soundsVolume : 0;
        }
    }

    public void ChangeVolume()
    {
        bool isSoundsOn = Convert.ToBoolean(PlayerPrefs.GetInt(PlayerPrefsConst.SoundOn));
        float soundsVolume = PlayerPrefs.GetFloat(PlayerPrefsConst.SoundVolume);

        foreach(Sound s in sounds)
        {
            s.source.volume = isSoundsOn ? soundsVolume : 0;
        }
    }

    public void Play(string name)
    {
        Sound s = Array.Find(sounds, sound => sound.Name.Equals(name));
        if (s != null)
            s.source.Play();
    }
}
