using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using UnityEngine.UI;
using Unity.Cinemachine;
using Mirror;
using TMPro;

public class SpectatorController : MonoBehaviour {
	public static SpectatorController Instance;
	public CinemachineCamera vcam;

	private List<Transform> _validTargets = new List<Transform>();
	private int _currentIndex = 0;

	[SerializeField] private TextMeshProUGUI _specNicknameUI;

	private NetworkRoomManager networkRoommManager;

	//----- 메서드
	private void Awake() {
		if (Instance == null) Instance = this;
		else Destroy(gameObject);

		gameObject.SetActive(false);
	}

	private void Start() {
		networkRoommManager = NetworkManager.singleton as NetworkRoomManager;
	}

	private void Update() {
		if (vcam.Follow == null || IsFinished(vcam.Follow)) {
			SwitchNext();
		}
	}

	private void UpdateValidTargets() {
		_validTargets.Clear();

		// [구조 개선] RaceManager의 수동 리스트 대신 Mirror가 클라이언트에 스폰한 객체 풀을 직접 조회.
		// 중복 데이터 관리가 사라지고 데이터 동기화 누락 문제가 원천 차단됩니다.
		foreach (NetworkIdentity identity in NetworkClient.spawned.Values) {
			if (identity.TryGetComponent(out PlayerCore pc) && pc.player_state != PlayerState.Finish) {
				_validTargets.Add(identity.transform);
			}
		}
	}

	public void ActivateSpectatorMode() {
		gameObject.SetActive(true);
		UIManager.Instance.ShowUIforSpectator(); 
		SwitchNext();
	}

	private bool IsFinished(Transform t) {
		// 대상이 파괴되었거나(null), Finish 상태라면 true 반환
		return t == null || (t.TryGetComponent(out PlayerCore pc) && pc.player_state == PlayerState.Finish);
	}


	// UI 버튼 (오른쪽) 연결용
	public void SwitchNext() => SwitchTarget(1);

	// UI 버튼 (왼쪽) 연결용
	public void SwitchPrev() => SwitchTarget(-1);

	private void SwitchTarget(int direction) {
		UpdateValidTargets();

		if (_validTargets.Count == 0) {
			vcam.Follow = vcam.LookAt = null;
			return;
		}

		_currentIndex = (_currentIndex + direction + _validTargets.Count) % _validTargets.Count;
		vcam.Follow = vcam.LookAt = _validTargets[_currentIndex];

		if (_validTargets[_currentIndex].TryGetComponent(out PlayerDataSync playerData)) {
			_specNicknameUI.text = playerData.SyncNickname;
		}
	}
}
