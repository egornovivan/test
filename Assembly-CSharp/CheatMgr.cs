using System;
using Pathea;
using UnityEngine;

public class CheatMgr : MonoBehaviour
{
	private void Awake()
	{
		UIMessageCtrl component = GetComponent<UIMessageCtrl>();
		if (null != component)
		{
			component.SeedMsgEvent = (Action<string>)Delegate.Combine(component.SeedMsgEvent, new Action<string>(CheckCheat));
		}
	}

	private void CheckCheat(string msg)
	{
		CheckAddCheatItem(msg);
	}

	private void CheckAddCheatItem(string msg)
	{
		if (null == PeSingleton<MainPlayer>.Instance.entity)
		{
			return;
		}
		CheatData data = CheatData.GetData(msg);
		if (data == null)
		{
			return;
		}
		if (data.addType == 1)
		{
			if (PeSingleton<MainPlayer>.Instance.entity.packageCmpt.Add(data.itemID, 1) && "0" != data.successNotice)
			{
				PeTipMsg.Register(data.successNotice, PeTipMsg.EMsgLevel.HighLightRed);
			}
		}
		else if (data.addType == 2 && VCEditor.MakeCreation("Isos/Mission/" + data.isoName) == 0 && "0" != data.successNotice)
		{
			PeTipMsg.Register(data.successNotice, PeTipMsg.EMsgLevel.HighLightRed);
		}
	}

	private void CheckMoveNPCByName(string msg)
	{
		string[] array = msg.Split(',');
		if (array.Length > 1 && array[1].Contains("come here"))
		{
			PeEntity peEntity = PeSingleton<EntityMgr>.Instance.Get(array[0]);
			if (null != peEntity)
			{
				NpcMgr.CallBackNpcToMainPlayer(peEntity);
			}
		}
	}
}
