using System.Collections.Generic;
using Pathea;

public class ShopData
{
	public int m_ID;

	public int m_ItemID;

	public int m_Price1;

	public int m_Price2;

	public int m_ExtDemand;

	public int m_LimitNum;

	public int m_RefreshTime;

	public int m_LimitType;

	public List<int> m_LimitMisIDList = new List<int>();

	public int m_Price
	{
		get
		{
			if (!Money.Digital)
			{
				return m_Price1;
			}
			return m_Price2;
		}
	}
}
