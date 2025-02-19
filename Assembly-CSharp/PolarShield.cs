using System.Collections.Generic;
using Pathea;
using PETools;
using UnityEngine;

[RequireComponent(typeof(SphereCollider))]
public class PolarShield : MonoBehaviour
{
	public delegate void TriggerEvent(PeEntity peEntity, int skillId);

	public delegate void TriggerExitEvent(PeEntity peEntity);

	public GameObject m_Model;

	public int[] levelSkills = new int[3];

	public float m_Radius;

	public float target_Radius;

	public float min_Radius = 35f;

	public int m_Level;

	private static List<PolarShield> allShileds = new List<PolarShield>();

	private List<PeEntity> monsterList = new List<PeEntity>();

	private SphereCollider m_Collider;

	private Transform m_Trans;

	public bool IsEmpty;

	private int counter;

	public int GetSkillID => levelSkills[m_Level];

	public Vector3 Pos
	{
		get
		{
			return m_Trans.position;
		}
		set
		{
			m_Trans.position = value;
		}
	}

	public event TriggerEvent onEnterTrigger;

	public event TriggerExitEvent onExitTrigger;

	public static bool IsInsidePolarShield(Vector3 pos, int level)
	{
		for (int i = 0; i < allShileds.Count; i++)
		{
			if (allShileds[i].Inside(pos) && allShileds[i].Difference(level) >= 2)
			{
				return true;
			}
		}
		return false;
	}

	public static bool GetPolarShield(Vector3 pos, int level, out Vector3 center, out float radius)
	{
		for (int i = 0; i < allShileds.Count; i++)
		{
			if (allShileds[i].Inside(pos) && allShileds[i].Difference(level) >= 2)
			{
				radius = allShileds[i].m_Radius;
				center = allShileds[i].Pos;
				return true;
			}
		}
		radius = 0f;
		center = Vector3.zero;
		return false;
	}

	public static Vector3 GetRandomPosition(Vector3 pos, int level)
	{
		for (int i = 0; i < allShileds.Count; i++)
		{
			if (allShileds[i].Inside(pos) && allShileds[i].Difference(level) >= 2)
			{
				return PEUtil.GetRandomPosition(allShileds[i].Pos, pos - allShileds[i].Pos, allShileds[i].m_Radius * 1.2f, allShileds[i].m_Radius * 1.5f, -75f, 75f);
			}
		}
		return Vector3.zero;
	}

	public void ShowModel(bool mShow)
	{
		m_Model.SetActive(mShow);
	}

	public void SetLerpRadius(float radius)
	{
		if (m_Radius < radius)
		{
			if (m_Radius < min_Radius)
			{
				m_Radius = min_Radius;
				m_Trans.localScale = new Vector3(m_Radius * 2f, m_Radius * 2f, m_Radius * 2f);
			}
			target_Radius = radius;
		}
	}

	public void SetRadius(float radius)
	{
		target_Radius = radius;
		m_Radius = target_Radius;
		m_Trans.localScale = new Vector3(m_Radius * 2f, m_Radius * 2f, m_Radius * 2f);
	}

	public void SetLevel(int level)
	{
		m_Level = level;
	}

	public bool Inside(Vector3 pos)
	{
		return (m_Trans.position - pos).sqrMagnitude <= m_Radius * m_Radius;
	}

	public int Difference(int lv)
	{
		return m_Level + 1 - lv;
	}

	private void Awake()
	{
		m_Collider = GetComponent<Collider>() as SphereCollider;
		m_Collider.isTrigger = true;
		m_Trans = base.transform;
		allShileds.Add(this);
	}

	private void Start()
	{
	}

	private void Update()
	{
		counter++;
		if (counter % 24 == 0)
		{
			List<PeEntity> list = monsterList.FindAll((PeEntity it) => it.IsDeath());
			for (int num = list.Count - 1; num >= 0; num--)
			{
				if (this.onExitTrigger != null)
				{
					this.onExitTrigger(list[num]);
				}
				monsterList.RemoveAt(num);
			}
			counter = 0;
		}
		IsEmpty = monsterList.Count == 0;
		if (m_Radius < target_Radius)
		{
			m_Radius += 0.2f;
			m_Trans.localScale = new Vector3(m_Radius * 2f, m_Radius * 2f, m_Radius * 2f);
		}
	}

	private void OnTriggerEnter(Collider other)
	{
		PeEntity componentInParent = other.GetComponentInParent<PeEntity>();
		if (!(componentInParent == null) && componentInParent.proto == EEntityProto.Monster && !monsterList.Contains(componentInParent))
		{
			monsterList.Add(componentInParent);
			int getSkillID = GetSkillID;
			if (this.onEnterTrigger != null)
			{
				this.onEnterTrigger(componentInParent, getSkillID);
			}
		}
	}

	private void OnTriggerExit(Collider other)
	{
		PeEntity componentInParent = other.GetComponentInParent<PeEntity>();
		if (!(componentInParent == null) && componentInParent.proto == EEntityProto.Monster && monsterList.Contains(componentInParent))
		{
			monsterList.Remove(componentInParent);
			if (this.onExitTrigger != null)
			{
				this.onExitTrigger(componentInParent);
			}
		}
	}

	public void AfterUpdate()
	{
		foreach (PeEntity monster in monsterList)
		{
			if (this.onExitTrigger != null)
			{
				this.onExitTrigger(monster);
			}
		}
		int getSkillID = GetSkillID;
		foreach (PeEntity monster2 in monsterList)
		{
			if (this.onEnterTrigger != null)
			{
				this.onEnterTrigger(monster2, getSkillID);
			}
		}
	}

	private void OnDestroy()
	{
		allShileds.Remove(this);
		foreach (PeEntity monster in monsterList)
		{
			if (this.onExitTrigger != null)
			{
				this.onExitTrigger(monster);
			}
		}
	}
}
