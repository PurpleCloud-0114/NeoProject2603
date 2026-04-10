using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Mirror;

public enum Type {
	None,
	Server,
	Client
}

public class Test_ServerChecker : MonoBehaviour {
	private NetworkManager manager;

	public Type connect_type = Type.None;

	private void Awake() {
		TryGetComponent(out manager);
	}

	private void Start() {
		if(connect_type == Type.Server) {
			Start_Server();
		}
		else if (connect_type == Type.Client) {
			Start_Client();
		}
	}

	private void Start_Server() {
		if (Application.platform == RuntimePlatform.WebGLPlayer) {
			Debug.Log("cannot webGL Server");
		} else {
			manager.StartServer();
			Debug.Log($"{manager.networkAddress} : startServer");

			NetworkServer.OnConnectedEvent += (NetworkConnectionToClient) => {
				Debug.Log($"New client connect : {NetworkConnectionToClient.address}");
			};
			NetworkServer.OnDisconnectedEvent += (NetworkConnectionToClient) => {
				Debug.Log($"client disconnect : {NetworkConnectionToClient.address}");
			};
		}
	}

	private void Start_Client() {
		manager.StartClient();
		Debug.Log($"{manager.networkAddress} : startClient");
	}

	private void OnApplicationQuit() {
		//프로그램이 꺼졌을때
		if (NetworkClient.isConnected) {
			//클라이언트 입장
			manager.StopClient();
		}
		if (NetworkServer.active) {
			//서버
			manager.StopServer();
		}
	}
}
