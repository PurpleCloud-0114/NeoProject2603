using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.InputSystem;

public class PlayerInputSystem : MonoBehaviour {
	public Vector2 BasePoint;
	public Vector2 MovePoint;

	public Vector3 Gravity;

	private Vector3 _calibratedGravity;
	private Quaternion _calibrationRotation = Quaternion.identity;

	public bool is_joystick = false;

	private PlayerInput _playerInput;

	private void Awake() {
		TryGetComponent(out _playerInput);
	}

	private void Start() {
#if UNITY_ANDROID && !UNITY_EDITOR
		if (GravitySensor.current != null) {
			InputSystem.EnableDevice(GravitySensor.current);
		} else {
			Debug.LogError("GravitySensor not found on this device");
		}
		if (GravitySensor.current.enabled) Debug.Log("GravitySensor is enabled");
#endif
		Application.targetFrameRate = 60;
	}
	public void OnMoveVector2(InputAction.CallbackContext context) {
		if (!is_joystick) return;
		MovePoint = context.ReadValue<Vector2>();
	}
	public void OnMoveVector3(InputAction.CallbackContext context) {
		if (is_joystick) return;
		Gravity = context.ReadValue<Vector3>();
		_calibratedGravity = _calibrationRotation * Gravity;
		MovePoint = new Vector2(_calibratedGravity.x, _calibratedGravity.y);
	}

	public void OnGravitySensorToggle() {
		is_joystick = !is_joystick; //Toggle
		if (is_joystick) {
			//조이스틱 활성화
			SetBasePoint(Vector2.zero);
		} 
		else {
			//그래비티 활성화
			Calibrate();
		}
	}

	public void SetBasePoint(Vector2 curPoint) => BasePoint = curPoint;

	public void Calibrate() {
		if(GravitySensor.current != null) {
			//현재 플레이어가 들고 있는 상태 그대로의 중력 벡터를 가져옵니다.
			Vector3 rawGravity = GravitySensor.current.gravity.ReadValue();
			SetBasePoint(rawGravity);

			//핵심 원리: '현재 기울기(rawGravity)'를 '완벽한 평면(Vector3.back)'으로 
			//강제로 돌려버리는 회전값을 계산해서 저장해 둡니다.
			//(참고: Vector3.back은 (0, 0, -1)로, 스마트폰 화면이 하늘을 똑바로 보는 평면 상태를 의미합니다)
			_calibrationRotation = Quaternion.FromToRotation(rawGravity, Vector3.back);
		}
	}

	public void DisableInputSystem() {
		_playerInput.actions.Disable();
	}

	public void EnableInputSystem() {
		_playerInput.actions.Enable();
	}
}

