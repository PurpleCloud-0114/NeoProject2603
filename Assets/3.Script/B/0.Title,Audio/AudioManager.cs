using UnityEngine;
using UnityEngine.Audio;
using System.Collections.Generic;
using Mirror;

[System.Serializable]
public class Sound
{
    public string name;
    public AudioClip clip;
}

public class AudioManager : MonoBehaviour
{
    public static AudioManager Instance;

    [Header("Settings")]
    [SerializeField] private AudioMixer _audio_mixer;

    [Header("Audio Clips")]
    [SerializeField] private Sound[] _bgm;
    [SerializeField] private Sound[] _sfx;

    [Header("Audio Source References")]
    [SerializeField] private AudioSource _bgm_player;
    [SerializeField] private AudioSource _sfx_player;

    private Dictionary<string, AudioClip> _bgmDictionary = new Dictionary<string, AudioClip>();
    private Dictionary<string, AudioClip> _sfxDictionary = new Dictionary<string, AudioClip>();

    private void Awake()
    {
        if (Instance == null)
        {
            Instance = this;
            DontDestroyOnLoad(gameObject);
            Init();
        }
        else
        {
            Destroy(gameObject);
        }
    }

    private void Init()
    {
        // 서버 전용 빌드라면 딕셔너리 채우기만 하고 리소스 할당은 건너뜀
        // (서버가 클립 이름은 알아야 RPC를 보낼 수 있음)
        foreach (var s in _bgm) _bgmDictionary[s.name] = s.clip;
        foreach (var s in _sfx) _sfxDictionary[s.name] = s.clip;

        if (isServerOnly) return;

        if (_bgm_player == null && transform.childCount > 0)
            _bgm_player = transform.GetChild(0).GetComponent<AudioSource>();
        if (_sfx_player == null && transform.childCount > 1)
            _sfx_player = transform.GetChild(1).GetComponent<AudioSource>();
    }

    private void Start()
    {
        if (isServerOnly) return;

        SetVolume("BGM", PlayerPrefs.GetFloat("BGM", 0.6f));
        SetVolume("SFX", PlayerPrefs.GetFloat("SFX", 0.6f));
    }

    private bool isServerOnly
    {
        get
        {
#if UNITY_SERVER
                return true;
#else
            return false;
#endif
        }
    }

    public void SetVolume(string parameterName, float sliderValue)
    {
        if (_audio_mixer == null || isServerOnly) return;

        float dB = Mathf.Log10(Mathf.Max(0.0001f, sliderValue)) * 20;
        _audio_mixer.SetFloat(parameterName, dB);
    }

    public void PlayBGM(string name)
    {
        if (isServerOnly) return;

        if (_bgmDictionary.TryGetValue(name, out AudioClip clip))
        {
            if (_bgm_player != null)
            {
                if (_bgm_player.clip == clip && _bgm_player.isPlaying) return;
                _bgm_player.clip = clip;
                _bgm_player.Play();
            }
        }
    }

    public void StopBGM()
    {
        if (isServerOnly || _bgm_player == null) return;
        _bgm_player.Stop();
    }

    public void PlaySFX(string name)
    {
        if (isServerOnly) return;

        if (_sfxDictionary.TryGetValue(name, out AudioClip clip))
        {
            if (_sfx_player != null) _sfx_player.PlayOneShot(clip);
        }
    }

    public AudioClip GetSFXClip(string name)
    {
        if (_sfxDictionary.TryGetValue(name, out AudioClip clip))
        {
            return clip;
        }
        return null;
    }
}