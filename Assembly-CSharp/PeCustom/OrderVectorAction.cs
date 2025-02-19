using Pathea;
using PETools;
using ScenarioRTL;
using UnityEngine;

namespace PeCustom;

[Statement("ORDER VECTOR", true)]
public class OrderVectorAction : Action
{
	private const int radius = 5;

	private OBJECT obj;

	private ECommand cmd;

	private EFunc func;

	private Vector3 pos;

	protected override void OnCreate()
	{
		obj = Utility.ToObject(base.parameters["object"]);
		cmd = (ECommand)Utility.ToEnumInt(base.parameters["command"]);
		func = Utility.ToFunc(base.parameters["func"]);
		pos = Utility.ToVector(base.missionVars, base.parameters["vector"]);
	}

	public override bool Logic()
	{
		PeEntity entity = PeScenarioUtility.GetEntity(obj);
		if (entity != null && (entity.proto == EEntityProto.Npc || entity.proto == EEntityProto.RandomNpc || entity.proto == EEntityProto.Monster) && entity.requestCmpt != null)
		{
			if (cmd == ECommand.MoveTo)
			{
				entity.requestCmpt.Register(EReqType.MoveToPoint, LocalToWorld(entity.peTrans, pos, func), 1f, true, SpeedState.Run);
			}
			else if (cmd == ECommand.FaceAt)
			{
				entity.requestCmpt.Register(EReqType.Dialogue, string.Empty, LocalToWorld(entity.peTrans, pos, func));
			}
		}
		return true;
	}

	private Vector3 LocalToWorld(PeTrans trans, Vector3 local, EFunc func)
	{
		Vector3 vector = Vector3.Cross(Vector3.up, trans.forward) * pos.x + Vector3.up * pos.y + trans.forward * pos.z;
		switch (func)
		{
		case EFunc.Plus:
			vector = PEUtil.GetRandomPositionOnGround(trans.position + vector, 0f, 5f);
			break;
		case EFunc.Minus:
			vector = PEUtil.GetRandomPositionOnGround(trans.position - vector, 0f, 5f);
			break;
		case EFunc.SetTo:
			vector = local;
			break;
		}
		return vector;
	}
}
