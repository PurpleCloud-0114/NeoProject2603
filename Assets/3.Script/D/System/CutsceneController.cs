using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Playables;
using Unity.Cinemachine;

public class CutsceneController : MonoBehaviour {
	public static CutsceneController Instance = null;

	[SerializeField] private PlayableDirector _director;
	[SerializeField] private GameObject _cutsceneCamera;

	private void Awake() {
		if (Instance == null) Instance = this;
		else Destroy(gameObject);
	}

	private void Start() {
		UIManager.Instance.PlayerUISetActive(false);
		_cutsceneCamera.SetActive(true);
	}

	private void OnEnable() {
		_director.stopped += OnCutsceneEnd;
	}

	private void OnDisable() {
		_director.stopped -= OnCutsceneEnd;
	}

	public void PlayIntro() {
		//플레이어 조작 잠금. --> 이미 되어있음.
		//컷신 카메라 활성화.
		_director.Play();
	}

	private void OnCutsceneEnd(PlayableDirector obj) {
		//플레이어 조작 해제 및 게임 UI 표시
		UIManager.Instance.PlayerUISetActive(true);
		//RaceManager작동
		RaceManager.Instance.StartCountdown();
		if (_cutsceneCamera != null) {
			_cutsceneCamera.SetActive(false);
		}
	}
}
