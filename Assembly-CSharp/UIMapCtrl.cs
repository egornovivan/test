using Pathea;
using UnityEngine;

public class UIMapCtrl : MonoBehaviour
{
	public OnGuiBtnClicked mMapMove;

	public OnGuiBtnClicked mMapZoomed;

	public Camera mMapCamera;

	public float mCameraSizes;

	public int mMaxSzieCount;

	public int mSpritLeng;

	public UILabel mLbMapScale;

	public int mCameraSizeCount = 2;

	public int mMaxMapHight;

	public int mMaxMapWidth;

	private bool isChangeZoomed;

	private void Awake()
	{
		ChangeCameraSize();
	}

	private void Start()
	{
	}

	private void Update()
	{
		if (UICamera.hoveredObject == base.gameObject)
		{
			float axis = Input.GetAxis("Mouse ScrollWheel");
			if (Mathf.Abs(axis) > float.Epsilon)
			{
				UpdateZoomed(axis);
			}
		}
	}

	private void UpdateZoomed(float ds)
	{
		if (!isChangeZoomed)
		{
			isChangeZoomed = true;
			if (ds > 0f)
			{
				ZoomedOut();
			}
			else
			{
				ZoomedIn();
			}
			Invoke("SetChageZoomedFale", 0.1f);
		}
	}

	private void SetChageZoomedFale()
	{
		isChangeZoomed = false;
	}

	private void ZoomedIn()
	{
		if (mCameraSizeCount < mMaxSzieCount)
		{
			mCameraSizeCount *= 2;
			ChangeCameraSize();
		}
	}

	private void ZoomedOut()
	{
		if (mCameraSizeCount > 1)
		{
			mCameraSizeCount /= 2;
			ChangeCameraSize();
		}
	}

	private void BtnZoomedIn()
	{
		ZoomedIn();
	}

	private void BtnZoomedOut()
	{
		ZoomedOut();
	}

	private void OnDrag(Vector2 DopPos)
	{
		MoveMapCamera(DopPos);
	}

	private void ChangeCameraSize()
	{
		mMapCamera.orthographicSize = mCameraSizes * (float)mCameraSizeCount;
		mLbMapScale.text = 1 * mSpritLeng * mCameraSizeCount + "m";
		if (mMapZoomed != null)
		{
			mMapZoomed();
		}
	}

	private void MoveMapCamera(Vector2 pos)
	{
		Vector3 localPosition = mMapCamera.transform.localPosition;
		localPosition.x -= pos.x * (float)mCameraSizeCount;
		localPosition.y -= pos.y * (float)mCameraSizeCount;
		if (PeGameMgr.IsSingleStory)
		{
			if (localPosition.x >= 0f && localPosition.x <= (float)mMaxMapWidth && localPosition.y >= 0f && localPosition.y < (float)mMaxMapHight)
			{
				mMapCamera.transform.localPosition = localPosition;
				if (mMapMove != null)
				{
					mMapMove();
				}
			}
		}
		else
		{
			mMapCamera.transform.localPosition = localPosition;
			if (mMapMove != null)
			{
				mMapMove();
			}
		}
	}
}
