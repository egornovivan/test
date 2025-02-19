using Railway;
using UnityEngine;

public class UIRailStation : MonoBehaviour
{
	public OnGuiIndexBaseCallBack mClickStatge;

	public Point mRailPointData;

	private UISprite mSprStation;

	public void Init(Point _mRailPointData)
	{
		mRailPointData = _mRailPointData;
		mSprStation = base.gameObject.GetComponent<UISprite>();
	}

	private void OnClickStatge()
	{
		if (mRailPointData != null && mRailPointData.pointType != 0 && mClickStatge != null)
		{
			mClickStatge(mRailPointData.id);
		}
	}

	public void SetSelected(bool isSelected)
	{
		if (mSprStation != null)
		{
			mSprStation.color = ((!isSelected) ? new Color(1f, 1f, 1f, 1f) : new Color(0f, 1f, 0.4f, 1f));
		}
	}
}
