using System;
using Pathea.Effect;
using UnityEngine;

namespace Pathea;

[Serializable]
public class Action_GunReload : PEAction
{
	private PEGun m_Gun;

	private int m_TargetAmmoIndex;

	private bool m_ReloadEnd;

	private bool m_AnimEnd;

	private AudioController m_Audio;

	public bool m_IgnoreItem;

	public override PEActionType ActionType => PEActionType.GunReload;

	public PEGun gun
	{
		get
		{
			return m_Gun;
		}
		set
		{
			m_Gun = value;
			if (null == m_Gun)
			{
				base.motionMgr.EndImmediately(ActionType);
			}
		}
	}

	public override bool CanDoAction(PEActionParam para = null)
	{
		if (null != gun && m_IgnoreItem)
		{
			return true;
		}
		PEActionParamN param = PEActionParamN.param;
		int n = param.n;
		if (m_IgnoreItem)
		{
			return null != gun && null != base.packageCmpt && gun.m_AmmoType == AmmoType.Bullet && gun.magazineValue < gun.magazineSize;
		}
		return null != gun && null != base.packageCmpt && gun.m_AmmoType == AmmoType.Bullet && gun.magazineValue < gun.magazineSize && (n != gun.curAmmoItemIndex || base.packageCmpt.GetItemCount(gun.curItemID) > 0);
	}

	public override void DoAction(PEActionParam para = null)
	{
		PEActionParamN param = PEActionParamN.param;
		base.motionMgr.SetMaskState(PEActionMask.GunReload, state: true);
		m_TargetAmmoIndex = param.n;
		if (null != gun && null != base.anim && base.motionMgr.IsActionRunning(PEActionType.GunHold))
		{
			base.anim.SetTrigger(gun.m_ReloadAnim);
			m_Audio = AudioManager.instance.Create(gun.transform.position, gun.m_ReloadSoundID, gun.transform);
			m_ReloadEnd = false;
			m_AnimEnd = false;
		}
		else
		{
			Reload();
		}
		if (null != gun && null != base.ikCmpt && null != base.ikCmpt.m_IKAimCtrl)
		{
			base.ikCmpt.m_IKAimCtrl.StartSyncAimAxie();
		}
	}

	public override bool Update()
	{
		if (null == gun)
		{
			base.motionMgr.SetMaskState(PEActionMask.GunReload, state: false);
			return true;
		}
		if (m_ReloadEnd)
		{
			if (!(null != base.anim))
			{
				return true;
			}
			if (m_AnimEnd)
			{
				base.motionMgr.SetMaskState(PEActionMask.GunReload, state: false);
				if (null != base.ikCmpt && null != base.ikCmpt.m_IKAimCtrl)
				{
					base.ikCmpt.m_IKAimCtrl.EndSyncAimAxie();
				}
				return true;
			}
		}
		if (null != gun && null != base.ikCmpt && null != base.ikCmpt.m_IKAimCtrl)
		{
			base.ikCmpt.m_IKAimCtrl.StartSyncAimAxie();
		}
		return false;
	}

	public override void EndImmediately()
	{
		base.motionMgr.SetMaskState(PEActionMask.GunReload, state: false);
		if (null != base.anim)
		{
			base.anim.SetTrigger("ResetUpbody");
			if (null != gun)
			{
				base.anim.ResetTrigger(gun.m_ReloadAnim);
			}
		}
		if (null != m_Audio)
		{
			m_Audio.Delete();
			m_Audio = null;
		}
		if (null != gun && null != base.ikCmpt && null != base.ikCmpt.m_IKAimCtrl)
		{
			base.ikCmpt.m_IKAimCtrl.EndSyncAimAxie();
		}
	}

	private void Reload()
	{
		if (!(null != gun) || !base.motionMgr.IsActionRunning(ActionType) || !(null != base.entity))
		{
			return;
		}
		m_ReloadEnd = true;
		if (null != gun.m_MagazineObj)
		{
			gun.m_MagazineObj.SetActive(value: false);
		}
		if (gun.m_AmmoItemIDList == null || gun.m_AmmoItemIDList.Length <= m_TargetAmmoIndex || gun.m_AmmoItemIDList.Length <= gun.curAmmoItemIndex)
		{
			gun.magazineValue = gun.magazineSize;
			return;
		}
		int oldProtoId = gun.m_AmmoItemIDList[m_TargetAmmoIndex];
		if (GameConfig.IsMultiMode && !m_IgnoreItem && null != PlayerNetwork.mainPlayer)
		{
			PlayerNetwork.mainPlayer.RequestReload(base.entity.Id, gun.ItemObj.instanceId, oldProtoId, gun.m_AmmoItemIDList[m_TargetAmmoIndex], gun.magazineSize);
		}
		if (!GameConfig.IsMultiMode && gun.magazineValue > 0f && null != base.packageCmpt && !m_IgnoreItem)
		{
			PlayerPackageCmpt playerPackageCmpt = base.packageCmpt as PlayerPackageCmpt;
			if (playerPackageCmpt != null)
			{
				playerPackageCmpt.package.Add(gun.m_AmmoItemIDList[gun.curAmmoItemIndex], Mathf.RoundToInt(gun.magazineValue));
			}
			else
			{
				base.packageCmpt.Add(gun.m_AmmoItemIDList[gun.curAmmoItemIndex], Mathf.RoundToInt(gun.magazineValue));
			}
		}
		gun.curAmmoItemIndex = m_TargetAmmoIndex;
		int num = Mathf.RoundToInt(gun.magazineSize);
		if (!m_IgnoreItem && null != base.packageCmpt)
		{
			num = base.packageCmpt.GetItemCount(gun.m_AmmoItemIDList[m_TargetAmmoIndex]);
		}
		if (num > 0)
		{
			int num2 = Mathf.Min(num, Mathf.RoundToInt(gun.magazineSize));
			gun.magazineValue = num2;
			if (!m_IgnoreItem && num != 0)
			{
				base.packageCmpt.Destory(gun.m_AmmoItemIDList[m_TargetAmmoIndex], num2);
			}
		}
	}

	private void MagazineOff()
	{
		if (null != gun && null != gun.m_MagazinePos && gun.m_MagazineEffectID != 0)
		{
			Singleton<EffectBuilder>.Instance.Register(gun.m_MagazineEffectID, null, gun.m_MagazinePos);
		}
	}

	private void MagazineShow()
	{
		if (null != gun && null != gun.m_MagazineObj)
		{
			gun.m_MagazineObj.SetActive(value: true);
		}
	}

	protected override void OnAnimEvent(string eventParam)
	{
		if (null != gun && base.motionMgr.IsActionRunning(ActionType))
		{
			switch (eventParam)
			{
			case "Reload":
				Reload();
				break;
			case "ReloadEnd":
				m_AnimEnd = true;
				break;
			case "MagazineOff":
				MagazineOff();
				break;
			case "MagazineShow":
				MagazineShow();
				break;
			}
		}
	}
}
