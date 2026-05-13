using UnityEngine;
using UnityEngine.Audio;
using System.Collections.Generic;

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
        // 인스펙터에서 비어있을 경우만 자동 할당 (자식 오브젝트 순서 기준)
        if (_bgm_player == null) _bgm_player = transform.GetChild(0).GetComponent<AudioSource>();
        if (_sfx_player == null) _sfx_player = transform.GetChild(1).GetComponent<AudioSource>();

        // 딕셔너리에 데이터 캐싱
        foreach (var s in _bgm) _bgmDictionary[s.name] = s.clip;
        foreach (var s in _sfx) _sfxDictionary[s.name] = s.clip;
    }

    private void Start()
    {
        // 게임 시작 시 저장된 볼륨 값 적용
        SetVolume("BGM", PlayerPrefs.GetFloat("BGM", 0.6f));
        SetVolume("SFX", PlayerPrefs.GetFloat("SFX", 0.6f));
    }

    public void SetVolume(string parameterName, float sliderValue)
    {
        if (_audio_mixer == null) return;

        // 슬라이더 0~1 값을 -80dB~0dB로 변환
        float dB = Mathf.Log10(Mathf.Max(0.0001f, sliderValue)) * 20;
        _audio_mixer.SetFloat(parameterName, dB);
    }

    public void PlayBGM(string name)
    {
        if (_bgmDictionary.TryGetValue(name, out AudioClip clip))
        {
            if (_bgm_player.clip == clip && _bgm_player.isPlaying) return;
            _bgm_player.clip = clip;
            _bgm_player.Play();
        }
    }

    public void StopBGM() => _bgm_player.Stop();

    public void PlaySFX(string name)
    {
        if (_sfxDictionary.TryGetValue(name, out AudioClip clip))
        {
            _sfx_player.PlayOneShot(clip);
        }
    }

    public AudioClip GetSFXClip(string name)
    {
        if (_sfxDictionary.TryGetValue(name, out AudioClip clip))
        {
            return clip;
        }
        Debug.LogWarning($"SFX Clip {name}을 찾을 수 없습니다!");
        return null;
    }
}
