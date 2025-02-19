using System;
using UnityEngine;

public class RegularMemoryCleaning : MonoBehaviour
{
	[Header("间隔时间，小时")]
	[SerializeField]
	private float _intervalTime = 1f;

	private float _intervalTimeS;

	private float _lastCleanTime;

	private void Awake()
	{
		_intervalTimeS = _intervalTime * 3600f;
		_lastCleanTime = Time.realtimeSinceStartup;
		UnityEngine.Object.DontDestroyOnLoad(base.gameObject);
	}

	private void Update()
	{
		if (Time.realtimeSinceStartup - _lastCleanTime > _intervalTimeS)
		{
			_lastCleanTime = Time.realtimeSinceStartup;
			CleanMemory();
		}
	}

	[ContextMenu("CleanMemory")]
	private void CleanMemory()
	{
		Debug.Log("Auto Clean Memory");
		Resources.UnloadUnusedAssets();
		GC.Collect();
	}
}
