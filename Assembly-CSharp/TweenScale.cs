using UnityEngine;

[AddComponentMenu("NGUI/Tween/Scale")]
public class TweenScale : UITweener
{
	public Vector3 from = Vector3.one;

	public Vector3 to = Vector3.one;

	public bool updateTable;

	public bool updateChildTable = true;

	private Transform mTrans;

	private UITable mTable;

	private UITable[] mChildTables;

	public Transform cachedTransform
	{
		get
		{
			if (mTrans == null)
			{
				mTrans = base.transform;
			}
			return mTrans;
		}
	}

	public Vector3 scale
	{
		get
		{
			return cachedTransform.localScale;
		}
		set
		{
			cachedTransform.localScale = value;
		}
	}

	protected override void OnUpdate(float factor, bool isFinished)
	{
		cachedTransform.localScale = from * (1f - factor) + to * factor;
		if (updateTable)
		{
			if (mTable == null)
			{
				mTable = NGUITools.FindInParents<UITable>(base.gameObject);
				if (mTable == null)
				{
					updateTable = false;
					return;
				}
			}
			mTable.repositionNow = true;
		}
		if (!updateChildTable)
		{
			return;
		}
		if (mChildTables == null)
		{
			mChildTables = base.gameObject.GetComponentsInChildren<UITable>(includeInactive: true);
			if (mChildTables == null)
			{
				updateChildTable = false;
				return;
			}
		}
		UITable[] array = mChildTables;
		foreach (UITable uITable in array)
		{
			uITable.repositionNow = true;
		}
	}

	public static TweenScale Begin(GameObject go, float duration, Vector3 scale)
	{
		TweenScale tweenScale = UITweener.Begin<TweenScale>(go, duration);
		tweenScale.from = tweenScale.scale;
		tweenScale.to = scale;
		if (duration <= 0f)
		{
			tweenScale.Sample(1f, isFinished: true);
			tweenScale.enabled = false;
		}
		return tweenScale;
	}
}
