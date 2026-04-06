using UnityEngine;
using TMPro;
using Mirror;

public class NickNameSync : NetworkBehaviour
{
    [SyncVar(hook = "OnNameChange")]
    public string player_nickname = "Empty";

    //캐릭터 프리팹 내부에 Canvas를 생성이후
    //Canvas 컴포넌트의 Render Mode를 World Space로 변경하여 사용(닉네임 카드가 플레이어 따라다니게 됨)
    
    [SerializeField] private TMP_Text _nicknamecard_tmp;
    [SerializeField] private GameObject _nicknameard_ob;
    public override void OnStartLocalPlayer()
    {
        base.OnStartLocalPlayer();
        string nickname = SQLManager.Instance.user_info.user_nickname;
        CmdSendNameToServer(nickname);
    }
    void Update()
    {
        if (Camera.main == null) return;
        _nicknameard_ob.transform.LookAt(Camera.main.transform);
    }
    public void SetNickName(string name)
    {
        _nicknamecard_tmp.text = name;
    }
    //-------Server한테 닉네임 보고
    [Command]
    public void CmdSendNameToServer(string name)
    {
        player_nickname = name;
    }

    public void OnNameChange(string oldname, string newname)
    {
        SetNickName(newname);
    }
}
