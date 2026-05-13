using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Mirror;


public enum PlayerState {
	Wait,
	Falling,
	Finish
}

public enum StatusEffect {
	None,
	Stun,
	Invinsible
}

public class PlayerCore : NetworkBehaviour {
	private ClientPlayer _clientPlayer;
	private GameObject _mainCamera;
	public Animator animator;
	[SerializeField] private GameObject _portalPrefabs;

	public PlayerState player_state = PlayerState.Wait;
	public StatusEffect status_effect = StatusEffect.None;

	public Action<PlayerState> on_player_state_change_requested;
	public Action<StatusEffect> on_state_effect_change_requested;
	public Action<IUseable> on_item_acquired;
	public Action on_race_start;
	public Action on_race_finish;
	public Action on_redzone_entered;
	public Action on_endpoint_landed;
	public Action on_wing_button_clicked;
	public Action on_item_button_clicked;
	public Action<Collider> on_spiderweb_hit;
	public Action on_obstacle_hit;

	public Action<float, float, float, StatusEffect> on_max_drop_speed_change_requested;  //속도, 시간
	public Action<Vector3> on_impulse_requested;
	public Action<Vector3> on_continuous_force_requested;
	public Action<float> on_stun_requested;

	public float player_number = 0;

	//임시
	public bool is_dummy = false;

	//----- 메서드
	private void Awake() {
		if (TryGetComponent(out _clientPlayer)) {
			player_number = _clientPlayer.index;
		}
	}

	private void OnEnable() { 
		on_player_state_change_requested += ChangePlayerState;
		on_state_effect_change_requested += ChangeStatusEffect;
	}
	private void OnDisable() { 
		on_player_state_change_requested -= ChangePlayerState;
		on_state_effect_change_requested -= ChangeStatusEffect;
	}

	private void ChangePlayerState(PlayerState newState) { 
		player_state = newState;
		switch (newState) {
			case PlayerState.Falling:
				on_race_start?.Invoke();
				break;
			case PlayerState.Finish:
				on_race_finish?.Invoke();
				break;
		}
	}
	private void ChangeStatusEffect(StatusEffect newState) { status_effect = newState; }

	//----- 네트워킹

	public override void OnStartLocalPlayer() {
		//int totalPlayer = RaceManager.Instance.START_MAX_PLAYER;
		int totalPlayer = 10;
		player_number = UnityEngine.Random.Range(0, totalPlayer);
		float angle = player_number * Mathf.PI * 2f / totalPlayer;
		float x = Mathf.Cos(angle) * 5f;
		float z = Mathf.Sin(angle) * 5f;
		Vector3 spawnCenter = StageManager.Instance.map_size.map_center + Vector3.up * StageManager.Instance.stage_data_sync.map_height;
		//Vector3 spawnCenter = StageManager.Instance.map_size.map_center + Vector3.up * StageManager.Instance.stage_data_sync.map_height;
		Vector3 spawnPosition = spawnCenter + new Vector3(x, 0f, z);
		transform.position = spawnPosition;
		if ((isLocalPlayer || RaceManager.Instance.isSinglePlay) && !is_dummy) {
			_mainCamera = GameObject.FindGameObjectWithTag("MainCamera");
			if (_mainCamera.TryGetComponent(out DynamicFOVController FOVController)) FOVController.BindPlayer(gameObject);
		}
		if (TryGetComponent(out Rigidbody rigid)) {
			rigid.interpolation = RigidbodyInterpolation.Interpolate;
		}
		RaceManager.Instance.CmdReportReady();
	}

	// 서버에서 객체가 생성될 때 호출되는 메서드 추가
	public override void OnStartServer() {
		base.OnStartServer();

		// 서버가 직접 RaceManager의 리스트에 이 트랜스폼을 등록합니다.
		if (RaceManager.Instance != null) {
			RaceManager.Instance.RegisterPlayer(this.transform);
		}
	}

	// 종료 시에도 서버에서 안전하게 제거
	public override void OnStopServer() {
		base.OnStopServer();
		if (RaceManager.Instance != null) {
			RaceManager.Instance.UnregisterPlayer(this.transform);
		}
	}

	//클라 -> 서버에게 아이템 요청
	[Command]
	public void CmdRequestItemSelection() {
		// ItemManager에게 '이 유저(gameObject)'의 순위를 기반으로 아이템을 달라고 함
		IUseable newItem = ItemManager.Instance.RandomItem(this.gameObject);

		if (newItem != null) {
			// 결정된 아이템 타입을 해당 클라이언트에게만 알려줌
			TargetAcquireItem(newItem.Type);
		}
	}
	//서버 -> 클라 아이템 정보 주기.
	[TargetRpc]
	private void TargetAcquireItem(ItemType type) {
		// 클라이언트 측 ItemManager에서 실제 아이템 객체를 생성
		IUseable item = ItemManager.Instance.GetItemUseable(type);

		// 기존 아이템 획득 이벤트 호출 (UI 업데이트 등 실행)
		on_item_acquired?.Invoke(item);
	}

	//순위 업데이트.
	[TargetRpc]
	public void TargetUpdateRank(NetworkConnectionToClient target, int newRank) {
		if (UIManager.Instance != null) {
			UIManager.Instance.UpdateMyRank(newRank);
		}
	}

	[Command]
	//서버에게 보내는 Finish 신호. (도착 속도 / 시간,
	public void SendArriveResult(float impactSpeed, double finishTime) {
		RaceManager.Instance.GetArriveResult(connectionToClient, impactSpeed, finishTime);
	}

	[Command]
	//서버에게 보내는 EndPoint 신호.
	public void SendEndpoint() {
		RaceManager.Instance.EndRaceCheck();
	}
	
	public void SpawnPortal() {
		Vector3 spawnPos = new Vector3(transform.position.x, -37f, transform.position.z);
		Quaternion rotation = Quaternion.Euler(-90, 0, 0);
		Instantiate(_portalPrefabs, spawnPos, rotation);
	}
}
