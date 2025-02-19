using System;
using UnityEngine;

[Serializable]
public class UIConpomentEvent
{
	[SerializeField]
	public string functionName = string.Empty;

	public void Send(GameObject target, object sender, bool includeChildren = false)
	{
		if (string.IsNullOrEmpty(functionName) || target == null)
		{
			return;
		}
		if (includeChildren)
		{
			Transform[] componentsInChildren = target.GetComponentsInChildren<Transform>();
			int i = 0;
			for (int num = componentsInChildren.Length; i < num; i++)
			{
				Transform transform = componentsInChildren[i];
				transform.gameObject.SendMessage(functionName, sender, SendMessageOptions.DontRequireReceiver);
			}
		}
		else
		{
			target.SendMessage(functionName, sender, SendMessageOptions.DontRequireReceiver);
		}
	}
}
