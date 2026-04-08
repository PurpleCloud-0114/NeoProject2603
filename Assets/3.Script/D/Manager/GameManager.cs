using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Mirror;

public class GameManager : NetworkBehaviour {
	//공용 변수
	

	//서버

	//클라이언트

	//----- 메서드
	//서버

	/*
	 TODO List
	 - 플레이어 정보들 받기
	 - 시간 시작

	 - 모두가 로딩이 끝날 경우, 게임 시작.
	 - 모두가 플레이 종료 시, 게임 종료.
	 */

	private void Awake() {
		
	}

	[Server]
	//클라이언트의 도착 신호 (성공 or 실패)
	public void GetArriveResult(bool result) {
		//True : Success
		if (result) {

		}
		//False : Fail
		else {

		}
	}

	//클라이언트

	/*
	 TODO List
	 
	 
	 
	 
	 
	 */
}
