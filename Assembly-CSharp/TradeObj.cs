using System;
using uLink;

[Obsolete]
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

	public TradeObj(CSUI_ItemInfo ci)
	{
		protoId = ci.protoId;
		count = ci.Number;
	}

	public TradeObj(int protoId, int count, int max)
	{
		this.protoId = protoId;
		this.count = count;
		this.max = max;
	}

	public static object Deserialize(BitStream stream, params object[] codecOptions)
	{
		try
		{
			int num = stream.ReadInt32();
			int num2 = stream.ReadInt32();
			int num3 = stream.ReadInt32();
			return new TradeObj(num, num2, num3);
		}
		catch (Exception ex)
		{
			throw ex;
		}
	}

	public static void Serialize(BitStream stream, object value, params object[] codecOptions)
	{
		try
		{
			TradeObj tradeObj = value as TradeObj;
			stream.WriteInt32(tradeObj.protoId);
			stream.WriteInt32(tradeObj.count);
			stream.WriteInt32(tradeObj.max);
		}
		catch (Exception ex)
		{
			throw ex;
		}
	}
}
