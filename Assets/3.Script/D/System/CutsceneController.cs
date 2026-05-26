using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Playables;
using Mirror;

public class CutsceneController : NetworkBehaviour {
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
		// 서버 본인 환경에서 즉시 실행 (데디 서버 포함)
		ExecuteIntro();

		// 모든 클라이언트들에게도 실행하라고 명령
		RpcPlayIntro();
	}

	[ClientRpc]
	private void RpcPlayIntro() {
		if (isServer) return; // 서버는 위에서 이미 실행했으므로 중복 실행 방지

		ExecuteIntro();
	}

	// 3. 실제 컷신이 실행되는 핵심 로직
	private void ExecuteIntro() {
		_director.Play();
		StartCoroutine("Co_Delay");
	}

	private IEnumerator Co_Delay() {
		while (RoundManager.Instance == null) {
			yield return null;
		}
		UIManager.Instance.PlayTextEffect();
	}

	private void OnCutsceneEnd(PlayableDirector obj) {
		//플레이어 조작 해제 및 게임 UI 표시
		UIManager.Instance.PlayerUISetActive(true);
		//RaceManager작동
		RaceManager.Instance.StartCountdown();
		if (_cutsceneCamera != null) {
			_cutsceneCamera.SetActive(false);
		}
		StopCoroutine("Co_Delay");
	}
}
