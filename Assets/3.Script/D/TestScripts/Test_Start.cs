using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Test_Start : MonoBehaviour {
	[SerializeField] private InputController _inputController;
	[SerializeField] private Camera _mainCamera;

	private void Awake() {
		Time.timeScale = 0f;
	}

	public void OnBtnClick() {
#if UNITY_EDITOR
		_inputController.base_point = _inputController.move_point;
#elif UNITY_ANDROID && !UNITY_EDITOR
		_inputController.base_point = _inputController.gravity;
#endif
		Time.timeScale = 1f;
		gameObject.SetActive(false);
	}
}
