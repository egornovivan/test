using UnityEngine;

public class ItemScript_Colony : ItemScript
{
	public CSBuildingLogic csbl;

	public override void OnConstruct()
	{
		base.OnConstruct();
		csbl = GetComponentInParent<CSBuildingLogic>();
		if (!GameConfig.IsMultiMode)
		{
			CSMgCreator s_MgCreator = CSMain.s_MgCreator;
			if (s_MgCreator != null)
			{
				CSEntityObject component = GetComponent<CSEntityObject>();
				int num = ((!(csbl != null)) ? component.Init(base.itemObjectId, s_MgCreator) : component.Init(csbl, s_MgCreator));
				if (num != 4)
				{
					Debug.LogError("Error with Init Entities");
				}
				else if (component.m_Type == CSConst.ObjectType.Assembly)
				{
					CSMain.SinglePlayerCheckClod();
				}
			}
			return;
		}
		int num2 = ((!(csbl != null)) ? mNetlayer.TeamId : csbl.TeamId);
		CSMgCreator cSMgCreator = ((num2 != BaseNetwork.MainPlayer.TeamId) ? (CSMain.Instance.MultiGetOtherCreator(num2) as CSMgCreator) : CSMain.s_MgCreator);
		if (cSMgCreator != null)
		{
			CSEntityObject component2 = GetComponent<CSEntityObject>();
			ColonyNetwork colonyNetwork = mNetlayer as ColonyNetwork;
			component2._ColonyObj = colonyNetwork._ColonyObj;
			int num3 = ((!(csbl != null)) ? component2.Init(base.itemObjectId, cSMgCreator) : component2.Init(csbl, cSMgCreator));
			if (num3 != 4)
			{
				Debug.LogError("Error with Init Entities");
			}
			else if (component2.m_Type == CSConst.ObjectType.Assembly)
			{
				CSMain.SinglePlayerCheckClod();
			}
		}
	}

	public override void OnDestruct()
	{
		base.OnDestruct();
		if (!GameConfig.IsMultiMode)
		{
			SendMessage("OnDestoryGo", mItemObj.instanceId);
		}
	}
}
