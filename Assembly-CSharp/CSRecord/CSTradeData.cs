using System.Collections.Generic;

namespace CSRecord;

public class CSTradeData : CSObjectData
{
	public Dictionary<int, stShopData> mShopList;

	public CSTradeData()
	{
		dType = 10;
		mShopList = new Dictionary<int, stShopData>();
	}
}
