using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class UIManager : MonoBehaviour {
	public static UIManager Instance = null;

	private void Awake() {
		if (Instance == null) Instance = this;
		else Destroy(gameObject);
	}


	/*
	 TODO List

	 [ 인게임 ]
	 * 1. 순위 UI
	 * 2. 시간 UI
	 * 3. Progress UI
	 * 4. Wing 버튼
	 * 5. 아이템 버튼
	 * 6. 조이스틱
	 * 
	 [ 팝업 ]
	 * 1. 게임 종료 시 - 결과 창
	 * 2. 라운드 결과
	 * 3. 다시하기?	 
	 */



}
