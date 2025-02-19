using System;
using UnityEngine;

public class MissionTargetItem_N : MonoBehaviour
{
	public UILabel mContent;

	public UISprite mTargetTex;

	public UIGrid mGrid;

	public UIMissionMgr.TargetShow targetShow;

	public void SetTarget(UIMissionMgr.TargetShow target)
	{
		targetShow = target;
		UpdateContent();
		if (target.mIconName.Count == 0)
		{
			mTargetTex.gameObject.SetActive(value: false);
		}
		else
		{
			mTargetTex.spriteName = target.mIconName[0];
		}
		TypeMonsterData typeMonsterData = MissionManager.GetTypeMonsterData(target.mID);
		if (typeMonsterData != null && typeMonsterData.type == 2)
		{
			mTargetTex.spriteName = "Null";
		}
		mTargetTex.MakePixelPerfect();
		for (int i = 1; i < target.mIconName.Count; i++)
		{
			GameObject gameObject = UnityEngine.Object.Instantiate(mTargetTex.gameObject);
			gameObject.transform.parent = mGrid.transform;
			if (typeMonsterData != null && typeMonsterData.type == 2)
			{
				gameObject.GetComponent<UISprite>().spriteName = "Null";
			}
			else
			{
				gameObject.GetComponent<UISprite>().spriteName = target.mIconName[i];
			}
			gameObject.GetComponent<UISprite>().MakePixelPerfect();
		}
		mGrid.Reposition();
	}

	private void Update()
	{
		Vector3 localPosition = base.gameObject.transform.localPosition;
		base.gameObject.transform.localPosition = new Vector3(Convert.ToInt32(localPosition.x), Convert.ToInt32(localPosition.y), Convert.ToInt32(localPosition.z));
		UpdateContent();
	}

	private void UpdateContent()
	{
		if (targetShow != null)
		{
			string text = ((targetShow.mMaxCount <= 0) ? string.Empty : (targetShow.mCount + "/" + targetShow.mMaxCount));
			mContent.text = targetShow.mContent + " " + text;
		}
	}
}
