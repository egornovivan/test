using UnityEngine;

namespace PeMap;

public class LabelMark : ILabel
{
	public Vector3 pos;

	public string text;

	public int icon;

	public ELabelType labelType;

	public bool needArrow;

	public float radius = -1f;

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
		return labelType;
	}

	bool ILabel.NeedArrow()
	{
		return needArrow;
	}

	float ILabel.GetRadius()
	{
		return radius;
	}

	EShow ILabel.GetShow()
	{
		return EShow.All;
	}

	public bool CompareTo(ILabel label)
	{
		if (label is LabelMark)
		{
			LabelMark labelMark = (LabelMark)label;
			if (pos == labelMark.pos && icon == labelMark.icon)
			{
				return true;
			}
			return false;
		}
		return false;
	}
}
