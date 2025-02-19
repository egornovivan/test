using Pathea;
using PeMap;
using UnityEngine;

public class DungeonEntranceLabel : ILabel
{
	private Vector3 pos;

	private string name;

	public DungeonEntranceLabel(Vector3 pos)
	{
		this.pos = pos;
		name = PELocalization.GetString(8000889);
		PeSingleton<LabelMgr>.Instance.Add(this);
	}

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

	public int GetIcon()
	{
		return 44;
	}

	public string GetText()
	{
		return name;
	}

	public bool FastTravel()
	{
		return false;
	}

	public static bool Remove(Vector3 pos)
	{
		ILabel label = PeSingleton<LabelMgr>.Instance.Find((ILabel item) => (item is DungeonEntranceLabel dungeonEntranceLabel && dungeonEntranceLabel.pos == pos) ? true : false);
		if (label != null)
		{
			return PeSingleton<LabelMgr>.Instance.Remove(label);
		}
		return false;
	}

	public virtual bool CompareTo(ILabel label)
	{
		if (label is DungeonEntranceLabel)
		{
			DungeonEntranceLabel dungeonEntranceLabel = (DungeonEntranceLabel)label;
			if (name != null && dungeonEntranceLabel.name != null)
			{
				return pos == dungeonEntranceLabel.pos && name.Equals(dungeonEntranceLabel.name);
			}
			return pos == dungeonEntranceLabel.pos;
		}
		return false;
	}
}
