using UnityEngine;
using UnityEngine.UI;
using TMPro;

public enum ControlPreset
{
    JoyRight = 0,
    JoyLeft = 1,
    GyroRight = 2,
    GyroLeft = 3
}

public class OptionController : MonoBehaviour
{
    private const string _PRESET_KEY = "ControlPreset";
    private const string _SENSITIVITY_KEY = "GyroSensitivity";
    private const string _BGM_KEY = "BGM";
    private const string _SFX_KEY = "SFX";

    [Header("UI Panels")]
    [SerializeField] private GameObject _titlePanel;

    [Header("Preset Buttons")]
    [SerializeField] private Button _joy_right_button;
    [SerializeField] private Button _joy_left_button;
    [SerializeField] private Button _gyro_right_button;
    [SerializeField] private Button _gyro_left_button;

    [Header("Color Settings")]
    [SerializeField] private Color _selected_color = Color.white;
    [SerializeField] private Color _unselected_color = Color.gray;

    [Header("Gyro Slider Settings")]
    [SerializeField] private Slider _sensitivity_slider;
    [SerializeField] private Color _slider_disabled_color = new Color(0.5f, 0.5f, 0.5f, 0.5f);
    [SerializeField] private Image _sensitivity_fill_img;
    [SerializeField] private Image _sensitivity_handle_img;

    [Header("Other Sliders")]
    [SerializeField] private Slider _bgm_slider;
    [SerializeField] private Slider _sfx_slider;

    [Header("Navigation")]
    [SerializeField] private Button _back_button;

    private Color _original_fill_color = Color.white;
    private Color _original_handle_color = Color.white;

    // 현재 선택된 프리셋을 Enum 타입으로 관리
    private ControlPreset _currentPreset = ControlPreset.JoyRight;

    private void Start()
    {
        InitSliderCaching();

        // 데이터 로드 (int를 Enum으로 형변환)
        _currentPreset = (ControlPreset)PlayerPrefs.GetInt(_PRESET_KEY, 0);

        _sensitivity_slider.value = PlayerPrefs.GetFloat(_SENSITIVITY_KEY, 1.0f);
        _bgm_slider.value = PlayerPrefs.GetFloat(_BGM_KEY, 0.75f);
        _sfx_slider.value = PlayerPrefs.GetFloat(_SFX_KEY, 0.75f);

        // 리스너 등록 (Enum 값을 인자로 전달)
        _joy_right_button.onClick.AddListener(() => SetPreset(ControlPreset.JoyRight));
        _joy_left_button.onClick.AddListener(() => SetPreset(ControlPreset.JoyLeft));
        _gyro_right_button.onClick.AddListener(() => SetPreset(ControlPreset.GyroRight));
        _gyro_left_button.onClick.AddListener(() => SetPreset(ControlPreset.GyroLeft));
        _back_button.onClick.AddListener(BackToTitle);

        _sensitivity_slider.onValueChanged.AddListener((val) => PlayerPrefs.SetFloat(_SENSITIVITY_KEY, val));

        _bgm_slider.onValueChanged.AddListener((val) => {
            PlayerPrefs.SetFloat(_BGM_KEY, val);
            if (AudioManager.Instance != null) AudioManager.Instance.SetVolume(_BGM_KEY, val);
        });

        _sfx_slider.onValueChanged.AddListener((val) => {
            PlayerPrefs.SetFloat(_SFX_KEY, val);
            if (AudioManager.Instance != null) AudioManager.Instance.SetVolume(_SFX_KEY, val);
        });

        UpdateControlUI();
    }

    private void InitSliderCaching()
    {
        if (_sensitivity_slider != null)
        {
            if (_sensitivity_fill_img == null && _sensitivity_slider.fillRect != null)
                _sensitivity_fill_img = _sensitivity_slider.fillRect.GetComponent<Image>();

            if (_sensitivity_handle_img == null && _sensitivity_slider.handleRect != null)
                _sensitivity_handle_img = _sensitivity_slider.handleRect.GetComponent<Image>();
        }

        if (_sensitivity_fill_img != null) _original_fill_color = _sensitivity_fill_img.color;
        if (_sensitivity_handle_img != null) _original_handle_color = _sensitivity_handle_img.color;
    }

    public void SetPreset(ControlPreset preset)
    {
        _currentPreset = preset;
        // 저장할 때는 다시 int로 형변환
        PlayerPrefs.SetInt(_PRESET_KEY, (int)preset);
        PlayerPrefs.Save();
        UpdateControlUI();
    }

    private void UpdateControlUI()
    {
        UpdateButtonStyle(_joy_right_button, _currentPreset == ControlPreset.JoyRight);
        UpdateButtonStyle(_joy_left_button, _currentPreset == ControlPreset.JoyLeft);
        UpdateButtonStyle(_gyro_right_button, _currentPreset == ControlPreset.GyroRight);
        UpdateButtonStyle(_gyro_left_button, _currentPreset == ControlPreset.GyroLeft);

        // 자이로 모드 판별 (Enum 기반)
        bool isGyroMode = (_currentPreset == ControlPreset.GyroRight || _currentPreset == ControlPreset.GyroLeft);

        if (_sensitivity_slider != null)
        {
            _sensitivity_slider.interactable = isGyroMode;
            if (_sensitivity_fill_img != null)
                _sensitivity_fill_img.color = isGyroMode ? _original_fill_color : _slider_disabled_color;
            if (_sensitivity_handle_img != null)
                _sensitivity_handle_img.color = isGyroMode ? _original_handle_color : _slider_disabled_color;
        }
    }

    private void UpdateButtonStyle(Button btn, bool isSelected)
    {
        if (btn == null) return;
        if (btn.TryGetComponent(out Image img))
        {
            img.color = isSelected ? _selected_color : _unselected_color;
        }
        btn.interactable = !isSelected;
    }

    public void BackToTitle()
    {
        PlayerPrefs.Save();
        if (_titlePanel != null) _titlePanel.SetActive(true);
        gameObject.SetActive(false);
    }
}