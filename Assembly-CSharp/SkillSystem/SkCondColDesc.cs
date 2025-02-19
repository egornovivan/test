using System.Linq;
using Pathea;
using UnityEngine;

namespace SkillSystem;

public class SkCondColDesc
{
	public enum ColType
	{
		ColName,
		ColLayer
	}

	public string[][] colStrings = new string[2][];

	public ColType[] colTypes = new ColType[2];

	public SkCondColDesc(string colDesc)
	{
		string[] array = colDesc.Split(new char[1] { ',' }, 2);
		colStrings[0] = array[0].Split(':');
		int num = 0;
		if (colStrings[0].Length > 1)
		{
			colTypes[0] = ((!colStrings[0][0].Contains("name")) ? ColType.ColLayer : ColType.ColName);
			num = 1;
		}
		colStrings[0] = colStrings[0][num].Split('|');
		num = 0;
		colStrings[1] = array[1].Split(':');
		if (colStrings[1].Length > 1)
		{
			colTypes[1] = ((!colStrings[1][0].Contains("name")) ? ColType.ColLayer : ColType.ColName);
			num = 1;
		}
		colStrings[1] = colStrings[1][num].Split('|');
	}

	public bool Contain(PECapsuleHitResult colInfo)
	{
		string value = ((colTypes[0] != 0) ? LayerMask.LayerToName(colInfo.selfTrans.gameObject.layer) : colInfo.selfTrans.name);
		string value2 = ((colTypes[1] != 0) ? LayerMask.LayerToName(colInfo.hitTrans.gameObject.layer) : colInfo.hitTrans.name);
		if (colStrings[0][0].Length == 0 || colStrings[0].Contains(value))
		{
			if (colStrings[1][0].Length == 0 || colStrings[1].Contains(value2))
			{
				return true;
			}
			return false;
		}
		return false;
	}
}
