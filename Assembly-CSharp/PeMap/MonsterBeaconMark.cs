using UnityEngine;

namespace PeMap;

public class MonsterBeaconMark : ILabel
{
	private string text = "MonsterBeacon";

	private int icon = 45;

	public Vector3 Position;

	public bool IsMonsterSiege;

	ELabelType ILabel.GetType()
	{
		return ELabelType.Mark;
	}

	public int GetIcon()
	{
		return icon;
	}

	public Vector3 GetPos()
	{
		return Position;
	}

	public string GetText()
	{
		return text;
	}

	public bool FastTravel()
	{
		return false;
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
		return EShow.All;
	}

	public bool CompareTo(ILabel label)
	{
		return label is MonsterBeaconMark monsterBeaconMark && monsterBeaconMark.Position == Position;
	}
}
