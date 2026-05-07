using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using Unity.Cinemachine;

public class SpectatorController : MonoBehaviour {
	public static SpectatorController Instance;
	public CinemachineCamera vcam;

	private List<Transform> _validTargets = new List<Transform>();
	private int _currentIndex = 0;

	//----- 메서드
	private void Awake() {
		if (Instance == null) Instance = this;
		else Destroy(gameObject);

		gameObject.SetActive(false);
	}

	private void Update() {
		UpdateValidTargets();

		if (vcam.Follow == null || IsFinished(vcam.Follow)) {
			SwitchNext();
		}
	}

	private void UpdateValidTargets() {
		_validTargets = RaceManager.Instance.active_players.Where(t => t != null && t.TryGetComponent(out PlayerCore pc) && pc.player_state != PlayerState.Finish).ToList();
	}

	public void ActivateSpectatorMode() {
		gameObject.SetActive(true);
		UIManager.Instance.ShowUIforSpectator(); 
		SwitchNext();
	}

	private bool IsFinished(Transform t) {
		return t.TryGetComponent(out PlayerCore pc) && pc.player_state == PlayerState.Finish;
	}

	// UI 버튼 (오른쪽) 연결용
	public void SwitchNext() => SwitchTarget(1);

	// UI 버튼 (왼쪽) 연결용
	public void SwitchPrev() => SwitchTarget(-1);

	private void SwitchTarget(int direction) {
		if (_validTargets.Count == 0) {
			vcam.Follow = vcam.LookAt = null;
			return;
		}

		_currentIndex = (_currentIndex + direction + _validTargets.Count) % _validTargets.Count;
		vcam.Follow = vcam.LookAt = _validTargets[_currentIndex];
	}
}
