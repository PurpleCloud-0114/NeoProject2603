using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Playables;

public class CutsceneManager : MonoBehaviour {
	public static CutsceneManager Instance = null;

	[Header("ÄÄÆ÷³ÍÆ®")]
	[SerializeField] private PlayableDirector _director;
	[SerializeField] private PlayerMovement _playerMovement;

	private void Awake() {
		if (Instance == null) Instance = this;
		else Destroy(gameObject);
	}

	private void Start() {
		_director.stopped += OnCutsceneFinished;
	}

	public void PlayStartCutscene() {
		Debug.Log("ÄÆ½Å ½ÃÀÛ");
		_playerMovement.CutsceneTest(true);
		_director.Play();
	}

	private void OnCutsceneFinished(PlayableDirector driector) {
		Debug.Log("ÄÆ½Å Á¾·á");
		Timer.Instance.StartStopwatch();
	}

	private void OnDestroy() {
		if (_director != null) _director.stopped -= OnCutsceneFinished;
	}
}
