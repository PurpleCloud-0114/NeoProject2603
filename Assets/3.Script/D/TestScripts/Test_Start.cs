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
		_inputController.BasePoint = _inputController.MovePoint;
#elif UNITY_ANDROID && !UNITY_EDITOR
		_inputController.Calibrate();
#endif

		Time.timeScale = 1f;
		gameObject.SetActive(false);
	}
}
