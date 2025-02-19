using System;
using System.Collections.Generic;
using UnityEngine;

public class UITalkBoxCtrl : MonoBehaviour
{
	public const string LANGE_CN = "[lang:cn]";

	public const string LANGE_OTHER = "[lang:other]";

	public UISlicedSprite mWndBg;

	public BoxCollider mTopClider;

	public BoxCollider mRightClider;

	public BoxCollider mDrgPanelClider;

	public UISprite mTopTuodong;

	public UISprite mRightTuodong;

	public GameObject mBtnDown;

	public GameObject mBtnUp;

	public GameObject mBtnClear;

	public GameObject mBtnLock;

	public GameObject mBtnUnLock;

	public CommonChatItem_N m_ChatItemPrefab;

	public UITable m_ContentTable;

	public UIScrollBar mScrollBar;

	private bool IsLock = true;

	private bool IsOnDrapTop;

	private bool IsOnDrapRight;

	public float mMinWidth;

	public float mMinHeight;

	public float mMoveHeight;

	public int FixedTimeCount = 40;

	public int LineWidth = 890;

	private int mMaxMsgCount = 300;

	private int mDeleteMsgCount = 100;

	private List<CommonChatItem_N> m_CurChatItems = new List<CommonChatItem_N>();

	private Queue<CommonChatItem_N> m_ChatItemsPool = new Queue<CommonChatItem_N>();

	private int m_RepositionCount;

	private bool isScroll;

	private int tempCount;

	private bool IsHide;

	private bool IsWndMove;

	private float mWndBgHeight;

	private int Movetemp;

	public void AddMsg(string userName, string strMsg, string strColor)
	{
		bool isChinese = strMsg.Contains("[lang:cn]");
		strMsg = strMsg.Replace("[lang:cn]", string.Empty);
		strMsg = strMsg.Replace("[lang:other]", string.Empty);
		string info = "[" + strColor + "]" + userName + "[-]:" + strMsg;
		CommonChatItem_N newChatItem = GetNewChatItem();
		if (null != newChatItem)
		{
			newChatItem.SetLineWidth(LineWidth);
			newChatItem.UpdateText(isChinese, info);
			m_CurChatItems.Add(newChatItem);
		}
		if (m_CurChatItems.Count > mMaxMsgCount)
		{
			RecoveryItems(mDeleteMsgCount);
		}
		m_RepositionCount = 3;
	}

	public void ClearInputBox()
	{
		RecoveryItems(m_CurChatItems.Count);
	}

	private void Start()
	{
	}

	private void OnScrollValueChage(UIScrollBar bar)
	{
		if (Mathf.Abs(Input.GetAxis("Mouse ScrollWheel")) > float.Epsilon || Input.GetMouseButtonDown(0))
		{
			isScroll = true;
			tempCount = 0;
		}
	}

	private void Update()
	{
		if (IsOnDrapTop)
		{
			OnDrapTop();
		}
		if (IsOnDrapRight)
		{
			OnDrapRight();
		}
		UpdateCliderTrans();
		if (IsWndMove)
		{
			UpdateWndMove();
		}
		UpdateScollValue();
		if (m_RepositionCount > 0)
		{
			m_ContentTable.Reposition();
			mScrollBar.scrollValue = 1f;
			m_RepositionCount--;
		}
	}

	private void UpdateScollValue()
	{
		if (mScrollBar.scrollValue != 1f && isScroll)
		{
			tempCount++;
			if (tempCount >= 300)
			{
				isScroll = false;
				tempCount = 0;
				mScrollBar.scrollValue = 1f;
			}
		}
	}

	private void UpdateCliderTrans()
	{
		Vector3 localScale = mWndBg.gameObject.transform.localScale;
		if (mTopClider.size.y != localScale.y)
		{
			mRightClider.gameObject.transform.localPosition = new Vector3(localScale.x, 0f, 0f);
			mRightClider.center = new Vector3(0f, localScale.y / 2f, -1f);
			mRightClider.size = new Vector3(20f, localScale.y, 1f);
		}
		if (mRightClider.size.x != localScale.x)
		{
			mTopClider.gameObject.transform.localPosition = new Vector3(0f, localScale.y, -1f);
			mTopClider.center = new Vector3(localScale.x / 2f, 0f, -1f);
			mTopClider.size = new Vector3(localScale.x, 20f, 1f);
		}
	}

	private void OnDrapTop()
	{
		float x = Input.mousePosition.x - base.gameObject.transform.localPosition.x;
		Vector3 localPosition = mTopTuodong.transform.localPosition;
		mTopTuodong.transform.localPosition = new Vector3(x, localPosition.y, localPosition.z);
		if (Input.GetMouseButton(0))
		{
			float num = Input.mousePosition.y - base.gameObject.transform.localPosition.y;
			Vector3 localScale = mWndBg.gameObject.transform.localScale;
			if (num > mMinHeight && num < (float)(Screen.height - 100))
			{
				mWndBg.gameObject.transform.localScale = new Vector3(localScale.x, num, localScale.z);
			}
		}
	}

	private void OnDrapRight()
	{
		float y = Input.mousePosition.y - base.gameObject.transform.localPosition.y;
		Vector3 localPosition = mRightTuodong.transform.localPosition;
		mRightTuodong.transform.localPosition = new Vector3(localPosition.x, y, localPosition.z);
		if (Input.GetMouseButton(0))
		{
			float num = Input.mousePosition.x - base.gameObject.transform.localPosition.x;
			Vector3 localScale = mWndBg.gameObject.transform.localScale;
			if (num > mMinWidth && num < (float)(Screen.width - 10))
			{
				mWndBg.gameObject.transform.localScale = new Vector3(num, localScale.y, localScale.z);
				LineWidth = Convert.ToInt32(num - 70f);
				SetCurItemsLineWidth(LineWidth);
			}
		}
	}

	private void RightColiderOnMouseOver()
	{
		if (!IsOnDrapRight && !IsLock)
		{
			Cursor.visible = false;
			float y = Input.mousePosition.y - base.gameObject.transform.localPosition.y;
			Vector3 localPosition = mRightTuodong.transform.localPosition;
			mRightTuodong.transform.localPosition = new Vector3(localPosition.x, y, localPosition.z);
			mRightTuodong.enabled = true;
			IsOnDrapRight = true;
		}
	}

	private void RightColiderOnMouseOut()
	{
		if (IsOnDrapRight && !IsLock)
		{
			Cursor.visible = true;
			mRightTuodong.enabled = false;
			IsOnDrapRight = false;
			m_RepositionCount = 3;
		}
	}

	private void TopColiderOnMouseOver()
	{
		if (!IsOnDrapTop && !IsLock)
		{
			Cursor.visible = false;
			float x = Input.mousePosition.x - base.gameObject.transform.localPosition.x;
			Vector3 localPosition = mTopTuodong.transform.localPosition;
			mTopTuodong.transform.localPosition = new Vector3(x, localPosition.y, localPosition.z);
			mTopTuodong.enabled = true;
			IsOnDrapTop = true;
			Debug.Log("TopColiderOnMouseOver");
		}
	}

	private void TopColiderOnMouseOut()
	{
		if (IsOnDrapTop && !IsLock)
		{
			Cursor.visible = true;
			mTopTuodong.enabled = false;
			IsOnDrapTop = false;
			m_RepositionCount = 3;
			Debug.Log("TopColiderOnMouseOut");
		}
	}

	private void BtnUpOnClick()
	{
		if (IsHide && !IsWndMove && mWndBgHeight != 0f)
		{
			IsHide = false;
			IsWndMove = true;
		}
	}

	private void BtnDownOnClick()
	{
		if (!IsHide && !IsWndMove)
		{
			mWndBgHeight = mWndBg.gameObject.transform.localScale.y;
			Movetemp = Convert.ToInt32((mWndBgHeight - mMoveHeight) / (float)FixedTimeCount);
			IsHide = true;
			IsWndMove = true;
			mBtnDown.SetActive(value: false);
			mBtnUp.SetActive(value: true);
			mBtnLock.SetActive(value: false);
			mBtnUnLock.SetActive(value: false);
			mBtnClear.SetActive(value: false);
			mDrgPanelClider.enabled = false;
			m_RepositionCount = 3;
			mScrollBar.gameObject.SetActive(!IsHide);
		}
	}

	private void UpdateWndMove()
	{
		if (IsHide)
		{
			Vector3 localScale = mWndBg.gameObject.transform.localScale;
			float num = Mathf.Lerp(mWndBg.gameObject.transform.localScale.y, mMoveHeight, Time.deltaTime * 10f);
			if (num - mMoveHeight < 1f)
			{
				num = mMoveHeight;
			}
			if (num > mMoveHeight)
			{
				mWndBg.gameObject.transform.localScale = new Vector3(localScale.x, num, localScale.z);
				return;
			}
			IsWndMove = false;
			mWndBg.gameObject.transform.localScale = new Vector3(localScale.x, mMoveHeight, localScale.z);
			return;
		}
		Vector3 localScale2 = mWndBg.gameObject.transform.localScale;
		float num2 = Mathf.Lerp(mWndBg.gameObject.transform.localScale.y, mWndBgHeight, Time.deltaTime * 10f);
		if (mWndBgHeight - num2 < 1f)
		{
			num2 = mWndBgHeight;
		}
		if (num2 < mWndBgHeight)
		{
			mWndBg.gameObject.transform.localScale = new Vector3(localScale2.x, num2, localScale2.z);
			return;
		}
		IsWndMove = false;
		mWndBg.gameObject.transform.localScale = new Vector3(localScale2.x, mWndBgHeight, localScale2.z);
		mBtnDown.SetActive(value: true);
		mBtnUp.SetActive(value: false);
		mBtnLock.SetActive(IsLock);
		mBtnUnLock.SetActive(!IsLock);
		mBtnClear.SetActive(value: true);
		mDrgPanelClider.enabled = true;
		mScrollBar.gameObject.SetActive(!IsHide);
	}

	private void BtnLockOnClick()
	{
		if (IsLock)
		{
			IsLock = false;
			mTopClider.enabled = true;
			mRightClider.enabled = true;
		}
	}

	private void BtnUnLockOnClick()
	{
		if (!IsLock)
		{
			IsLock = true;
			mTopClider.enabled = false;
			mRightClider.enabled = false;
		}
	}

	private void BtnClearOnClick()
	{
		ClearInputBox();
	}

	private void SetCurItemsLineWidth(int lineWidth)
	{
		if (m_CurChatItems != null && m_CurChatItems.Count > 0)
		{
			for (int i = 0; i < m_CurChatItems.Count; i++)
			{
				m_CurChatItems[i].SetLineWidth(lineWidth);
			}
		}
	}

	private CommonChatItem_N GetNewChatItem()
	{
		CommonChatItem_N commonChatItem_N;
		if (m_ChatItemsPool.Count > 0)
		{
			commonChatItem_N = m_ChatItemsPool.Dequeue();
			commonChatItem_N.gameObject.SetActive(value: true);
		}
		else
		{
			GameObject gameObject = UnityEngine.Object.Instantiate(m_ChatItemPrefab.gameObject);
			gameObject.transform.parent = m_ContentTable.transform;
			gameObject.transform.localPosition = Vector3.zero;
			gameObject.transform.localScale = Vector3.one;
			gameObject.transform.localRotation = Quaternion.identity;
			commonChatItem_N = gameObject.GetComponent<CommonChatItem_N>();
		}
		return commonChatItem_N;
	}

	private void RecoveryItems(int count)
	{
		if (m_CurChatItems != null && m_CurChatItems.Count >= count)
		{
			for (int i = 0; i < count; i++)
			{
				CommonChatItem_N commonChatItem_N = m_CurChatItems[i];
				commonChatItem_N.ResetItem();
				commonChatItem_N.gameObject.SetActive(value: false);
				m_ChatItemsPool.Enqueue(commonChatItem_N);
			}
			m_CurChatItems.RemoveRange(0, count);
		}
	}
}
