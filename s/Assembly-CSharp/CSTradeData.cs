using System.Collections.Generic;

public class CSTradeData : CSObjectData
{
	public Dictionary<int, Record.stShopData> mShopList;

	public CSTradeData()
	{
		dType = 10;
		mShopList = new Dictionary<int, Record.stShopData>();
	}
}
