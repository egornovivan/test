using uLink;

public class TradeObj
{
	public int protoId;

	public int count;

	public int max;

	public TradeObj(int protoId, int count)
	{
		this.protoId = protoId;
		this.count = count;
	}

	public TradeObj(int protoId, int count, int max)
	{
		this.protoId = protoId;
		this.count = count;
		this.max = max;
	}

	public static object Deserialize(BitStream stream, params object[] codecOptions)
	{
		int num = stream.ReadInt32();
		int num2 = stream.ReadInt32();
		int num3 = stream.ReadInt32();
		return new TradeObj(num, num2, num3);
	}

	public static void Serialize(BitStream stream, object value, params object[] codecOptions)
	{
		TradeObj tradeObj = value as TradeObj;
		stream.WriteInt32(tradeObj.protoId);
		stream.WriteInt32(tradeObj.count);
		stream.WriteInt32(tradeObj.max);
	}
}
