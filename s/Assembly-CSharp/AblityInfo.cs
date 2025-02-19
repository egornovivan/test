using System.Collections.Generic;
using ItemAsset;

public class AblityInfo
{
	public bool IsSkill;

	public bool IsBuff;

	public bool IsGetItem;

	public bool IsTalent;

	public float _Percent;

	public float _Skill_R;

	public float _Skill_Per;

	public float _Correctrate;

	public string _icon;

	public int _abityid;

	public int SkillId;

	public int BuffId;

	public int DecsId;

	public int _level;

	public AblityType _type;

	public List<int> _ProtoIds;

	public List<MaterialItem> _Items;

	public AblityInfo()
	{
		_ProtoIds = new List<int>();
		_Items = new List<MaterialItem>();
	}
}
