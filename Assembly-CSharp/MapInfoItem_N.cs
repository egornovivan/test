using System;
using UnityEngine;

public class MapInfoItem_N : MonoBehaviour
{
	private bool mIsMissionTrack;

	public int mMissionId;

	private Vector3 mMissionPos;

	public MonoBehaviour mAiObj;

	public UISprite mIcon;

	private float mWorldMapConvert = 1f;

	private float mViewRadius = 128f;

	private float mMissionRadius = 32f;

	private bool mIsBigMap;

	public void SetAiObj(MonoBehaviour aiObj)
	{
		mIcon.MakePixelPerfect();
		Update();
	}

	public void SetMissionTrack(int missionId, Vector3 pos, bool worldMap = false)
	{
		mIsBigMap = worldMap;
		mIsMissionTrack = true;
		mMissionId = missionId;
		mMissionPos = pos;
		if (worldMap)
		{
			mIcon.spriteName = "MissionTrack";
			mIcon.MakePixelPerfect();
		}
		Update();
	}

	public void SetMissionRadius(float radius)
	{
		mMissionRadius = Mathf.Clamp(radius * mWorldMapConvert, 32f, 128f);
	}

	public void SetMapInfo(float convert, float viewRadius)
	{
		mWorldMapConvert = 1f;
		mViewRadius = viewRadius;
		Update();
	}

	private void Update()
	{
		if (mIsMissionTrack)
		{
			Vector3 vector = Vector3.zero;
			if (!mIsBigMap)
			{
				vector.y = vector.z;
				vector.z = 0f;
				vector *= mWorldMapConvert;
				if (vector.magnitude + 30f > mViewRadius + mMissionRadius)
				{
					mIcon.spriteName = "MissionTrackDirArrow";
					vector = (mViewRadius - 10f) * vector.normalized;
					mIcon.MakePixelPerfect();
					base.transform.localScale = new Vector3(11f, 16f, 1f);
					base.transform.rotation = Quaternion.FromToRotation(Vector3.up, vector);
				}
				else
				{
					mIcon.spriteName = "MissionTrack";
					base.transform.localScale = new Vector3(mMissionRadius, mMissionRadius, 1f);
					base.transform.rotation = Quaternion.identity;
				}
				vector.x = Convert.ToInt32(Mathf.Round(vector.x) * GameUI.Instance.mUIMinMapCtrl.mMapScale.x);
				vector.y = Convert.ToInt32(Mathf.Round(vector.y) * GameUI.Instance.mUIMinMapCtrl.mMapScale.y);
				base.transform.localPosition = vector;
			}
		}
		else if (mAiObj == null)
		{
			UnityEngine.Object.Destroy(base.gameObject);
		}
		else
		{
			Vector3 zero = Vector3.zero;
			zero *= mWorldMapConvert;
			zero.x = (int)zero.x;
			zero.y = (int)zero.y;
			if (!mIsBigMap)
			{
				zero.x = Convert.ToInt32(zero.x * GameUI.Instance.mUIMinMapCtrl.mMapScale.x);
				zero.y = Convert.ToInt32(zero.y * GameUI.Instance.mUIMinMapCtrl.mMapScale.y);
			}
			base.transform.localPosition = new Vector3(zero.x, zero.z, 0f);
		}
	}
}
