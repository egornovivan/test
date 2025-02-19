using System.IO;
using Pathea;
using PETools;
using ScenarioRTL;

namespace PeCustom;

[Statement("STOP ANIMATION", true)]
public class StopAnimAction : Action
{
	private OBJECT obj;

	protected override void OnCreate()
	{
		obj = Utility.ToObject(base.parameters["object"]);
	}

	public override bool Logic()
	{
		if (PeGameMgr.IsMulti)
		{
			byte[] array = Serialize.Export(delegate(BinaryWriter w)
			{
				BufferHelper.Serialize(w, obj);
			});
			PlayerNetwork.RequestServer(EPacketType.PT_Custom_StopAnimation, array);
		}
		else
		{
			PeEntity entity = PeScenarioUtility.GetEntity(obj);
			if (null == entity)
			{
				return true;
			}
			entity.animCmpt.SetTrigger("Custom_ResetAni");
		}
		return true;
	}
}
