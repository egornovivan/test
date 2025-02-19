using System.Collections.Generic;
using UnityEngine;

public class DunItemGeneratorMgr : MonoBehaviour
{
	public List<GameObject> allGenerators = new List<GameObject>();

	public List<int> weightList = new List<int>();

	public int pickMin;

	public int pickMax;

	private void Awake()
	{
		foreach (GameObject allGenerator in allGenerators)
		{
			if (allGenerator != null)
			{
				allGenerator.SetActive(value: true);
			}
		}
	}
}
