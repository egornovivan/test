using System.IO;
using Pathea;
using PETools;
using ScenarioRTL;

namespace PeCustom;

[Statement("CANCEL ORDER", true)]
public class CancelOrderAction : Action
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
			PlayerNetwork.RequestServer(EPacketType.PT_Custom_CancelOrder, array);
		}
		else
		{
			PeEntity entity = PeScenarioUtility.GetEntity(obj);
			if (entity != null && entity.requestCmpt != null)
			{
				if (entity.requestCmpt.Contains(EReqType.Dialogue))
				{
					entity.requestCmpt.RemoveRequest(EReqType.Dialogue);
				}
				if (entity.requestCmpt.Contains(EReqType.MoveToPoint))
				{
					entity.requestCmpt.RemoveRequest(EReqType.MoveToPoint);
				}
				if (entity.requestCmpt.Contains(EReqType.FollowTarget))
				{
					entity.requestCmpt.RemoveRequest(EReqType.FollowTarget);
				}
			}
		}
		return true;
	}
}
