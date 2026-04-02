using UnityEngine;
using UnityEngine.UI;

public class OptionController : MonoBehaviour
{
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

    // --- 캐싱용 변수 ---
    private Color _original_fill_color;
    private Color _original_handle_color;
    private Image _sensitivity_fill_img;
    private Image _sensitivity_handle_img;

    private int _currentPresetIndex = 0;

    private void Start()
    {
        // 1. 자이로 슬라이더 컴포넌트 및 원래 색상 캐싱
        if (_sensitivity_slider != null)
        {
            if (_sensitivity_slider.fillRect != null)
            {
                _sensitivity_fill_img = _sensitivity_slider.fillRect.GetComponent<Image>();
                _original_fill_color = _sensitivity_fill_img.color; // 원래 색 캐싱
            }
            if (_sensitivity_slider.handleRect != null)
            {
                _sensitivity_handle_img = _sensitivity_slider.handleRect.GetComponent<Image>();
                _original_handle_color = _sensitivity_handle_img.color; // 원래 색 캐싱
            }
        }

        _joy_right_button.onClick.AddListener(() => SetPreset(0));
        _joy_left_button.onClick.AddListener(() => SetPreset(1));
        _gyro_right_button.onClick.AddListener(() => SetPreset(2));
        _gyro_left_button.onClick.AddListener(() => SetPreset(3));
        _back_button.onClick.AddListener(BackToTitle);

        _sensitivity_slider.onValueChanged.AddListener((val) => Debug.Log($"자이로 감도: {val}"));
        _bgm_slider.onValueChanged.AddListener((val) => Debug.Log($"BGM: {val}"));
        _sfx_slider.onValueChanged.AddListener((val) => Debug.Log($"SFX: {val}"));

        UpdateControlUI();
    }

    public void SetPreset(int index)
    {
        _currentPresetIndex = index;
        UpdateControlUI();
    }

    private void UpdateControlUI()
    {
        UpdateButtonStyle(_joy_right_button, _currentPresetIndex == 0);
        UpdateButtonStyle(_joy_left_button, _currentPresetIndex == 1);
        UpdateButtonStyle(_gyro_right_button, _currentPresetIndex == 2);
        UpdateButtonStyle(_gyro_left_button, _currentPresetIndex == 3);

        bool isGyroMode = (_currentPresetIndex == 2 || _currentPresetIndex == 3);

        _sensitivity_slider.interactable = isGyroMode;

        if (_sensitivity_fill_img != null)
        {
            _sensitivity_fill_img.color = isGyroMode ? _original_fill_color : _slider_disabled_color;
        }
        if (_sensitivity_handle_img != null)
        {
            _sensitivity_handle_img.color = isGyroMode ? _original_handle_color : _slider_disabled_color;
        }
    }

    private void UpdateButtonStyle(Button btn, bool isSelected)
    {
        if (btn.TryGetComponent(out Image img))
        {
            img.color = isSelected ? _selected_color : _unselected_color;
        }
        btn.interactable = !isSelected;
    }

    public void BackToTitle()
    {
        if (_titlePanel != null) _titlePanel.SetActive(true);
        gameObject.SetActive(false);
    }
}