using UnityEngine;
using UnityEngine.UI;

public class OptionController : MonoBehaviour
{
    // const 앞에는 [SerializeField]를 붙일 수 없습니다 (상수니까요)
    private const string _PRESET_KEY = "ControlPreset";
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
    [SerializeField] private Color _unselected_color = Color.gray; // 차이를 위해 기본값 조정

    [Header("Gyro Slider Settings")]
    [SerializeField] private Slider _sensitivity_slider;
    [SerializeField] private Color _slider_disabled_color = new Color(0.5f, 0.5f, 0.5f, 0.5f);
    // 슬라이더 에셋의 경우 Fill과 Handle을 직접 연결해주는 것이 가장 안전합니다.
    [SerializeField] private Image _sensitivity_fill_img;
    [SerializeField] private Image _sensitivity_handle_img;

    [Header("Other Sliders")]
    [SerializeField] private Slider _bgm_slider;
    [SerializeField] private Slider _sfx_slider;

    [Header("Navigation")]
    [SerializeField] private Button _back_button;

    private Color _original_fill_color = Color.white;
    private Color _original_handle_color = Color.white;
    private int _currentPresetIndex = 0;

    private void Start()
    {
        // 1. 초기화 및 캐싱
        InitSliderCaching();

        // 2. 데이터 로드
        _currentPresetIndex = PlayerPrefs.GetInt(_PRESET_KEY, 0);
        _sensitivity_slider.value = PlayerPrefs.GetFloat(_SENSITIVITY_KEY, 1.0f);
        _bgm_slider.value = PlayerPrefs.GetFloat(_BGM_KEY, 0.75f);
        _sfx_slider.value = PlayerPrefs.GetFloat(_SFX_KEY, 0.75f);

        // 3. 리스너 등록
        _joy_right_button.onClick.AddListener(() => SetPreset(0));
        _joy_left_button.onClick.AddListener(() => SetPreset(1));
        _gyro_right_button.onClick.AddListener(() => SetPreset(2));
        _gyro_left_button.onClick.AddListener(() => SetPreset(3));
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

        // 4. UI 갱신
        UpdateControlUI();
    }

    private void InitSliderCaching()
    {
        // 인스펙터에서 할당 안 했을 경우를 대비한 자동 찾기 (방어 코드)
        if (_sensitivity_slider != null)
        {
            if (_sensitivity_fill_img == null && _sensitivity_slider.fillRect != null)
                _sensitivity_fill_img = _sensitivity_slider.fillRect.GetComponent<Image>();

            if (_sensitivity_handle_img == null && _sensitivity_slider.handleRect != null)
                _sensitivity_handle_img = _sensitivity_slider.handleRect.GetComponent<Image>();
        }

        // 컬러 캐싱 (Null 체크 필수)
        if (_sensitivity_fill_img != null) _original_fill_color = _sensitivity_fill_img.color;
        if (_sensitivity_handle_img != null) _original_handle_color = _sensitivity_handle_img.color;
    }

    public void SetPreset(int index)
    {
        _currentPresetIndex = index;
        PlayerPrefs.SetInt(_PRESET_KEY, index);
        PlayerPrefs.Save();
        UpdateControlUI();
    }

    private void UpdateControlUI()
    {
        UpdateButtonStyle(_joy_right_button, _currentPresetIndex == 0);
        UpdateButtonStyle(_joy_left_button, _currentPresetIndex == 1);
        UpdateButtonStyle(_gyro_right_button, _currentPresetIndex == 2);
        UpdateButtonStyle(_gyro_left_button, _currentPresetIndex == 3);

        bool isgyromode = (_currentPresetIndex == 2 || _currentPresetIndex == 3);

        if (_sensitivity_slider != null)
        {
            _sensitivity_slider.interactable = isgyromode;

            // 색상 변경 로직
            if (_sensitivity_fill_img != null)
                _sensitivity_fill_img.color = isgyromode ? _original_fill_color : _slider_disabled_color;
            if (_sensitivity_handle_img != null)
                _sensitivity_handle_img.color = isgyromode ? _original_handle_color : _slider_disabled_color;
        }
    }

    private void UpdateButtonStyle(Button btn, bool isselected)
    {
        if (btn == null) return;

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