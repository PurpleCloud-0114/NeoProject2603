using UnityEngine;
using UnityEngine.Audio;

[System.Serializable]
public class Sound
{
    public string name;
    public AudioClip clip;
}

public class AudioManager : MonoBehaviour
{
    public static AudioManager Instance;

    [SerializeField] private AudioMixer _audio_mixer;

    [Space(10f)]
    [Header("Audio Clip")]
    [Space(10f)]
    [SerializeField] private Sound[] _bgm;
    [SerializeField] private Sound[] _sfx;

    [Space(50f)]
    [Header("Audio Source")]
    [Space(10f)]
    [SerializeField] private AudioSource _bgm_player;
    [SerializeField] private AudioSource _sfx_player;

    private void Awake()
    {
        if (Instance == null)
        {
            Instance = this;
            DontDestroyOnLoad(gameObject);
        }
        else
        {
            Destroy(gameObject);
        }
        AutoSetting();
    }

    private void Start()
    {
        float bgmVol = PlayerPrefs.GetFloat("BGM", 0.75f);
        float sfxVol = PlayerPrefs.GetFloat("SFX", 0.75f);

        SetVolume("BGM", bgmVol);
        SetVolume("SFX", sfxVol);

    }

    public void SetVolume(string parametername, float slidervalue)
    {
        float dB = Mathf.Log10(Mathf.Max(0.0001f, slidervalue)) * 20;

        _audio_mixer.SetFloat(parametername, dB);
    }

    private void AutoSetting()
    {
        _bgm_player = transform.GetChild(0).GetComponent<AudioSource>();
        _sfx_player = transform.GetChild(1).GetComponent<AudioSource>();
    }

    public void PlayBGM(string name)
    {
        foreach (Sound s in _bgm)
        {
            if (s.name.Equals(name))
            {
                _bgm_player.clip = s.clip;
                _bgm_player.Play();
                break;
            }
        }
    }
    public void StopBGM()
    {
        _bgm_player.Stop();
    }

    public void PlaySFX(string name)
    {
        foreach (Sound s in _sfx)
        {
            if (s.name.Equals(name))
            {
                _sfx_player.PlayOneShot(s.clip);
            }
        }
    }
}