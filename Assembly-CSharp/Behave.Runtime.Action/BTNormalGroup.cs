using System;
using UnityEngine;

namespace Behave.Runtime.Action;

public class BTNormalGroup : BTAction
{
	protected bool GetData<T>(Tree sender, ref T t)
	{
		if (m_TreeDataList.ContainsKey(sender.ActiveStringParameter))
		{
			try
			{
				t = (T)m_TreeDataList[sender.ActiveStringParameter];
				return true;
			}
			catch (Exception message)
			{
				Debug.LogWarning(message);
				return false;
			}
		}
		return false;
	}
}
