using UnityEngine;
using TMPro;
using Mirror;

public class NickNameSync : NetworkBehaviour
{
    [Header("Sync Variable")]
    [SyncVar(hook = nameof(OnNameChange))]
    public string player_nickname = "";

    [Header("UI Reference")]
    [SerializeField] private TMP_Text _nicknamecard_tmp;
    [SerializeField] private GameObject _nicknamecard_ob;

    private Camera _main_camera;

    public override void OnStartClient()
    {
        base.OnStartClient();
        SetNickName(player_nickname);
    }

    public override void OnStartLocalPlayer()
    {
        base.OnStartLocalPlayer();
        _main_camera = Camera.main;
    }

    void LateUpdate()
    {
        if (_main_camera == null) _main_camera = Camera.main;
        if (_main_camera == null || _nicknamecard_ob == null) return;

        _nicknamecard_ob.transform.rotation = _main_camera.transform.rotation;
    }

    public void SetNickName(string name)
    {
        if (_nicknamecard_tmp != null)
            _nicknamecard_tmp.text = name;
    }

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