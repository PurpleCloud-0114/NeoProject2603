using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.SceneManagement;

public class LoadingManager : MonoBehaviour {
	public static LoadingManager Instance;

	[SerializeField] private GameObject _loadingCanvas;
	[SerializeField] private Slider _progressBar;
	private void Awake() {
		if (Instance == null) {
			Instance = this;
			DontDestroyOnLoad(gameObject);
			_loadingCanvas.SetActive(false);
		} else Destroy(gameObject);
	}

	public void ShowLoading(AsyncOperation op) {
		_loadingCanvas.SetActive(true);
		StartCoroutine(UpdateProgress(op));
	}

	private IEnumerator UpdateProgress(AsyncOperation op) {
		while (!op.isDone) {
			// progress는 0~0.9까지가 로드, 나머지 0.1이 활성화 단계
			float progress = Mathf.Clamp01(op.progress / 0.9f);
			_progressBar.value = progress;
			yield return null;
		}
		yield return new WaitForSeconds(0.5f);
		_loadingCanvas.SetActive(false);
	}
}
