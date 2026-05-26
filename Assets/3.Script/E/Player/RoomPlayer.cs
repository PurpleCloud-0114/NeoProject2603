using System.Collections;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.SceneManagement;
using Mirror;

public class RoomPlayer : NetworkRoomPlayer
{
    [SyncVar]
    public string NicknameSync = "";
    public GameObject btn;
    public override void OnStartLocalPlayer()
    {
        base.OnStartLocalPlayer();
        string saved_nickname = PlayerPrefs.GetString("PlayerNickname", "NoName");
        CmdSetNickname(saved_nickname);
        RegisterReadyButton();
        SceneManager.sceneLoaded += OnSceneLoaded;
    }
    private void OnSceneLoaded(Scene scene, LoadSceneMode mode)
    {
        var roomManager = NetworkManager.singleton as NetworkRoomManager;
        if (roomManager == null) return;

        if (scene.name == roomManager.RoomScene)
        {
            Debug.Log("[RoomPlayer] RoomScene 재로드 감지 → 버튼 재등록");
            StartCoroutine(Co_RegisterReadyButton_co());
        }
    }
    private IEnumerator Co_RegisterReadyButton_co()
    {
        // 씬 오브젝트 Awake()/OnEnable() 완료 보장 대기 // <<---
        yield return new WaitForEndOfFrame();

        GameObject btn = GameObject.Find("ReadyButton");

        // WaitForEndOfFrame 이후에도 못 찾으면 1프레임 추가 대기 // <<---
        if (btn == null)
        {
            yield return null;
            btn = GameObject.Find("ReadyButton");
        }

        if (btn == null)
        {
            Debug.LogWarning("[RoomPlayer] ReadyButton을 찾을 수 없음");
            yield break;
        }

        var button = btn.GetComponent<UnityEngine.UI.Button>();
        if (button == null) yield break;

        button.onClick.RemoveListener(OnClickReady); // <<--- 중복 방지
        button.onClick.AddListener(OnClickReady);
        Debug.Log("[RoomPlayer] ReadyButton 리스너 등록 완료");
    }

    private void RegisterReadyButton()
    {
        btn = GameObject.Find("ReadyButton");
        if (btn == null)
        {
            Debug.LogWarning("[RoomPlayer] ReadyButton을 찾을 수 없음");
            return;
        }
        var button = btn.GetComponent<Button>();
        if (button == null) return;

        button.onClick.RemoveListener(OnClickReady);
        button.onClick.AddListener(OnClickReady);
        Debug.Log("[RoomPlayer] ReadyButton 리스너 등록 완료");
    }
    private void OnDestroy()
    {
        SceneManager.sceneLoaded -= OnSceneLoaded;
    }
    [Command]
    private void CmdSetNickname(string nickname)
    {
        NicknameSync = nickname;
        if(NetworkManager.singleton is RoomManagement rm)
        {
            rm.RefreshLobbyUI();
        }
    }
    public override void OnStartClient()
    {
        base.OnStartClient();
        StartCoroutine(Co_RequestUIRefresh_co());
    }
    private IEnumerator Co_RequestUIRefresh_co()
    {
        yield return new WaitForEndOfFrame();
        CmdRequestUIRefresh();
    }
    [Command]
    void CmdRequestUIRefresh()
    {
        if (NetworkManager.singleton is RoomManagement rm)
        {
            rm.RefreshLobbyUI();
        }
    }

    public void OnClickReady()
    {
        if (!isLocalPlayer) return;
        CmdChangeReadyState(!readyToBegin);
    }
    [ClientRpc]
    public void RpcUpdateUI(int slot_index, string player_name, bool _isReady)
    {
        if (LobbyTextUI.Instance == null)
        {
            Debug.LogWarning("[RoomPlayer] RpcUpdateUI: LobbyTextUI.Instance가 null");
            return; 
        }
        Debug.Log($"[RoomPlayer] RpcUpdateUI 실행: index={slot_index}, name={player_name}, isReady={_isReady}");
        LobbyTextUI.Instance.UpdateUI(slot_index, player_name, _isReady);
    }
    [ClientRpc]
    public void RpcSetCountdown(int time)
    {
        LobbyTextUI.Instance?.ui?.SetTime(time);
    }
    [ClientRpc]
    public void RpcCancelCountdown()
    {
        LobbyTextUI.Instance?.ui?.Hide();
    }
    [ClientRpc]
    public void RpcSimulateReadyReset()
    {
        if (!isLocalPlayer) return;
        StartCoroutine(Co_SimulateReadyReset_co());
    }
    private IEnumerator Co_SimulateReadyReset_co()
    {
        CmdChangeReadyState(true);
        yield return new WaitForEndOfFrame();
        CmdChangeReadyState(false);
    }
}