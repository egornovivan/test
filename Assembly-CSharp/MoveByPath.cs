using UnityEngine;
using UnityEngine.Events;
using WhiteCat;

public class MoveByPath : MonoBehaviour
{
	private GameObject pathObj;

	private BezierPath mPath;

	private PathDriver mDrive;

	private TweenPathDriver mTween;

	private TweenInterpolator mPolator;

	public static bool IsPlaying(GameObject target)
	{
		TweenInterpolator component = target.GetComponent<TweenInterpolator>();
		if (component == null)
		{
			return false;
		}
		return component.isPlaying;
	}

	private void SetData(GameObject path)
	{
		pathObj = path;
		mPath = path.GetComponent<BezierPath>();
		mDrive = base.gameObject.AddComponent<PathDriver>();
		mDrive.path = mPath;
		mTween = base.gameObject.AddComponent<TweenPathDriver>();
		mTween.from = 0f;
		mTween.to = mDrive.path.pathTotalLength;
		if (mPolator == null)
		{
			mPolator = base.gameObject.AddComponent<TweenInterpolator>();
			mPolator.enabled = false;
			mPolator.wrapMode = WhiteCat.WrapMode.Once;
		}
	}

	public void StartMove(GameObject pathObj, RotationMode rotateType = RotationMode.Ignore, TweenMethod moveType = TweenMethod.Linear)
	{
		SetData(pathObj);
		mDrive.rotationMode = rotateType;
		mPolator.method = moveType;
		mPolator.enabled = true;
		mPolator.onArriveAtEnding.AddListener(Destroy);
	}

	public void SetDurationDelay(float totalTime, float delay)
	{
		if (null == mPolator)
		{
			mPolator = base.gameObject.AddComponent<TweenInterpolator>();
			mPolator.enabled = false;
			mPolator.wrapMode = WhiteCat.WrapMode.Once;
		}
		mPolator.duration = totalTime;
		mPolator.delay = delay;
	}

	public void AddStartListener(UnityAction call)
	{
		if (null == mPolator)
		{
			mPolator = base.gameObject.AddComponent<TweenInterpolator>();
			mPolator.enabled = false;
			mPolator.wrapMode = WhiteCat.WrapMode.Once;
		}
		mPolator.onArriveAtBeginning.AddListener(call);
	}

	public void AddEndListener(UnityAction call)
	{
		if (null == mPolator)
		{
			mPolator = base.gameObject.AddComponent<TweenInterpolator>();
			mPolator.enabled = false;
			mPolator.wrapMode = WhiteCat.WrapMode.Once;
		}
		mPolator.onArriveAtEnding.AddListener(call);
	}

	private void Destroy()
	{
		Object.Destroy(mTween);
		Object.Destroy(mPolator);
		Object.Destroy(mDrive);
		Object.Destroy(pathObj);
		Object.Destroy(this);
	}
}
