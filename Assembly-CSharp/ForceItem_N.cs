using System;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

public class ForceItem_N : MonoBehaviour
{
	public UILabel ForceNameLabel;

	public UISprite ForceColorSprite;

	public UILabel ForceCountLabel;

	public UIGrid ForceGrid;

	public GameObject ForcePlayerItemPrefab;

	public Action RepositionEvent;

	public Action<int, int> ChangePlayerForceEvent;

	public Action<int> KickPlayerEvent;

	private ForceDesc m_ForceDesc;

	private List<ForcePlayerItem_N> m_CurForcePlayerItems;

	private Queue<ForcePlayerItem_N> m_ForcePlayerItemPool;

	private bool m_IsRoomMaster;

	private int m_FixRoleCount;

	private void Awake()
	{
		m_CurForcePlayerItems = new List<ForcePlayerItem_N>();
		m_ForcePlayerItemPool = new Queue<ForcePlayerItem_N>();
		m_FixRoleCount = 0;
	}

	public void SetForceInfo(ForceDesc forceInfo, List<PlayerDesc> roleInfos)
	{
		m_ForceDesc = forceInfo;
		InitRoleInfo(roleInfos);
		UpdateForceName(m_ForceDesc.Name);
		UpdateForceColor(m_ForceDesc.Color);
		UpdateForcePlayerCount();
	}

	public void JoinForceByNet(RoomPlayerInfo playerInfo, bool isRoomMaster)
	{
		m_IsRoomMaster = isRoomMaster;
		AddForcePlayerItem(playerInfo);
	}

	public void ExitForceByNet(int playerId)
	{
		RemoveForcePlayerItem(playerId);
	}

	public int GetForceID()
	{
		if (m_ForceDesc == null)
		{
			return -1;
		}
		return m_ForceDesc.ID;
	}

	public void ChangePlayerDelay(int playerID, int delay)
	{
		ForcePlayerItem_N forcePlayerItemByPlayerID = GetForcePlayerItemByPlayerID(playerID);
		if (forcePlayerItemByPlayerID != null)
		{
			forcePlayerItemByPlayerID.SetDelay(delay);
		}
	}

	public void ChangePlayerState(int playerID, int state)
	{
		ForcePlayerItem_N forcePlayerItemByPlayerID = GetForcePlayerItemByPlayerID(playerID);
		if (forcePlayerItemByPlayerID != null)
		{
			forcePlayerItemByPlayerID.SetState(state);
		}
	}

	public ForcePlayerItem_N GetForcePlayerItemByPlayerID(int playerID)
	{
		return m_CurForcePlayerItems.FirstOrDefault((ForcePlayerItem_N a) => a.GetPlayerID() == playerID);
	}

	public bool CheckFixRoleIsFull()
	{
		return !m_CurForcePlayerItems.Any((ForcePlayerItem_N a) => a.GetRoleID() != -1 && a.GetPlayerID() == -1);
	}

	private void ChangeForceToNet(int roleID)
	{
		if (ChangePlayerForceEvent != null)
		{
			ChangePlayerForceEvent(GetForceID(), roleID);
		}
	}

	private void KickPlayerToNet(int playerID)
	{
		if (KickPlayerEvent != null)
		{
			KickPlayerEvent(playerID);
		}
	}

	private void InitRoleInfo(List<PlayerDesc> roleInfos)
	{
		if (roleInfos != null && roleInfos.Count > 0)
		{
			m_FixRoleCount = roleInfos.Count;
			for (int i = 0; i < roleInfos.Count; i++)
			{
				ForcePlayerItem_N newForcePlayerItem = GetNewForcePlayerItem();
				newForcePlayerItem.SetFixRole(roleInfos[i]);
			}
		}
		TryAddNullItem();
		RepostionForce();
	}

	private ForcePlayerItem_N GetNewForcePlayerItem()
	{
		ForcePlayerItem_N forcePlayerItem_N = null;
		if (m_ForcePlayerItemPool.Count() > 0)
		{
			forcePlayerItem_N = m_ForcePlayerItemPool.Dequeue();
			forcePlayerItem_N.gameObject.SetActive(value: true);
		}
		else
		{
			GameObject gameObject = UnityEngine.Object.Instantiate(ForcePlayerItemPrefab);
			gameObject.transform.parent = ForceGrid.transform;
			gameObject.transform.localPosition = Vector3.zero;
			gameObject.transform.localScale = Vector3.one;
			forcePlayerItem_N = gameObject.GetComponent<ForcePlayerItem_N>();
		}
		forcePlayerItem_N.gameObject.name = GetNewItemName();
		forcePlayerItem_N.KickPlayerEvent = KickPlayerToNet;
		forcePlayerItem_N.ChangePlayerForceEvent = ChangeForceToNet;
		m_CurForcePlayerItems.Add(forcePlayerItem_N);
		return forcePlayerItem_N;
	}

	private void AddForcePlayerItem(RoomPlayerInfo playerInfo)
	{
		ForcePlayerItem_N forcePlayerItem_N = null;
		if (m_CurForcePlayerItems != null)
		{
			forcePlayerItem_N = m_CurForcePlayerItems.FirstOrDefault((ForcePlayerItem_N a) => a.GetRoleID() == playerInfo.mRoleID && a.GetPlayerID() == -1);
		}
		if (null == forcePlayerItem_N)
		{
			forcePlayerItem_N = GetNewForcePlayerItem();
		}
		forcePlayerItem_N.SetPlayer(playerInfo, m_IsRoomMaster);
		TryAddNullItem();
		UpdateForcePlayerCount();
		RepostionForce();
	}

	private string GetNewItemName()
	{
		return "ForcePlayerItem" + m_CurForcePlayerItems.Count().ToString("D4");
	}

	private void TryAddNullItem()
	{
		if (null == GetNullJoinableItem() && GetCurJoinableCount() < m_ForceDesc.JoinablePlayerCount)
		{
			ForcePlayerItem_N newForcePlayerItem = GetNewForcePlayerItem();
			newForcePlayerItem.SetFixRole(null);
			newForcePlayerItem.UpdateCanJoinCount(GetCurJoinableCount(), m_ForceDesc.JoinablePlayerCount);
		}
	}

	private int GetCurJoinableCount()
	{
		return m_CurForcePlayerItems.Count((ForcePlayerItem_N a) => a.GetRoleID() == -1 && a.GetPlayerID() != -1);
	}

	private int GetCurPlayerCount()
	{
		return m_CurForcePlayerItems.Where((ForcePlayerItem_N a) => a.GetPlayerID() != -1).Count();
	}

	private int GetCanJoinPlayerCount()
	{
		return m_FixRoleCount + m_ForceDesc.JoinablePlayerCount;
	}

	private void RemoveForcePlayerItem(int playerID)
	{
		ForcePlayerItem_N forcePlayerItem_N = null;
		if (m_CurForcePlayerItems != null)
		{
			forcePlayerItem_N = m_CurForcePlayerItems.FirstOrDefault((ForcePlayerItem_N a) => a.GetPlayerID() == playerID);
		}
		if (forcePlayerItem_N != null)
		{
			if (forcePlayerItem_N.GetRoleID() == -1)
			{
				forcePlayerItem_N.ResetAll();
				forcePlayerItem_N.gameObject.SetActive(value: false);
				m_CurForcePlayerItems.Remove(forcePlayerItem_N);
				m_ForcePlayerItemPool.Enqueue(forcePlayerItem_N);
			}
			else
			{
				forcePlayerItem_N.ResetPlayer();
			}
			RenameAllItem();
			ForcePlayerItem_N nullJoinableItem = GetNullJoinableItem();
			if (null == nullJoinableItem)
			{
				TryAddNullItem();
			}
			else
			{
				nullJoinableItem.UpdateCanJoinCount(GetCurJoinableCount(), m_ForceDesc.JoinablePlayerCount);
			}
			UpdateForcePlayerCount();
			RepostionForce();
		}
	}

	private void RenameAllItem()
	{
		for (int i = 0; i < m_CurForcePlayerItems.Count; i++)
		{
			m_CurForcePlayerItems[i].gameObject.name = "ForcePlayerItem" + i.ToString("D4");
		}
	}

	private ForcePlayerItem_N GetNullJoinableItem()
	{
		return m_CurForcePlayerItems.FirstOrDefault((ForcePlayerItem_N a) => a.GetPlayerID() == -1 && a.GetRoleID() == -1);
	}

	private void RepostionForce()
	{
		ForceGrid.Reposition();
		if (RepositionEvent != null)
		{
			RepositionEvent();
		}
	}

	private void UpdateForceName(string name)
	{
		if (name != null)
		{
			ForceNameLabel.text = name;
		}
	}

	private void UpdateForceColor(Color32 color)
	{
		ForceColorSprite.color = new Color((float)(int)color.r / 255f, (float)(int)color.g / 255f, (float)(int)color.b / 255f, (float)(int)color.a / 255f);
	}

	private void UpdateForcePlayerCount()
	{
		if (m_CurForcePlayerItems != null && m_ForceDesc != null)
		{
			ForceCountLabel.text = $"{GetCurPlayerCount()}/{GetCanJoinPlayerCount()}";
		}
	}
}
