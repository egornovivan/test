using System.Collections.Generic;
using Pathea;
using UnityEngine;

public class UIHpChange : MonoBehaviour
{
	private static UIHpChange s_Instance;

	private List<UIHpNode> m_Nodes;

	[SerializeField]
	private GameObject nodePrefab;

	[SerializeField]
	private Transform contentTs;

	[SerializeField]
	private float m_DataOutTime = 1f;

	public Color MonsterHurt = Color.red;

	public Color NPCHurt = Color.blue;

	public Color CreationHurt = Color.yellow;

	public Color PlayerHurt = Color.red;

	public Color PlayerHeal = Color.green;

	public Color DoodadHurt = Color.red;

	private IHPEventData m_HPEventData;

	private bool m_CurFrameHasShow;

	[SerializeField]
	private EEntityProto testProto;

	[SerializeField]
	private bool testAdd;

	[SerializeField]
	private int testValue = -200;

	[SerializeField]
	private Transform testTs;

	public static UIHpChange Instance => s_Instance;

	public bool m_ShowHPChange => SystemSettingData.Instance.HPNumbers;

	private void Awake()
	{
		s_Instance = this;
		m_Nodes = new List<UIHpNode>();
		m_HPEventData = PeSingleton<HPChangeEventDataMan>.Instance;
	}

	private void Update()
	{
		if (Application.isEditor)
		{
		}
		m_CurFrameHasShow = false;
		while (!m_CurFrameHasShow && m_HPEventData != null)
		{
			HPChangeEventData hPChangeEventData = m_HPEventData.Pop();
			if (hPChangeEventData == null || Time.realtimeSinceStartup - hPChangeEventData.m_AddTime >= m_DataOutTime)
			{
				m_CurFrameHasShow = true;
			}
			else if (null != hPChangeEventData.m_Transfrom && null != hPChangeEventData.m_Self)
			{
				PeEntity component = hPChangeEventData.m_Self.GetComponent<PeEntity>();
				if (null != component && !component.IsDeath() && ShowHPChange(hPChangeEventData.m_HPChange, hPChangeEventData.m_Transfrom.position, hPChangeEventData.m_Proto))
				{
					m_CurFrameHasShow = true;
				}
			}
		}
	}

	public void RemoveNode(UIHpNode node)
	{
		if (node != null)
		{
			node.transform.parent = null;
			Object.Destroy(node.gameObject);
		}
		m_Nodes.Remove(node);
	}

	private void AddNode(Vector3 position, float hpChange, Color col)
	{
		GameObject gameObject = Object.Instantiate(nodePrefab);
		gameObject.transform.parent = contentTs;
		gameObject.transform.localScale = Vector3.one;
		gameObject.SetActive(value: true);
		UIHpNode component = gameObject.GetComponent<UIHpNode>();
		component.color = col;
		component.worldPostion = position + Random.insideUnitSphere + Vector3.up;
		component.text = hpChange.ToString("0");
		component.isHurt = hpChange < 0f;
		m_Nodes.Add(component);
	}

	private bool ShowHPChange(float hpChange, Vector3 position, EEntityProto entityProto)
	{
		if (!m_ShowHPChange)
		{
			return false;
		}
		if (GameUI.Instance == null)
		{
			return false;
		}
		if (GameUI.Instance.mMainPlayer == null)
		{
			return false;
		}
		if (GameUI.Instance.mUIWorldMap == null)
		{
			return false;
		}
		if (GameUI.Instance.mUIWorldMap.isShow)
		{
			return false;
		}
		if ((position - GameUI.Instance.mMainPlayer.position).magnitude > 100f)
		{
			return false;
		}
		if (s_Instance == null)
		{
			return false;
		}
		if (hpChange == 0f)
		{
			return false;
		}
		if (hpChange < 1f && hpChange > 0f)
		{
			hpChange = 1f;
		}
		if (hpChange > -1f && hpChange < 0f)
		{
			hpChange = -1f;
		}
		Color col = Color.red;
		if (CanShowHpChange(hpChange, entityProto, out col))
		{
			AddNode(position, hpChange, col);
			return true;
		}
		return false;
	}

	private bool CanShowHpChange(float hpChange, EEntityProto entityProto, out Color col)
	{
		col = Color.red;
		if ((hpChange > 0f && entityProto != 0) || PeGameMgr.gamePause)
		{
			return false;
		}
		switch (entityProto)
		{
		case EEntityProto.Player:
			col = ((!(hpChange < 0f)) ? PlayerHeal : PlayerHurt);
			break;
		case EEntityProto.RandomNpc:
		case EEntityProto.Npc:
			col = NPCHurt;
			break;
		case EEntityProto.Tower:
			col = CreationHurt;
			break;
		case EEntityProto.Monster:
			col = MonsterHurt;
			break;
		case EEntityProto.Doodad:
			col = DoodadHurt;
			break;
		}
		return true;
	}

	private void UpdateTest()
	{
		if (testAdd && testTs != null)
		{
			Color col = Color.red;
			if (CanShowHpChange(testValue, testProto, out col))
			{
				AddNode(testTs.localPosition, testValue, col);
			}
			testAdd = false;
		}
	}
}
