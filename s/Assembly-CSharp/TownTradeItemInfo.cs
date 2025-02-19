using System.Collections.Generic;
using ItemAsset;
using uLink;
using UnityEngine;

public class TownTradeItemInfo
{
	public delegate void RefreshEvent(TownTradeItemInfo target);

	public List<TradeObj> needItems = new List<TradeObj>();

	public List<TradeObj> rewardItems = new List<TradeObj>();

	public int campId;

	public float m_CurTime;

	public float m_Time;

	public string icon;

	public IntVector2 pos;

	public string name;

	public CSTradeInfoData csti;

	public RefreshEvent RefreshCome;

	public float timeLeft => m_Time - m_CurTime;

	public TownTradeItemInfo(IntVector2 pos)
	{
		this.pos = pos;
	}

	public TownTradeItemInfo(DetectedTown dt)
	{
		pos = dt.PosCenter;
		name = dt.name;
		campId = dt.campId;
	}

	public void InitRewardItem()
	{
		for (int i = 0; i < rewardItems.Count; i++)
		{
			rewardItems[i].count = 0;
		}
	}

	public void setNeedNum(int protoId, int num)
	{
		TradeObj tradeObj = needItems.Find((TradeObj it) => it.protoId == protoId);
		if (tradeObj != null)
		{
			tradeObj.count = Mathf.Min(num, tradeObj.max);
		}
	}

	public void setRewardNum(int protoId, int num)
	{
		TradeObj tradeObj = rewardItems.Find((TradeObj it) => it.protoId == protoId);
		if (tradeObj != null)
		{
			tradeObj.count = Mathf.Min(num, tradeObj.max);
		}
	}

	public int GetNeedItemSumPrice()
	{
		int num = 0;
		for (int i = 0; i < needItems.Count; i++)
		{
			num += ItemProto.GetPrice(needItems[i].protoId) * needItems[i].count;
		}
		return num;
	}

	public int GetRewardItemSumPrice()
	{
		int num = 0;
		for (int i = 0; i < needItems.Count; i++)
		{
			num += ItemProto.GetPrice(rewardItems[i].protoId) * rewardItems[i].count;
		}
		return num;
	}

	public void NeedRefresh()
	{
		if (RefreshCome != null)
		{
			RefreshCome(this);
		}
	}

	public void DoTrade(ICollection<TradeObj> need)
	{
		TradeObj to;
		foreach (TradeObj item in need)
		{
			to = item;
			TradeObj tradeObj = needItems.Find((TradeObj it) => it.protoId == to.protoId);
			if (tradeObj != null)
			{
				tradeObj.count -= to.count;
				if (tradeObj.count < 0)
				{
					tradeObj.count = 0;
				}
			}
		}
	}

	public static object Deserialize(uLink.BitStream stream, params object[] codecOptions)
	{
		IntVector2 intVector = new IntVector2(stream.ReadInt32(), stream.ReadInt32());
		TownTradeItemInfo townTradeItemInfo = new TownTradeItemInfo(intVector);
		townTradeItemInfo.csti = CSTradeInfoData.GetData(stream.ReadInt32());
		townTradeItemInfo.m_CurTime = stream.ReadSingle();
		townTradeItemInfo.m_Time = stream.ReadSingle();
		townTradeItemInfo.icon = townTradeItemInfo.csti.icon;
		int num = stream.ReadInt32();
		for (int i = 0; i < num; i++)
		{
			townTradeItemInfo.needItems.Add((TradeObj)TradeObj.Deserialize(stream));
		}
		int num2 = stream.ReadInt32();
		for (int j = 0; j < num2; j++)
		{
			townTradeItemInfo.rewardItems.Add((TradeObj)TradeObj.Deserialize(stream));
		}
		return townTradeItemInfo;
	}

	public static void Serialize(uLink.BitStream stream, object value, params object[] codecOptions)
	{
		TownTradeItemInfo townTradeItemInfo = value as TownTradeItemInfo;
		stream.WriteInt32(townTradeItemInfo.pos.x);
		stream.WriteInt32(townTradeItemInfo.pos.y);
		stream.WriteInt32(townTradeItemInfo.csti.id);
		stream.WriteSingle(townTradeItemInfo.m_CurTime);
		stream.WriteSingle(townTradeItemInfo.m_Time);
		stream.WriteInt32(townTradeItemInfo.needItems.Count);
		foreach (TradeObj needItem in townTradeItemInfo.needItems)
		{
			TradeObj.Serialize(stream, needItem);
		}
		stream.WriteInt32(townTradeItemInfo.rewardItems.Count);
		foreach (TradeObj rewardItem in townTradeItemInfo.rewardItems)
		{
			TradeObj.Serialize(stream, rewardItem);
		}
	}
}
