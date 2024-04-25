using UnityEngine;
using UnityEngine.Audio;
using UnityEngine.UI;

public class SoundSettings : MonoBehaviour
{
    public Sprite mute;
    public Sprite unmute;
    public Sprite music;
    public Sprite nomusic;
    public AudioMixerGroup master;
    public AudioMixerGroup musicMixer;

    public Image audioButtonImage;
    public Image musicButtonImage;

    private bool _masterMuted;
    private bool _musicMuted;
    private float _defaultMusicVolume;

    private void Start()
    {
        musicMixer.audioMixer.GetFloat("MusicVolume", out _defaultMusicVolume);
    }

    public void MuteAll()
    {
        if (_masterMuted)
        {
            master.audioMixer.SetFloat("MasterVolume", 0);
            audioButtonImage.sprite = unmute;
            _masterMuted = !_masterMuted;
        }
        else
        {
            master.audioMixer.SetFloat("MasterVolume", -80);
            audioButtonImage.sprite = mute;
            _masterMuted = !_masterMuted;
        }
    }

    public void MuteMusic()
    {
        if (_musicMuted)
        {
            musicMixer.audioMixer.SetFloat("MusicVolume", _defaultMusicVolume);
            musicButtonImage.sprite = music;
            _musicMuted = !_musicMuted;
        }
        else
        {
            musicMixer.audioMixer.SetFloat("MusicVolume", -80);
            musicButtonImage.sprite = nomusic;
            _musicMuted = !_musicMuted;
        }
    }
}
