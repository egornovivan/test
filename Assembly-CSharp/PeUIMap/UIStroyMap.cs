using Pathea;
using UnityEngine;

namespace PeUIMap;

public class UIStroyMap : UIMap
{
	public const float StoryWorldSize = 18432f;

	public const int ShowFindNpcRangeMissionID = 383;

	[SerializeField]
	private UISprite m_PlayerFindNpcRangeSpr;

	[SerializeField]
	private UICheckbox m_FindNpcRangeCheck;

	[HideInInspector]
	public bool ShowFindNpcRange;

	private Vector2 mapSize => new Vector2(18432f, 18432f);

	protected override void InitWindow()
	{
		base.InitWindow();
		mScale = 1f;
		mScaleMin = 1f;
		mMapPosMin = Vector3.zero;
		mScaleMin = (float)Screen.width / base.texSize;
		mMapPosMin.x = (base.texSize - (float)Screen.width) / 2f;
		mMapPosMin.y = (base.texSize - (float)Screen.height) / 2f;
		mMapPos = GetInitPos();
		mMapPos.x = Mathf.Clamp(mMapPos.x, 0f - mMapPosMin.x, mMapPosMin.x);
		mMapPos.y = Mathf.Clamp(mMapPos.y, 0f - mMapPosMin.y, mMapPosMin.y);
		mMapWnd.transform.localPosition = new Vector3(mMapPos.x, mMapPos.y, -10f);
		mMeatSprite.gameObject.SetActive(value: true);
		mMoneySprite.gameObject.SetActive(value: false);
		m_PlayerFindNpcRangeSpr.gameObject.SetActive(value: false);
		if (MissionManager.Instance != null)
		{
			ShowFindNpcRange = MissionManager.Instance.HadCompleteMissionAnyNum(383);
		}
		else
		{
			Debug.Log("MissionManager is null!");
		}
	}

	protected override void OpenWarpWnd(UIMapLabel label)
	{
		if (Money.Digital)
		{
			mMeatSprite.gameObject.SetActive(value: false);
			mMoneySprite.gameObject.SetActive(value: true);
		}
		else
		{
			mMeatSprite.gameObject.SetActive(value: true);
			mMoneySprite.gameObject.SetActive(value: false);
		}
		base.OpenWarpWnd(label);
	}

	protected override Vector3 GetUIPos(Vector3 worldPos)
	{
		Vector3 zero = Vector3.zero;
		zero.x = (worldPos.x - mapSize.x / 2f) * base.texSize / mapSize.x;
		zero.y = (worldPos.z - mapSize.y / 2f) * base.texSize / mapSize.y;
		zero.z = -1f;
		return zero;
	}

	protected override float ConvetMToPx(float m)
	{
		return base.texSize / 18432f * m;
	}

	protected override Vector3 GetInitPos()
	{
		if (GameUI.Instance.mMainPlayer != null)
		{
			Vector3 position = GameUI.Instance.mMainPlayer.position;
			Vector3 zero = Vector3.zero;
			zero.x = (0f - (position.x - mapSize.x / 2f)) * base.texSize / mapSize.x;
			zero.y = (0f - (position.z - mapSize.y / 2f)) * base.texSize / mapSize.y;
			zero.z = -1f;
			return zero;
		}
		return Vector3.zero;
	}

	protected override Vector3 GetWorldPos(Vector2 mousePos)
	{
		Vector3 zero = Vector3.zero;
		Vector3 zero2 = Vector3.zero;
		zero2.x = (0f - mMapPos.x) * mapSize.x / (base.texSize * mScale) + mapSize.x / 2f;
		zero2.z = (0f - mMapPos.y) * mapSize.y / (base.texSize * mScale) + mapSize.y / 2f;
		Vector3 zero3 = Vector3.zero;
		zero3.x = (mousePos.x - (float)(Screen.width / 2)) * mapSize.x / (base.texSize * mScale);
		zero3.z = (mousePos.y - (float)(Screen.height / 2)) * mapSize.y / (base.texSize * mScale);
		zero = zero2 + zero3;
		zero.y = 0f;
		return zero;
	}

	protected override void Update()
	{
		base.Update();
		if (ShowFindNpcRange && m_FindNpcRangeCheck.isChecked)
		{
			if (!m_PlayerFindNpcRangeSpr.gameObject.activeSelf)
			{
				m_PlayerFindNpcRangeSpr.gameObject.SetActive(value: true);
				float num = ShowNpcRadiusPx * 2f;
				m_PlayerFindNpcRangeSpr.transform.localScale = new Vector3(num, num, 0f);
			}
			m_PlayerFindNpcRangeSpr.transform.localPosition = mPlayerSpr.transform.localPosition;
		}
		else if (m_PlayerFindNpcRangeSpr.gameObject.activeSelf)
		{
			m_PlayerFindNpcRangeSpr.gameObject.SetActive(value: false);
		}
	}
}
