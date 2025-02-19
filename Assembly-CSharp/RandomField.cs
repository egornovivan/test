using System.Collections.Generic;

public class RandomField
{
	public List<TargetListInfo> TargetIDMap;

	public List<TargetListInfo> TalkOPMap;

	public List<TargetListInfo> TalkINMap;

	public List<TargetListInfo> TalkEDMap;

	public List<TargetListInfo> TalkOPSMap;

	public List<TargetListInfo> TalkINSMap;

	public List<TargetListInfo> TalkEDSMap;

	public List<List<MissionIDNum>> RewardMap;

	public List<MissionIDNum> FixedRewardMap;

	public bool keepItem;

	public RandomField()
	{
		TargetIDMap = new List<TargetListInfo>();
		TalkOPMap = new List<TargetListInfo>();
		TalkINMap = new List<TargetListInfo>();
		TalkEDMap = new List<TargetListInfo>();
		TalkOPSMap = new List<TargetListInfo>();
		TalkINSMap = new List<TargetListInfo>();
		TalkEDSMap = new List<TargetListInfo>();
		RewardMap = new List<List<MissionIDNum>>();
		FixedRewardMap = new List<MissionIDNum>();
	}
}
