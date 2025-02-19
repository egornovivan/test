using System.IO;
using Pathea;
using ScenarioRTL;

namespace PeCustom;

[Statement("WORLD")]
public class WorldCondition : Condition
{
	private OBJECT player;

	private int worldId;

	protected override void OnCreate()
	{
		player = Utility.ToObject(base.parameters["player"]);
		worldId = Utility.ToInt(base.missionVars, base.parameters["id"]);
	}

	public override bool? Check()
	{
		PeEntity entity = PeScenarioUtility.GetEntity(player);
		if (entity != null && PeSingleton<CustomGameData.Mgr>.Instance.curGameData.WorldIndex == worldId)
		{
			return true;
		}
		return false;
	}

	protected override void SendReq()
	{
		byte[] array = BufferHelper.Export(delegate(BinaryWriter w)
		{
			w.Write(reqId);
			BufferHelper.Serialize(w, player);
			w.Write(worldId);
		});
		PlayerNetwork.RequestServer(EPacketType.PT_Custom_CheckWorld, array);
	}
}
