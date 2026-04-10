using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using TMPro;
using Mirror;

public class TimerUI : MonoBehaviour {
	[SerializeField] private TextMeshProUGUI _timerText;

	private void Update() {
		if (RaceManager.Instance.current_state_sync != RaceState.Racing) return;
		//나중에 조건으로 플레이어가 EndPoint에 도달했을 경우 시간 정지.

		double elapsedTime = NetworkTime.time - RaceManager.Instance.race_start_time_sync;

		TimeSpan time = TimeSpan.FromSeconds(elapsedTime);
		_timerText.text = string.Format($"{time.Minutes:00}:{time.Seconds:00}:{time.Milliseconds/10:00}");
	}
}