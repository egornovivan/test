using System.Collections.Generic;
using System.Linq;
using Pathea;
using UnityEngine;

public class UIAllianceCtrl : UIBaseWidget
{
	[SerializeField]
	private UIGrid m_Grid;

	[SerializeField]
	private AllianceItem_N m_AllianceItemPrefab;

	private Queue<AllianceItem_N> m_ItemPools = new Queue<AllianceItem_N>();

	private Dictionary<int, AllianceItem_N> m_CurItemDic = new Dictionary<int, AllianceItem_N>();

	private int m_MainPlayerID;

	protected override void InitWindow()
	{
		base.InitWindow();
	}

	public override void Show()
	{
		base.Show();
		UpdateAllianceInfo();
		VArtifactUtil.RegistTownDestryedEvent(UpdateBuildCountEvent);
		PeSingleton<ReputationSystem>.Instance.onReputationChange += UpdateReputationEvent;
	}

	protected override void OnHide()
	{
		base.OnHide();
		RecoveryItem();
		VArtifactUtil.UnRegistTownDestryedEvent(UpdateBuildCountEvent);
		PeSingleton<ReputationSystem>.Instance.onReputationChange -= UpdateReputationEvent;
	}

	private void UpdateBuildCountEvent(int allyID)
	{
		int playerId = VATownGenerator.Instance.GetPlayerId(allyID);
		if (m_CurItemDic.ContainsKey(playerId))
		{
			m_CurItemDic[playerId].UpdateBuildCount();
		}
	}

	private void UpdateReputationEvent(int forceID, int targetPlayerID)
	{
		float num = Singleton<ForceSetting>.Instance.GetForceID(m_MainPlayerID);
		if (num == (float)forceID && m_CurItemDic.ContainsKey(targetPlayerID))
		{
			m_CurItemDic[targetPlayerID].UpdateReputation();
		}
	}

	private AllianceItem_N GetNewItem()
	{
		AllianceItem_N allianceItem_N = null;
		if (m_ItemPools.Count > 0)
		{
			allianceItem_N = m_ItemPools.Dequeue();
		}
		else
		{
			GameObject gameObject = Object.Instantiate(m_AllianceItemPrefab.gameObject);
			gameObject.transform.parent = m_Grid.transform;
			gameObject.transform.localRotation = Quaternion.identity;
			gameObject.transform.localPosition = Vector3.zero;
			gameObject.transform.localScale = Vector3.one;
			allianceItem_N = gameObject.GetComponent<AllianceItem_N>();
		}
		allianceItem_N.gameObject.SetActive(value: true);
		return allianceItem_N;
	}

	private void UpdateAllianceInfo()
	{
		m_MainPlayerID = (int)PeSingleton<PeCreature>.Instance.mainPlayer.GetAttribute(AttribType.DefaultPlayerID);
		int allyCount = RandomMapConfig.allyCount;
		for (int i = 1; i < allyCount; i++)
		{
			AllianceItem_N newItem = GetNewItem();
			int playerId = VATownGenerator.Instance.GetPlayerId(i);
			newItem.UpdateInfo(i, playerId, m_MainPlayerID);
			m_CurItemDic.Add(playerId, newItem);
		}
		m_Grid.Reposition();
	}

	private void RecoveryItem()
	{
		if (m_CurItemDic.Count > 0)
		{
			AllianceItem_N[] array = m_CurItemDic.Values.ToArray();
			for (int i = 0; i < array.Length; i++)
			{
				array[i].Reset();
				array[i].gameObject.SetActive(value: false);
				m_ItemPools.Enqueue(array[i]);
			}
			m_CurItemDic.Clear();
		}
	}
}
