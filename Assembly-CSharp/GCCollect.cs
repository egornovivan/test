using System;
using UnityEngine;

public class GCCollect : MonoBehaviour
{
	private void OnGUI()
	{
		if (GUI.Button(new Rect(50f, 50f, 200f, 50f), "GC.Collect"))
		{
			GC.Collect();
		}
	}
}
