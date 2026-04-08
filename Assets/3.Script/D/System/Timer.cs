using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using System.Diagnostics;

public class Timer : MonoBehaviour {
	public static Timer Instance = null;

	private Stopwatch _stopwatch = new Stopwatch();

	[SerializeField] private GameObject _curTimer;
	[SerializeField] private Text _curTimerText;
	[SerializeField] private Text _endTimerText;

	private bool _isTimering = false;

	public void Awake() {
		if (Instance == null) Instance = this;
		else Destroy(gameObject);
	}

	public void StartStopwatch() {
		_curTimer.SetActive(true);
		_stopwatch.Restart();
		//_stopwatch.Start();
		_isTimering = true;
	}

	private void Update() {
		if(_isTimering) {
			_curTimerText.text = _stopwatch.Elapsed.ToString(@"mm\:ss\.ff");
		}
	}

	public void EndStopwatch() {
		_curTimer.SetActive(false);
		_stopwatch.Stop();
		_isTimering = false;
		_endTimerText.text = _stopwatch.Elapsed.ToString(@"mm\:ss\.ff");
	}
}
