using System.Collections.Generic;
using ScenarioRTL;
using ScenarioRTL.IO;

namespace PeCustom;

public class MissionProperty
{
	public enum EType
	{
		Hidden,
		MainStory,
		SideQuest
	}

	public string name;

	public EType type;

	public bool canAbort;

	public string objective;

	public int beginNpcId;

	public int beginNpcWorldIndex;

	public int endNpcId;

	public int endNpcWorldIndex;

	public string rewardDesc;

	public List<int> rewardItemIds;

	public List<int> rewardItemCount;

	public MissionProperty()
	{
		rewardItemCount = new List<int>(5);
		rewardItemIds = new List<int>(5);
	}

	public void Parse(ParamRaw param, string mission_name)
	{
		name = mission_name;
		type = (EType)Utility.ToInt(null, param["type"]);
		canAbort = Utility.ToBool(null, param["can_abort"]);
		objective = param["objective"];
		string text = null;
		string[] array = null;
		try
		{
			text = param["begin_npc"];
			array = text.Split('|');
			beginNpcWorldIndex = int.Parse(array[0]);
			beginNpcId = int.Parse(array[1]);
		}
		catch
		{
			beginNpcWorldIndex = -1;
			beginNpcId = -1;
		}
		try
		{
			text = param["end_npc"];
			array = text.Split('|');
			endNpcWorldIndex = int.Parse(array[0]);
			endNpcId = int.Parse(array[1]);
		}
		catch
		{
			endNpcId = -1;
			endNpcWorldIndex = -1;
		}
		try
		{
			text = param["award"];
			array = text.Split('|');
			if (array != null && array.Length != 0)
			{
				for (int i = 0; i < array.Length; i++)
				{
					string[] array2 = array[i].Split(':');
					rewardItemIds.Add(int.Parse(array2[0]));
					rewardItemCount.Add(int.Parse(array2[1]));
				}
			}
			else
			{
				rewardDesc = text;
			}
		}
		catch
		{
			rewardDesc = text;
		}
	}
}
