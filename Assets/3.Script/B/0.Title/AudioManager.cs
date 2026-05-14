using UnityEngine;
using UnityEngine.Audio;
using System.Collections.Generic;
using Mirror; // NetworkServer 체크를 위해 추가

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
        // 1. 서버 전용 빌드라면 딕셔너리 채우기만 하고 리소스 할당은 건너뜀
        // (서버가 클립 이름은 알아야 RPC를 보낼 수 있음)
        foreach (var s in _bgm) _bgmDictionary[s.name] = s.clip;
        foreach (var s in _sfx) _sfxDictionary[s.name] = s.clip;

        // 서버 전용 빌드(오디오 없음)인 경우 여기서 중단하여 NullReference 방지
        if (isServerOnly) return;

        if (_bgm_player == null && transform.childCount > 0)
            _bgm_player = transform.GetChild(0).GetComponent<AudioSource>();
        if (_sfx_player == null && transform.childCount > 1)
            _sfx_player = transform.GetChild(1).GetComponent<AudioSource>();
    }

    private void Start()
    {
        // 서버 전용 빌드라면 볼륨 설정을 하지 않음
        if (isServerOnly) return;

        SetVolume("BGM", PlayerPrefs.GetFloat("BGM", 0.6f));
        SetVolume("SFX", PlayerPrefs.GetFloat("SFX", 0.6f));
    }

    // 서버인지 체크하는 프로퍼티 (Mirror가 없는 환경에서도 에러 안 나게 안전장치)
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
        // 믹서가 없거나 서버라면 실행 안 함
        if (_audio_mixer == null || isServerOnly) return;

        float dB = Mathf.Log10(Mathf.Max(0.0001f, sliderValue)) * 20;
        _audio_mixer.SetFloat(parameterName, dB);
    }

    public void PlayBGM(string name)
    {
        if (isServerOnly) return; // 서버는 소리를 재생하지 않음

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
        if (isServerOnly) return; // 서버는 소리를 재생하지 않음

        if (_sfxDictionary.TryGetValue(name, out AudioClip clip))
        {
            if (_sfx_player != null) _sfx_player.PlayOneShot(clip);
        }
    }

    // 서버가 클립을 찾을 때 사용 (에러 방지용)
    public AudioClip GetSFXClip(string name)
    {
        if (_sfxDictionary.TryGetValue(name, out AudioClip clip))
        {
            return clip;
        }
        return null;
    }
}