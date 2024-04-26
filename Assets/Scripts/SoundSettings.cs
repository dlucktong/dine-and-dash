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

    private bool masterMuted;
    private bool musicMuted;
    private float defaultMusicVolume;

    private void Start()
    {
        musicMixer.audioMixer.GetFloat("MusicVolume", out defaultMusicVolume);
    }

    public void MuteAll()
    {
        if (masterMuted)
        {
            master.audioMixer.SetFloat("MasterVolume", 0);
            audioButtonImage.sprite = unmute;
            masterMuted = !masterMuted;
        }
        else
        {
            master.audioMixer.SetFloat("MasterVolume", -80);
            audioButtonImage.sprite = mute;
            masterMuted = !masterMuted;
        }
    }

    public void MuteMusic()
    {
        if (musicMuted)
        {
            musicMixer.audioMixer.SetFloat("MusicVolume", defaultMusicVolume);
            musicButtonImage.sprite = music;
            musicMuted = !musicMuted;
        }
        else
        {
            musicMixer.audioMixer.SetFloat("MusicVolume", -80);
            musicButtonImage.sprite = nomusic;
            musicMuted = !musicMuted;
        }
    }
}
