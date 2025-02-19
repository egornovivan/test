using Pathea;
using ScenarioRTL;

namespace PeCustom;

public class MissionGoal_Item : MissionGoal
{
	public OBJECT item;

	public ECompare compare;

	public int target;

	public int current
	{
		get
		{
			if (PeSingleton<PeCreature>.Instance != null && PeSingleton<PeCreature>.Instance.mainPlayer != null)
			{
				PeEntity mainPlayer = PeSingleton<PeCreature>.Instance.mainPlayer;
				if (item.isSpecificPrototype)
				{
					return mainPlayer.packageCmpt.GetItemCount(item.Id);
				}
				if (item.isAnyPrototypeInCategory)
				{
					return mainPlayer.packageCmpt.GetCountByEditorType(item.Group);
				}
				if (item.isAnyPrototype)
				{
					return mainPlayer.packageCmpt.GetAllItemsCount();
				}
			}
			return 0;
		}
	}

	public override bool achieved
	{
		get
		{
			return Utility.Compare(current, target, compare);
		}
		set
		{
		}
	}
}
