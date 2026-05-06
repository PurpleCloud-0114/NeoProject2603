using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.InputSystem.Layouts;
using UnityEngine.InputSystem.OnScreen;


public class FloatingController : OnScreenControl, IPointerDownHandler, IPointerUpHandler, IDragHandler {
    [InputControl(layout = "Vector2")]
    [SerializeField] private string m_ControlPath = "<Gamepad>/leftStick"; // Left Stick [Gamepad]에 매핑

    public RectTransform background; // 조이스틱 배경
    public RectTransform handle;     // 움직이는 손잡이
    public float movementRange = 100f; // 핸들 이동 반경

    protected override string controlPathInternal {
        get => m_ControlPath;
        set => m_ControlPath = value;
    }

    private void Start() {
        // 시작 시 조이스틱 숨김
        background.gameObject.SetActive(false);
    }

    public void OnPointerDown(PointerEventData eventData) {
        // 터치한 위치로 배경 이동 후 활성화
        background.position = eventData.position;
        background.gameObject.SetActive(true);
        OnDrag(eventData);
    }

    public void OnDrag(PointerEventData eventData) {
        Vector2 position = eventData.position;
        Vector2 center = background.position;
        Vector2 delta = position - center;

        // 이동 반경 제한
        Vector2 clampedDelta = Vector2.ClampMagnitude(delta, movementRange);
        handle.position = center + clampedDelta;

        // New Input System으로 정규화된 벡터(-1.0 ~ 1.0) 전송
        SendValueToControl(clampedDelta / movementRange);
    }

    public void OnPointerUp(PointerEventData eventData) {
        // 터치 종료 시 숨기고 입력 초기화
        background.gameObject.SetActive(false);
        handle.anchoredPosition = Vector2.zero;
        SendValueToControl(Vector2.zero);
    }
}