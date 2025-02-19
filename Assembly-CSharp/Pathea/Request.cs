using System.Collections.Generic;
using System.IO;

namespace Pathea;

public abstract class Request
{
	private int m_ID;

	private string m_Name;

	private EReqMask m_Mask;

	private Dictionary<EReqType, EReqRelation> m_Relations = new Dictionary<EReqType, EReqRelation>();

	public int ID => m_ID;

	public string Name => m_Name;

	public EReqMask Mask => m_Mask;

	public abstract EReqType Type { get; }

	public Request()
	{
		m_Mask = EReqMask.None;
		m_Name = Type.ToString();
		m_ID = RequestRelation.Name2ID(m_Name);
	}

	public abstract void Serialize(BinaryWriter w);

	public abstract void Deserialize(BinaryReader r);

	public void Addmask(EReqMask mask)
	{
		m_Mask |= mask;
	}

	public void RemoveMask(EReqMask mask)
	{
		m_Mask &= ~mask;
	}

	public void AddRelation(EReqType type, EReqRelation relation)
	{
		if (m_Relations.ContainsKey(type))
		{
			m_Relations[type] = relation;
		}
		else
		{
			m_Relations.Add(type, relation);
		}
	}

	public void RemoveRelation(EReqType type)
	{
		if (m_Relations.ContainsKey(type))
		{
			m_Relations.Remove(type);
		}
	}

	public EReqRelation GetRelation(Request request)
	{
		if (request.GetRelation(Type, out var relation))
		{
			return relation;
		}
		return (EReqRelation)RequestRelation.GetRelationValue((int)Type, (int)request.Type);
	}

	public bool CanRun()
	{
		return (m_Mask & EReqMask.Blocking) == 0 && Type != EReqType.Attack;
	}

	public bool CanWait()
	{
		return (m_Mask & EReqMask.Stucking) == 0;
	}

	private bool GetRelation(EReqType type, out EReqRelation relation)
	{
		if (m_Relations.ContainsKey(type))
		{
			relation = m_Relations[type];
			return true;
		}
		relation = EReqRelation.None;
		return false;
	}
}
