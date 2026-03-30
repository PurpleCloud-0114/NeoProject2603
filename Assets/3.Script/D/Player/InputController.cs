using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.InputSystem;

public class InputController : MonoBehaviour {
	public Vector2 base_point;
	public Vector2 move_point;

	public Vector3 gravity;

#if UNITY_ANDROID && !UNITY_EDITOR
	private void Start() {
		if (Accelerometer.current != null) {
			InputSystem.EnableDevice(Accelerometer.current);
		} else {
			Debug.LogError("Accelerometer not found on this device");
		}
		if (Accelerometer.current.enabled) Debug.Log("Accelerometer is enabled");
		Application.targetFrameRate = 60;
	}
#endif
	public void OnMouseMove(InputAction.CallbackContext context) {
		move_point = context.ReadValue<Vector2>();
	}
	public void OnAccelMove(InputAction.CallbackContext context) {
		gravity = context.ReadValue<Vector3>();
		move_point = new Vector2(gravity.x, gravity.y);
	}
}

