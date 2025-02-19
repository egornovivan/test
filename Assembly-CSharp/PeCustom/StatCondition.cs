using System.IO;
using Pathea;
using ScenarioRTL;

namespace PeCustom;

[Statement("STAT")]
public class StatCondition : Condition
{
	private OBJECT obj;

	private AttribType stat;

	private ECompare comp;

	private float amt;

	protected override void OnCreate()
	{
		obj = Utility.ToObject(base.parameters["object"]);
		stat = (AttribType)Utility.ToEnumInt(base.parameters["stat"]);
		comp = Utility.ToCompare(base.parameters["compare"]);
		amt = Utility.ToSingle(base.missionVars, base.parameters["amount"]);
	}

	public override bool? Check()
	{
		PeEntity entity = PeScenarioUtility.GetEntity(obj);
		if (entity != null && entity.skEntity != null && Utility.Compare(entity.skEntity.GetAttribute((int)stat), amt, comp))
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
			BufferHelper.Serialize(w, obj);
			w.Write(amt);
			w.Write((byte)stat);
			w.Write((byte)comp);
		});
		PlayerNetwork.RequestServer(EPacketType.PT_Custom_CheckAttribute, array);
	}
}
