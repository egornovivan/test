using System;
using UnityEngine;

public class ForcePlayerItem_N : MonoBehaviour
{
	public UILabel RoleNameLabel;

	public UILabel PlayerNameLabel;

	public UILabel DelayLabel;

	public UILabel StateLabel;

	public UILabel CanJoinCountLabel;

	public UIButton ExitBtn;

	public UISprite RoomMasterMarkSprite;

	public UISprite SelecteSprite;

	public Action<int> ChangePlayerForceEvent;

	public Action<int> KickPlayerEvent;

	private RoomPlayerInfo m_PlayerInfo;

	private PlayerDesc m_RoleInfo;

	private bool m_IsRoomMaster;

	private void OnEnable()
	{
		UIEventListener uIEventListener = UIEventListener.Get(ExitBtn.gameObject);
		uIEventListener.onClick = (UIEventListener.VoidDelegate)Delegate.Combine(uIEventListener.onClick, new UIEventListener.VoidDelegate(OnKickPlayerClick));
		UIEventListener uIEventListener2 = UIEventListener.Get(base.gameObject);
		uIEventListener2.onClick = (UIEventListener.VoidDelegate)Delegate.Combine(uIEventListener2.onClick, new UIEventListener.VoidDelegate(OnJoinClick));
	}

	private void OnDisable()
	{
		UIEventListener uIEventListener = UIEventListener.Get(ExitBtn.gameObject);
		uIEventListener.onClick = (UIEventListener.VoidDelegate)Delegate.Remove(uIEventListener.onClick, new UIEventListener.VoidDelegate(OnKickPlayerClick));
		UIEventListener uIEventListener2 = UIEventListener.Get(base.gameObject);
		uIEventListener2.onClick = (UIEventListener.VoidDelegate)Delegate.Remove(uIEventListener2.onClick, new UIEventListener.VoidDelegate(OnJoinClick));
	}

	public int GetPlayerID()
	{
		if (m_PlayerInfo == null)
		{
			return -1;
		}
		return m_PlayerInfo.mId;
	}

	public int GetRoleID()
	{
		if (m_RoleInfo == null)
		{
			return -1;
		}
		return m_RoleInfo.ID;
	}

	public void SetDelay(int delay)
	{
		UpdateDelay(delay);
	}

	public void SetState(int state)
	{
		UpdateState(state);
	}

	public void SetPlayer(RoomPlayerInfo playerinfo, bool isRoomMaster)
	{
		ResetPlayer();
		m_PlayerInfo = playerinfo;
		m_IsRoomMaster = isRoomMaster;
		if (m_PlayerInfo != null)
		{
			UpdatePlayerName(m_PlayerInfo.mPlayerInfo.mName);
			UpdateDelay(m_PlayerInfo.mDelay);
			UpdateState(m_PlayerInfo.mState);
			UpdateRoomMasterMark(m_PlayerInfo.mRoomMaster);
			UpdateExitBtnState(isRoomMaster && m_PlayerInfo.mId != BaseNetwork.MainPlayer.Id);
			UpdateSelect(playerinfo.mId == BaseNetwork.MainPlayer.Id);
		}
	}

	public void SetFixRole(PlayerDesc roleInfo)
	{
		ResetAll();
		m_RoleInfo = roleInfo;
		UpdateRoleName((m_RoleInfo != null) ? m_RoleInfo.Name : string.Empty);
	}

	public void ResetAll()
	{
		RoleNameLabel.text = string.Empty;
		m_RoleInfo = null;
		ResetPlayer();
	}

	public void ResetPlayer()
	{
		PlayerNameLabel.text = string.Empty;
		DelayLabel.text = string.Empty;
		StateLabel.text = string.Empty;
		UpdateExitBtnState(isEnable: false);
		RoomMasterMarkSprite.enabled = false;
		m_PlayerInfo = null;
		CanJoinCountLabel.text = string.Empty;
		CanJoinCountLabel.gameObject.SetActive(value: false);
		UpdateSelect(isSelecte: false);
	}

	public void UpdateCanJoinCount(int currentCount, int totalCount)
	{
		if (!CanJoinCountLabel.gameObject.activeInHierarchy)
		{
			CanJoinCountLabel.gameObject.SetActive(value: true);
		}
		CanJoinCountLabel.text = string.Format(PELocalization.GetString(8000184), (totalCount - currentCount).ToString());
	}

	private void UpdateRoleName(string roleName)
	{
		if (roleName != null)
		{
			RoleNameLabel.text = roleName;
		}
	}

	private void UpdatePlayerName(string playerName)
	{
		if (playerName != null)
		{
			PlayerNameLabel.text = playerName;
		}
	}

	private void UpdateDelay(int delay)
	{
		Color white = Color.white;
		white = ((delay < 151) ? new Color(2f / 51f, 41f / 51f, 14f / 85f) : ((delay >= 251) ? Color.red : new Color(1f, 73f / 85f, 36f / 85f)));
		DelayLabel.color = white;
		DelayLabel.text = delay.ToString();
	}

	private void UpdateState(int state)
	{
		switch (state)
		{
		case 3:
			StateLabel.text = "Gaming";
			break;
		case 1:
			StateLabel.text = "Loading";
			break;
		case 0:
			StateLabel.text = "Prepare";
			break;
		default:
			StateLabel.text = "Ready";
			break;
		}
	}

	private void UpdateExitBtnState(bool isEnable)
	{
		ExitBtn.enabled = isEnable;
		ExitBtn.gameObject.SetActive(isEnable);
	}

	private void UpdateRoomMasterMark(bool isRoomMaster)
	{
		RoomMasterMarkSprite.enabled = isRoomMaster;
	}

	private void UpdateSelect(bool isSelecte)
	{
		SelecteSprite.gameObject.SetActive(isSelecte);
	}

	private void OnKickPlayerClick(GameObject go)
	{
		if (Input.GetMouseButtonUp(0) && KickPlayerEvent != null && m_PlayerInfo != null && m_IsRoomMaster && m_PlayerInfo.mId != BaseNetwork.MainPlayer.Id)
		{
			KickPlayerEvent(m_PlayerInfo.mId);
		}
	}

	private void OnJoinClick(GameObject go)
	{
		if (Input.GetMouseButtonUp(0) && ChangePlayerForceEvent != null && m_PlayerInfo == null)
		{
			ChangePlayerForceEvent(GetRoleID());
		}
	}
}
