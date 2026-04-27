using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class Option : MonoBehaviour
{
    [SerializeField] private PlayerMovement _playerMovement;
    [SerializeField] private PlayerInputSystem _playerInputSystem; // 인스펙터 할당됨

    [Header("Sensitive")]
    [SerializeField] private Slider _sensitiveSlider;

    [Header("Target UI (실제 조작할 UI)")]
    [SerializeField] private GameObject _joyStick;
    [SerializeField] private RectTransform _itemButton;
    [SerializeField] private RectTransform _wingButtons;

    [Header("Position Presets (위치 참고용 렉트)")]
    [SerializeField] private RectTransform _joyLeftRef;
    [SerializeField] private RectTransform _joyRightRef;
    [Space]
    [SerializeField] private RectTransform _itemLeftRef;
    [SerializeField] private RectTransform _itemRightRef;
    [Space]
    [SerializeField] private RectTransform _wingLeftRef;
    [SerializeField] private RectTransform _wingRightRef;

    public void BindPlayer(Transform player)
    {
        if (player.TryGetComponent(out _playerMovement))
        {
            ApplySettings();
        }
    }

    public void OnSensitiveValueChange()
    {
        if (_playerMovement != null)
            _playerMovement.MoveMobileSensitive = _sensitiveSlider.value;
    }

    public void OnJoyStickModChange()
    {
        if (_joyStick != null) _joyStick.SetActive(!_joyStick.activeSelf);
        if (_playerInputSystem != null) _playerInputSystem.OnGravitySensorToggle();
    }

    private void ApplySettings()
    {
        if (_playerMovement == null || _playerInputSystem == null) return;

        // OptionController가 저장한 프리셋 값 로드
        int preset = PlayerPrefs.GetInt("ControlPreset", 0);
        bool isLeft = (preset == 1 || preset == 3);
        bool isGyro = (preset == 2 || preset == 3);

        // 1. 민감도 적용
        _playerMovement.MoveMobileSensitive = PlayerPrefs.GetFloat("GyroSensitivity", 1.0f);

        // 2. 위치 및 활성화 (레퍼런스 렉트 참고)
        if (_joyStick != null)
            SetLayout(_joyStick.GetComponent<RectTransform>(), isLeft ? _joyLeftRef : _joyRightRef, !isGyro);

        SetLayout(_itemButton, isLeft ? _itemLeftRef : _itemRightRef, true);
        SetLayout(_wingButtons, isLeft ? _wingLeftRef : _wingRightRef, true);

        // 3. 인풋 모드 동기화
        if (_playerInputSystem.is_joystick == isGyro)
            _playerInputSystem.OnGravitySensorToggle();
    }

    private void SetLayout(RectTransform target, RectTransform reference, bool active)
    {
        if (target == null || reference == null) return;

        target.gameObject.SetActive(active);

        // 레퍼런스의 모든 위치 정보(앵커, 피벗, 좌표, 크기)를 타겟에 복사
        target.anchorMin = reference.anchorMin;
        target.anchorMax = reference.anchorMax;
        target.pivot = reference.pivot;
        target.anchoredPosition = reference.anchoredPosition;
    }
}