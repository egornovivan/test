using uLink;

namespace CustomData;

public class CompoudItem
{
	public float curTime;

	public float time = -1f;

	public int itemID;

	public int itemCnt;

	public bool IsFinished => curTime >= time;

	public static object Deserialize(BitStream stream, params object[] codecOptions)
	{
		CompoudItem compoudItem = new CompoudItem();
		compoudItem.curTime = stream.Read<float>(new object[0]);
		compoudItem.time = stream.Read<float>(new object[0]);
		compoudItem.itemID = stream.Read<int>(new object[0]);
		compoudItem.itemCnt = stream.Read<int>(new object[0]);
		return compoudItem;
	}

	public static void Serialize(BitStream stream, object value, params object[] codecOptions)
	{
		CompoudItem compoudItem = (CompoudItem)value;
		stream.Write(compoudItem.curTime);
		stream.Write(compoudItem.time);
		stream.Write(compoudItem.itemID);
		stream.Write(compoudItem.itemCnt);
	}
}
