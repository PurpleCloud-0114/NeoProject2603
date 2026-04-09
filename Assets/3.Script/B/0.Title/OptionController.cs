using UnityEngine;
using UnityEngine.UI;

public class OptionController : MonoBehaviour
{

    [SerializeField]private const string _PRESET_KEY = "ControlPreset";
    //_currentPresetIndex = PlayerPrefs.GetInt(_PRESET_KEY, 0); 이런식으로 뽑아서 UI표시
    private const string _SENSITIVITY_KEY = "GyroSensitivity";
    private const string _BGM_KEY = "BGM";
    private const string _SFX_KEY = "SFX";

    [Header("UI Panels")]
    [SerializeField] private GameObject _titlePanel;

    [Header("Preset Buttons (4-Way)")]
    [SerializeField] private Button _joy_right_button;
    [SerializeField] private Button _joy_left_button;
    [SerializeField] private Button _gyro_right_button;
    [SerializeField] private Button _gyro_left_button;

    [Header("Color Settings (Buttons)")]
    [SerializeField] private Color _selected_color = Color.white;
    [SerializeField] private Color _unselected_color = Color.white;

    [Header("Gyro Slider Disabled Color")]
    [SerializeField] private Color _slider_disabled_color = Color.white;

    [Header("Settings Sliders")]
    [SerializeField] private Slider _sensitivity_slider;
    [SerializeField] private Slider _bgm_slider;
    [SerializeField] private Slider _sfx_slider;

    [Header("Navigation")]
    [SerializeField] private Button _back_button;

    private Color _original_fill_color;
    private Color _original_handle_color;
    private Image _sensitivity_fill_img;
    private Image _sensitivity_handle_img;

    private int _currentPresetIndex = 0;

    private void Start()
    {
        InitSliderCaching();

        _currentPresetIndex = PlayerPrefs.GetInt(_PRESET_KEY, 0);
        float savedSensitivity = PlayerPrefs.GetFloat(_SENSITIVITY_KEY, 1.0f);
        float savedBGM = PlayerPrefs.GetFloat(_BGM_KEY, 0.75f);
        float savedSFX = PlayerPrefs.GetFloat(_SFX_KEY, 0.75f);

        _sensitivity_slider.value = savedSensitivity;
        _bgm_slider.value = savedBGM;
        _sfx_slider.value = savedSFX;

        _joy_right_button.onClick.AddListener(() => SetPreset(0));
        _joy_left_button.onClick.AddListener(() => SetPreset(1));
        _gyro_right_button.onClick.AddListener(() => SetPreset(2));
        _gyro_left_button.onClick.AddListener(() => SetPreset(3));
        _back_button.onClick.AddListener(BackToTitle);

        _sensitivity_slider.onValueChanged.AddListener((val) => {
            PlayerPrefs.SetFloat(_SENSITIVITY_KEY, val);
        });

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
            if (_sensitivity_slider.fillRect != null)
            {
                _sensitivity_fill_img = _sensitivity_slider.fillRect.GetComponent<Image>();
                _original_fill_color = _sensitivity_fill_img.color;
            }
            if (_sensitivity_slider.handleRect != null)
            {
                _sensitivity_handle_img = _sensitivity_slider.handleRect.GetComponent<Image>();
                _original_handle_color = _sensitivity_handle_img.color;
            }
        }
    }

    public void SetPreset(int index)
    {
        _currentPresetIndex = index;
        PlayerPrefs.SetInt(_PRESET_KEY, index); // 프리셋 변경 시 저장
        PlayerPrefs.Save(); // 명시적 저장
        UpdateControlUI();
    }

    private void UpdateControlUI()
    {
        UpdateButtonStyle(_joy_right_button, _currentPresetIndex == 0);
        UpdateButtonStyle(_joy_left_button, _currentPresetIndex == 1);
        UpdateButtonStyle(_gyro_right_button, _currentPresetIndex == 2);
        UpdateButtonStyle(_gyro_left_button, _currentPresetIndex == 3);

        bool isgyromode = (_currentPresetIndex == 2 || _currentPresetIndex == 3);
        _sensitivity_slider.interactable = isgyromode;

        if (_sensitivity_fill_img != null)
        {
            _sensitivity_fill_img.color = isgyromode ? _original_fill_color : _slider_disabled_color;
        }
        if (_sensitivity_handle_img != null)
        {
            _sensitivity_handle_img.color = isgyromode ? _original_handle_color : _slider_disabled_color;
        }
    }

    private void UpdateButtonStyle(Button btn, bool isselected)
    {
        if (btn.TryGetComponent(out Image img))
        {
            img.color = isselected ? _selected_color : _unselected_color;
        }
        btn.interactable = !isselected;
    }

    public void BackToTitle()
    {
        PlayerPrefs.Save();
        if (_titlePanel != null) _titlePanel.SetActive(true);
        gameObject.SetActive(false);
    }
}