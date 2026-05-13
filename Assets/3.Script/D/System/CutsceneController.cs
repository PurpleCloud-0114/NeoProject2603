using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Playables;

public class CutsceneController : MonoBehaviour {
	public static CutsceneController Instance;

	[SerializeField] private PlayableDirector director;

	private void Awake() {
		if (Instance == null) Instance = this;
		else Destroy(gameObject);
	}

	public void PlayIntro() {
		//플레이어 조작 잠금. --> 이미 되어있음.
		//컷신 카메라 활성화.
		director.Play();
	}

	private void OnCutsceneFinished(PlayableDirector obj) {
		//플레이어 조작 해제 및 게임 UI 표시
		Debug.Log("컷신 종료.");
	}
}
