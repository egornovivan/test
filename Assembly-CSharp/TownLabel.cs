using Pathea;
using PeMap;
using UnityEngine;

public abstract class TownLabel : ILabel
{
	public int allyId;

	public int allyColorId;

	public Vector3 pos;

	public string name;

	public TownLabelType townLabelType;

	public Vector3 GetPos()
	{
		return pos;
	}

	public new ELabelType GetType()
	{
		return ELabelType.Mark;
	}

	public bool NeedArrow()
	{
		return false;
	}

	public float GetRadius()
	{
		return -1f;
	}

	public EShow GetShow()
	{
		return EShow.BigMap;
	}

	public abstract int GetIcon();

	public virtual string GetText()
	{
		return name;
	}

	public abstract bool FastTravel();

	public virtual int GetColor()
	{
		return 0;
	}

	public virtual int GetAllianceColor()
	{
		return allyColorId;
	}

	public virtual int GetFriendlyLevel()
	{
		return -1;
	}

	public static bool Remove(Vector3 pos)
	{
		ILabel label = PeSingleton<LabelMgr>.Instance.Find((ILabel item) => (item is TownLabel townLabel && townLabel.pos == pos) ? true : false);
		if (label != null)
		{
			return PeSingleton<LabelMgr>.Instance.Remove(label);
		}
		return false;
	}

	public virtual bool CompareTo(ILabel label)
	{
		if (label is TownLabel)
		{
			TownLabel townLabel = (TownLabel)label;
			if (townLabelType == townLabel.townLabelType)
			{
				if (name != null && townLabel.name != null)
				{
					return pos == townLabel.pos && name.Equals(townLabel.name);
				}
				if (name == null && townLabel.name == null)
				{
					return pos == townLabel.pos;
				}
				return false;
			}
			return false;
		}
		return false;
	}
}
