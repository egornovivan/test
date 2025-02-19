using PeMap;
using UnityEngine;

public class ServantDeadLabel : ILabel
{
	private int icon;

	private Vector3 pos;

	private string text;

	public int servantId;

	public ServantDeadLabel(int _icon, Vector3 _pos, string _text, int _id)
	{
		icon = _icon;
		pos = _pos;
		text = _text;
		servantId = _id;
	}

	int ILabel.GetIcon()
	{
		return icon;
	}

	Vector3 ILabel.GetPos()
	{
		return pos;
	}

	string ILabel.GetText()
	{
		return text;
	}

	bool ILabel.FastTravel()
	{
		return false;
	}

	ELabelType ILabel.GetType()
	{
		return ELabelType.Npc;
	}

	bool ILabel.NeedArrow()
	{
		return false;
	}

	float ILabel.GetRadius()
	{
		return -1f;
	}

	EShow ILabel.GetShow()
	{
		return EShow.All;
	}

	public bool CompareTo(ILabel label)
	{
		if (label is ServantDeadLabel)
		{
			ServantDeadLabel servantDeadLabel = (ServantDeadLabel)label;
			if (servantId == servantDeadLabel.servantId)
			{
				return true;
			}
			return false;
		}
		return false;
	}
}
