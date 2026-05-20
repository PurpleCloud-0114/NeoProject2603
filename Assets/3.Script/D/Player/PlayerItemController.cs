using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Mirror;

public class PlayerItemController : NetworkBehaviour
{
    private PlayerCore _playerCore;

    public IUseable current_item = null;

    [Header("3D Audio")]
    [SerializeField] private AudioSource _player3DAudioSource;

    [Header("Player Attached Particles")]
    [SerializeField] private ParticleSystem item_WeightAcceleration;
    [SerializeField] private ParticleSystem[] item_Shockwave;
    [SerializeField] private ParticleSystem item_Magnetic;
    [SerializeField] private ParticleSystem item_Spiderweb;

    private void Awake()
    {
        TryGetComponent(out _playerCore);

        if (_player3DAudioSource == null)
        {
            _player3DAudioSource = GetComponentInChildren<AudioSource>(true);
        }

        //Debug.Log($"[{name}] AUDIO SOURCE FOUND : " +
        //          $"{(_player3DAudioSource != null ? _player3DAudioSource.name : "NULL")}");

        if (_player3DAudioSource != null)
        {
			//Debug.Log(
			//	$"[{name}] AUDIO INFO | " +
			//	$"enabled={_player3DAudioSource.enabled} | " +
			//	$"activeInHierarchy={_player3DAudioSource.gameObject.activeInHierarchy} | " +
			//	$"volume={_player3DAudioSource.volume} | " +
			//	$"mute={_player3DAudioSource.mute} | " +
			//	$"spatialBlend={_player3DAudioSource.spatialBlend}"
			//);
		}
    }

    private void OnEnable()
    {
        _playerCore.on_item_acquired += GetItem;
        _playerCore.on_item_button_clicked += UseItem;
    }

    private void OnDisable()
    {
        _playerCore.on_item_acquired -= GetItem;
        _playerCore.on_item_button_clicked -= UseItem;
    }

    private void GetItem(IUseable newItem)
    {
        current_item = newItem;
    }

    public void UseItem()
    {
        if (current_item != null)
        {
            string sfxName = GetItemActivationSFX(current_item.Type);

            //Debug.Log($"[{name}] USE ITEM : {current_item.Type} / SFX : {sfxName}");

            // 서버 요청
            CmdUseItem(current_item.Type, sfxName);
        }

        current_item = null;
    }

    [Command]
    private void CmdUseItem(ItemType itemType, string sfxName)
    {
        //Debug.Log($"[SERVER] CmdUseItem : {itemType} / {sfxName}");

        IUseable itemToUse = ItemManager.Instance.GetItemUseable(itemType);

        if (itemToUse != null)
        {
            //Debug.Log($"[SERVER] ITEM USE SUCCESS");

            itemToUse.Use(gameObject);
        }
        else
        {
            //Debug.LogError($"[SERVER] ITEM USEABLE NULL");
        }

        // 모든 클라이언트 재생
        RpcPlayItemEffect(itemType,sfxName);
    }

    [ClientRpc]
    private void RpcPlayItemEffect(ItemType itemType, string sfxName)
    {
        //Debug.Log($"[CLIENT RPC] ITEM RPC : {itemType} / {gameObject.name}");

        Play3DSFXLocal(sfxName);

        PlayItemParticle(itemType);
    }

    // ---------------- NEW: PARTICLE LOGIC ----------------

    private void PlayItemParticle(ItemType type)
    {
        if (type == ItemType.Shockwave)
        {
            if (item_Shockwave != null)
            {
                foreach (var particle in item_Shockwave)
                {
                    if (particle != null)
                    {
                        particle.Stop(true, ParticleSystemStopBehavior.StopEmittingAndClear);
                        particle.Play();
                    }
                }
            }
            return; // 쇼크웨이브 처리가 끝났으므로 리턴
        }

        // 그 외 기존 싱글 파티클 아이템들 처리
        ParticleSystem targetParticle = type switch
        {
            ItemType.WeightAcceleration => item_WeightAcceleration,
            ItemType.Magnetic => item_Magnetic,
            ItemType.Spiderweb => item_Spiderweb,
            _ => null
        };

        if (targetParticle != null)
        {
            targetParticle.Stop(true, ParticleSystemStopBehavior.StopEmittingAndClear);
            targetParticle.Play();
        }
    }

    // ---------------- LOCAL 3D SOUND ----------------

    private void Play3DSFXLocal(string sfxName)
    {
        //Debug.Log($"[{name}] TRY PLAY SFX : {sfxName}");

        if (string.IsNullOrEmpty(sfxName))
        {
            //Debug.LogWarning($"[{name}] SFX NAME EMPTY");
            return;
        }

        if (_player3DAudioSource == null)
        {
            //Debug.LogError($"[{name}] AudioSource NULL");
            return;
        }

        //Debug.Log(
        //    $"[{name}] SOURCE INFO | " +
        //    $"source={_player3DAudioSource.name} | " +
        //    $"enabled={_player3DAudioSource.enabled} | " +
        //    $"active={_player3DAudioSource.gameObject.activeInHierarchy} | " +
        //    $"volume={_player3DAudioSource.volume} | " +
        //    $"mute={_player3DAudioSource.mute}"
        //);

        if (AudioManager.Instance == null)
        {
            //Debug.LogError($"[{name}] AudioManager NULL");
            return;
        }

        AudioClip clip = AudioManager.Instance.GetSFXClip(sfxName);

        if (clip == null)
        {
            //Debug.LogError($"[{name}] SFX NOT FOUND : {sfxName}");
            return;
        }

        //Debug.Log(
        //    $"[{name}] CLIP FOUND | " +
        //    $"clip={clip.name} | " +
        //    $"length={clip.length}"
        //);

        _player3DAudioSource.PlayOneShot(clip);

        //Debug.Log($"[{name}] PlayOneShot CALLED");
    }

    // ---------------- SFX NAME ----------------

    private string GetItemActivationSFX(ItemType type)
    {
        return type switch
        {
            ItemType.WeightAcceleration => "ItemWeightActivate",
            ItemType.Shockwave => "ItemShockwaveActivate",
            ItemType.Magnetic => "ItemMagneticActivate",
            ItemType.Spiderweb => "Jump",
            _ => ""
        };
    }
}