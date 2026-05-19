using UnityEngine;
using Mirror;

public class Audio3DHandler : NetworkBehaviour
{
    [Header("Settings")]
    [SerializeField] private AudioSource _source;

    // AudioManager에서 클립을 가져오기 위한 이름 (선택 사항)
    // 혹은 직접 AudioClip을 할당해도 됩니다.

    private void Awake()
    {
        if (_source == null) _source = GetComponent<AudioSource>();

        // 중요: 3D 설정을 스크립트에서 강제 (실수 방지)
        _source.spatialBlend = 1.0f; // 100% 3D 사운드
        _source.playOnAwake = false;
    }

    /// <summary>
    /// 서버가 모든 클라이언트에게 특정 사운드 재생을 명령합니다.
    /// </summary>
    [ClientRpc]
    public void RpcPlay3DSound(string sfxName)
    {
        // 기존 AudioManager의 딕셔너리에서 클립을 참조하여 재생
        // (AudioManager가 Public Dictionary나 GetClip 함수를 가지고 있어야 함)
        // 여기서는 예시로 AudioManager.Instance를 활용합니다.

        // PlayOneShot을 사용하여 중첩 소음이 가능하게 합니다.
        // AudioManager에 GetClip(string name) 함수를 추가했다고 가정합니다.
        AudioClip clip = AudioManager.Instance.GetSFXClip(sfxName);
        if (clip != null)
        {
            _source.PlayOneShot(clip);
        }
    }
}