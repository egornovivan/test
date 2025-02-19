using System.Collections.Generic;
using UnityEngine;

public class TreeGlowMgr : MonoBehaviour
{
	public List<TreeGlowNode> TreeGlowList;

	private void Start()
	{
	}

	private void Update()
	{
		if (GameUI.Instance == null)
		{
			foreach (TreeGlowNode treeGlow in TreeGlowList)
			{
				treeGlow.Mat.SetFloat("_Glow", 0f);
			}
			return;
		}
		float time = (float)GameTime.Timer.TimeInDay;
		foreach (TreeGlowNode treeGlow2 in TreeGlowList)
		{
			treeGlow2.Mat.SetFloat("_Glow", treeGlow2.Brightness.Evaluate(time));
		}
	}
}
