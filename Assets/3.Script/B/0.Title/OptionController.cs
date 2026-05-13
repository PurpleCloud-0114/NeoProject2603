using UnityEngine;
using UnityEngine.UI;

public enum ControlPreset { JoyRight = 0, JoyLeft = 1, GyroRight = 2, GyroLeft = 3 }

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

    [Header("Sliders")]
    [SerializeField] private Slider _sensitivity_slider;
    [SerializeField] private Slider _bgm_slider;
    [SerializeField] private Slider _sfx_slider;

    [Header("Slider Visuals")]
    [SerializeField] private Image _sensitivity_fill_img;
    [SerializeField] private Image _sensitivity_handle_img;
    [SerializeField] private Color _disabled_color = new Color(0.5f, 0.5f, 0.5f, 0.5f);

    [Header("Navigation")]
    [SerializeField] private Button _back_button;

    private Color _original_fill_color;
    private Color _original_handle_color;
    private ControlPreset _currentPreset;

    private void Start()
    {
        // 초기 컬러 백업 및 슬라이더 이미지 캐싱
        if (_sensitivity_slider != null)
        {
            if (_sensitivity_fill_img == null) _sensitivity_fill_img = _sensitivity_slider.fillRect.GetComponent<Image>();
            if (_sensitivity_handle_img == null) _sensitivity_handle_img = _sensitivity_slider.handleRect.GetComponent<Image>();
            _original_fill_color = _sensitivity_fill_img.color;
            _original_handle_color = _sensitivity_handle_img.color;
        }

        // 데이터 로드 및 슬라이더 초기화
        _currentPreset = (ControlPreset)PlayerPrefs.GetInt(_PRESET_KEY, 0);
        _sensitivity_slider.value = PlayerPrefs.GetFloat(_SENSITIVITY_KEY, 6f);
        _bgm_slider.value = PlayerPrefs.GetFloat(_BGM_KEY, 0.6f);
        _sfx_slider.value = PlayerPrefs.GetFloat(_SFX_KEY, 0.6f);

        // 버튼 리스너 등록
        _joy_right_button.onClick.AddListener(() => SetPreset(ControlPreset.JoyRight));
        _joy_left_button.onClick.AddListener(() => SetPreset(ControlPreset.JoyLeft));
        _gyro_right_button.onClick.AddListener(() => SetPreset(ControlPreset.GyroRight));
        _gyro_left_button.onClick.AddListener(() => SetPreset(ControlPreset.GyroLeft));
        _back_button.onClick.AddListener(BackToTitle);

        // 슬라이더 리스너 (실시간 볼륨 조절)
        _bgm_slider.onValueChanged.AddListener((val) => {
            PlayerPrefs.SetFloat(_BGM_KEY, val);
            if (AudioManager.Instance != null) AudioManager.Instance.SetVolume("BGM", val);
        });

        _sfx_slider.onValueChanged.AddListener((val) => {
            PlayerPrefs.SetFloat(_SFX_KEY, val);
            if (AudioManager.Instance != null) AudioManager.Instance.SetVolume("SFX", val);
        });

        _sensitivity_slider.onValueChanged.AddListener((val) => PlayerPrefs.SetFloat(_SENSITIVITY_KEY, val));

        UpdateControlUI();
    }

    public void SetPreset(ControlPreset preset)
    {
        _currentPreset = preset;
        PlayerPrefs.SetInt(_PRESET_KEY, (int)preset);
        UpdateControlUI();
    }

    private void UpdateControlUI()
    {
        UpdateButtonStyle(_joy_right_button, _currentPreset == ControlPreset.JoyRight);
        UpdateButtonStyle(_joy_left_button, _currentPreset == ControlPreset.JoyLeft);
        UpdateButtonStyle(_gyro_right_button, _currentPreset == ControlPreset.GyroRight);
        UpdateButtonStyle(_gyro_left_button, _currentPreset == ControlPreset.GyroLeft);

        bool isGyro = (_currentPreset == ControlPreset.GyroRight || _currentPreset == ControlPreset.GyroLeft);
        _sensitivity_slider.interactable = isGyro;
        _sensitivity_fill_img.color = isGyro ? _original_fill_color : _disabled_color;
        _sensitivity_handle_img.color = isGyro ? _original_handle_color : _disabled_color;
    }

    private void UpdateButtonStyle(Button btn, bool isSelected)
    {
        if (btn.TryGetComponent(out Image img)) img.color = isSelected ? _selected_color : _unselected_color;
        btn.interactable = !isSelected;
    }

    public void BackToTitle()
    {
        PlayerPrefs.Save();
        if (_titlePanel != null) _titlePanel.SetActive(true);
        gameObject.SetActive(false);
    }
}