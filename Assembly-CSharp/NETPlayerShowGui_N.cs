using System.Collections.Generic;
using UnityEngine;

public class NETPlayerShowGui_N : UIStaticWnd
{
	public NetPlayerInfoItem_N mPerfab;

	public UIGrid mGrid;

	private List<NetPlayerInfoItem_N> mPlayerList = new List<NetPlayerInfoItem_N>();

	public void RemovePlayer(NetPlayerInfoItem_N item)
	{
		mPlayerList.Remove(item);
		item.transform.parent = null;
		Object.Destroy(item.gameObject);
		mGrid.Reposition();
	}
}
