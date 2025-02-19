using System.Collections;
using System.Collections.Generic;
using UnityEngine;

internal class ActionManager : MonoBehaviour
{
	private static List<ActionDelegate> _syncList = new List<ActionDelegate>();

	private void Start()
	{
		StartCoroutine(CheckActions());
	}

	private void OnApplicationQuit()
	{
		_syncList.Clear();
	}

	private IEnumerator CheckActions()
	{
		while (true)
		{
			if (_syncList.Count > 0)
			{
				ActionDelegate action = _syncList[0];
				if (action != null)
				{
					action.OnAction();
					_syncList.RemoveAt(0);
				}
			}
			yield return null;
		}
	}

	internal static void AddAction(ActionDelegate action)
	{
		_syncList.Add(action);
	}
}
