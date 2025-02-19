using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class CommonManager : MonoBehaviour
{
	public delegate void CommonEventHandler();

	public static CommonManager _self;

	private List<CommonEventHandler> _commonEventHandler = new List<CommonEventHandler>();

	private void Awake()
	{
		_self = this;
	}

	private void Start()
	{
		StartCoroutine(AutoUpdate());
	}

	private void Update()
	{
	}

	public void RegisterEvent(CommonEventHandler REvent)
	{
		if (!_commonEventHandler.Contains(REvent))
		{
			_commonEventHandler.Add(REvent);
		}
	}

	public void UnRegisterEvent(CommonEventHandler REvent)
	{
		_commonEventHandler.Remove(REvent);
	}

	private IEnumerator AutoUpdate()
	{
		while (true)
		{
			yield return new WaitForSeconds(2f);
			foreach (CommonEventHandler iter in _commonEventHandler)
			{
				iter();
			}
		}
	}
}
