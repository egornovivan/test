using UnityEngine;

namespace Pathea;

public class ColonyViewCmpt : ViewCmpt
{
	private CSBuildingLogic _logic;

	private CSBuildingLogic logic
	{
		get
		{
			if (!_logic)
			{
				_logic = GetComponent<CSBuildingLogic>();
			}
			return _logic;
		}
	}

	public override bool hasView => logic.HasModel;

	public override Transform centerTransform
	{
		get
		{
			if (!hasView)
			{
				return null;
			}
			return logic.transform;
		}
	}
}
