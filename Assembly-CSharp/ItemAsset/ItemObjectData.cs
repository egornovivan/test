using uLink;

namespace ItemAsset;

public class ItemObjectData
{
	public int itemId;

	public int objId;

	public int num;

	public int[] properties;

	public float[] values;

	public ItemObjectData()
	{
	}

	public ItemObjectData(int itemId, int objId, int num, int[] properties, float[] values)
	{
		this.itemId = itemId;
		this.objId = objId;
		this.num = num;
		this.properties = properties;
		this.values = values;
	}

	internal static object ReadItem(BitStream stream, params object[] codecOptions)
	{
		ItemObjectData itemObjectData = new ItemObjectData();
		itemObjectData.itemId = stream.Read<int>(new object[0]);
		itemObjectData.objId = stream.Read<int>(new object[0]);
		itemObjectData.num = stream.Read<int>(new object[0]);
		itemObjectData.properties = stream.Read<int[]>(new object[0]);
		itemObjectData.values = stream.Read<float[]>(new object[0]);
		return itemObjectData;
	}

	internal static void WriteItem(BitStream stream, object obj, params object[] codecOptions)
	{
		ItemObjectData itemObjectData = (ItemObjectData)obj;
		stream.Write(itemObjectData.itemId);
		stream.Write(itemObjectData.objId);
		stream.Write(itemObjectData.num);
		stream.Write(itemObjectData.properties);
		stream.Write(itemObjectData.values);
	}
}
