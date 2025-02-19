using System;
using UnityEngine;

public class GrassTest : MonoBehaviour
{
	public GameObject mNewPrefab;

	public GameObject mRandomPrefab;

	private void OnGUI()
	{
		if (GUI.Button(new Rect(300f, 50f, 100f, 50f), "New Grass"))
		{
			GameObject gameObject = UnityEngine.Object.Instantiate(mNewPrefab);
			gameObject.SetActive(value: false);
		}
		if (GUI.Button(new Rect(300f, 110f, 100f, 50f), "GC"))
		{
			GC.Collect();
		}
		if (GUI.Button(new Rect(300f, 170f, 100f, 50f), "Random Grass"))
		{
			GameObject gameObject2 = UnityEngine.Object.Instantiate(mRandomPrefab);
			gameObject2.SetActive(value: false);
		}
	}

	private void Start()
	{
	}

	private void Update()
	{
	}
}
