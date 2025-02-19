using UnityEngine;

public class UIBowSighting : UIBaseSighting
{
	[SerializeField]
	private int mMaxAddPos;

	[SerializeField]
	private float mTime_t = 0.2f;

	[SerializeField]
	private UISprite mSprTopLeft;

	[SerializeField]
	private UISprite mSprTopRight;

	[SerializeField]
	private UISprite mSprButtomLeft;

	[SerializeField]
	private UISprite mSprButtomRight;

	private Vector3 mTopLeft;

	private Vector3 mTopRight;

	private Vector3 mButtomLeft;

	private Vector3 mButtomRight;

	protected override void Start()
	{
		base.Start();
		mTopLeft = mSprTopLeft.transform.localPosition;
		mTopRight = mSprTopRight.transform.localPosition;
		mButtomLeft = mSprButtomLeft.transform.localPosition;
		mButtomRight = mSprButtomRight.transform.localPosition;
	}

	protected override void Update()
	{
		base.Update();
		UpdatePos(mTopLeft, mSprTopLeft.transform, new Vector2(-1f, 1f));
		UpdatePos(mTopRight, mSprTopRight.transform, new Vector2(1f, 1f));
		UpdatePos(mButtomLeft, mSprButtomLeft.transform, new Vector2(-1f, -1f));
		UpdatePos(mButtomRight, mSprButtomRight.transform, new Vector2(1f, -1f));
	}

	private void UpdatePos(Vector3 pos, Transform ts, Vector2 dir)
	{
		Vector3 localPosition = ts.localPosition;
		float x = pos.x + (float)mMaxAddPos * dir.x * base.Value;
		float y = pos.y + (float)mMaxAddPos * dir.y * base.Value;
		ts.localPosition = Vector3.Lerp(localPosition, new Vector3(x, y, pos.z), mTime_t);
	}
}
