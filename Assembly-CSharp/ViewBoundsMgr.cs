using System.Collections.Generic;
using UnityEngine;

public class ViewBoundsMgr : MonoBehaviour
{
	private static ViewBoundsMgr mInstance;

	private Stack<ViewBounds> mStack;

	public static ViewBoundsMgr Instance
	{
		get
		{
			if (null == mInstance)
			{
				GameObject gameObject = new GameObject("ViewBoundsMgr");
				gameObject.transform.position = Vector3.zero;
				gameObject.transform.rotation = Quaternion.identity;
				mInstance = gameObject.AddComponent<ViewBoundsMgr>();
			}
			return mInstance;
		}
	}

	private void Awake()
	{
		mStack = new Stack<ViewBounds>();
		base.transform.position = Vector3.zero;
		base.transform.rotation = Quaternion.identity;
	}

	public ViewBounds Get()
	{
		ViewBounds viewBounds = null;
		if (mStack.Count > 0)
		{
			viewBounds = mStack.Pop();
		}
		if (null == viewBounds)
		{
			viewBounds = CreatViewBounds();
		}
		viewBounds.gameObject.SetActive(value: true);
		return viewBounds;
	}

	public void Recycle(ViewBounds bounds)
	{
		bounds.gameObject.SetActive(value: false);
		mStack.Push(bounds);
	}

	private ViewBounds CreatViewBounds()
	{
		GameObject gameObject = Object.Instantiate(Resources.Load("Prefab/Other/ViewBounds")) as GameObject;
		if (null == gameObject)
		{
			Debug.LogError("Can't find ViewBounds");
			return null;
		}
		gameObject.transform.parent = base.transform;
		return gameObject.GetComponent<ViewBounds>();
	}
}
