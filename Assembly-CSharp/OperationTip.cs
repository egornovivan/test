using System.Collections.Generic;
using UnityEngine;

public class OperationTip : MonoBehaviour
{
	private static OperationTip mInstance;

	public Camera uiCamera;

	[SerializeField]
	private UISprite mOpSprite;

	[SerializeField]
	private Transform m_OpTipTrans;

	[SerializeField]
	private UILabel m_TipLabel;

	[SerializeField]
	private UISprite m_TipBg;

	[SerializeField]
	private TweenScale m_TipScaleTween;

	[SerializeField]
	private float m_BorderY = 10f;

	[SerializeField]
	private int m_ShopTipCount = 5;

	[SerializeField]
	private float m_WaitShowTime = 1f;

	private Dictionary<MouseOpMgr.MouseOpCursor, int> m_ShopTipDic = new Dictionary<MouseOpMgr.MouseOpCursor, int>();

	private bool m_TipShow;

	private MouseOpMgr.MouseOpCursor m_WaitBackupType;

	private MouseOpMgr.MouseOpCursor m_ShowBackupType;

	private float m_WaitStartTime;

	private Transform mTrans;

	private CursorHandler ch;

	private string mBag;

	private string mAxe;

	private string mNouseTalk;

	private string mHand;

	private string mRide;

	private string mDefault;

	private bool mIsShow;

	public static OperationTip Instance => mInstance;

	private void Start()
	{
		mTrans = base.transform;
		if (uiCamera == null)
		{
			uiCamera = NGUITools.FindCameraForLayer(base.gameObject.layer);
		}
		ch = new CursorHandler();
		ch.Type = CursorState.EType.None;
		m_TipScaleTween.onFinished = TipTweenFinish;
		mBag = "mouse_get";
		mAxe = "mouse_axe";
		mNouseTalk = "mouse_talk";
		mHand = "mouse_help";
		mRide = "mouse_ride";
		mDefault = "sighting_3";
	}

	private void EnableCursor(string IconName)
	{
		mOpSprite.spriteName = IconName;
		mOpSprite.MakePixelPerfect();
		CursorState.self.SetHandler(ch);
		mIsShow = true;
	}

	private void DisableCursor()
	{
		mOpSprite.spriteName = "null";
		mOpSprite.MakePixelPerfect();
		CursorState.self.SetHandler(null);
		mIsShow = false;
		PlayTipHideTween();
	}

	private void Update()
	{
		if (!PeInput.Get(PeInput.LogicFunction.Item_Drag) && !(UICamera.hoveredObject != null))
		{
			UpdateMouseIcon();
		}
		if ((UICamera.hoveredObject != null || PeInput.Get(PeInput.LogicFunction.Item_Drag)) && mIsShow)
		{
			DisableCursor();
		}
		if (PeInput.Get(PeInput.LogicFunction.InteractWithItem) || Input.GetMouseButtonDown(0))
		{
			PlayTipHideTween();
		}
		if (!(mOpSprite.atlas != null))
		{
			return;
		}
		Vector3 mousePosition = Input.mousePosition;
		if (uiCamera != null)
		{
			mousePosition.x = Mathf.Clamp01(mousePosition.x / (float)Screen.width);
			mousePosition.y = Mathf.Clamp01(mousePosition.y / (float)Screen.height);
			mTrans.position = uiCamera.ViewportToWorldPoint(mousePosition);
			if (uiCamera.orthographic)
			{
				mTrans.localPosition = NGUIMath.ApplyHalfPixelOffset(mTrans.localPosition, mTrans.localScale);
			}
		}
	}

	private void LateUpdate()
	{
	}

	private void UpdateMouseIcon()
	{
		if (!(Singleton<MouseOpMgr>.Instance != null))
		{
			return;
		}
		switch (Singleton<MouseOpMgr>.Instance.currentState)
		{
		case MouseOpMgr.MouseOpCursor.Gather:
			EnableCursor(mBag);
			break;
		case MouseOpMgr.MouseOpCursor.Fell:
			EnableCursor(mAxe);
			break;
		case MouseOpMgr.MouseOpCursor.NPCTalk:
			EnableCursor(mNouseTalk);
			break;
		case MouseOpMgr.MouseOpCursor.PickUpItem:
			EnableCursor(mBag);
			break;
		case MouseOpMgr.MouseOpCursor.WareHouse:
			EnableCursor(mBag);
			break;
		case MouseOpMgr.MouseOpCursor.LootCorpse:
			EnableCursor(mBag);
			break;
		case MouseOpMgr.MouseOpCursor.Hand:
			EnableCursor(mHand);
			break;
		case MouseOpMgr.MouseOpCursor.KickStarter:
			EnableCursor(mNouseTalk);
			break;
		case MouseOpMgr.MouseOpCursor.Ride:
			EnableCursor(mRide);
			break;
		case MouseOpMgr.MouseOpCursor.Null:
			if (SystemSettingData.Instance.FirstPersonCtrl || !SystemSettingData.Instance.mMMOControlType)
			{
				if (PeCamera.cursorLocked && UISightingTelescope.Instance.CurType == UISightingTelescope.SightingType.Null)
				{
					EnableCursor(mDefault);
				}
				else
				{
					DisableCursor();
				}
			}
			else
			{
				DisableCursor();
			}
			break;
		}
		ShowTipLabel(Singleton<MouseOpMgr>.Instance.currentState);
	}

	private void ShowTipLabel(MouseOpMgr.MouseOpCursor opType)
	{
		if (!SystemSettingData.Instance.MouseStateTip)
		{
			PlayTipHideTween();
		}
		else if (opType == MouseOpMgr.MouseOpCursor.Null)
		{
			PlayTipHideTween();
		}
		else if (m_WaitBackupType != opType)
		{
			m_WaitStartTime = Time.realtimeSinceStartup;
			m_WaitBackupType = opType;
		}
		else
		{
			if (Time.realtimeSinceStartup - m_WaitStartTime < m_WaitShowTime || m_ShowBackupType == opType || (m_ShopTipDic.ContainsKey(opType) && (!m_ShopTipDic.ContainsKey(opType) || m_ShopTipDic[opType] >= m_ShopTipCount)))
			{
				return;
			}
			int num = -1;
			switch (opType)
			{
			case MouseOpMgr.MouseOpCursor.Gather:
				num = 8000678;
				break;
			case MouseOpMgr.MouseOpCursor.Fell:
				num = 8000679;
				break;
			case MouseOpMgr.MouseOpCursor.NPCTalk:
				num = -1;
				break;
			case MouseOpMgr.MouseOpCursor.PickUpItem:
				num = 8000678;
				break;
			case MouseOpMgr.MouseOpCursor.WareHouse:
				num = 8000678;
				break;
			case MouseOpMgr.MouseOpCursor.LootCorpse:
				num = 8000678;
				break;
			case MouseOpMgr.MouseOpCursor.Hand:
				num = 8000680;
				break;
			case MouseOpMgr.MouseOpCursor.Ride:
				num = 8000983;
				break;
			}
			if (num != -1)
			{
				PlayTipShowTween(PELocalization.GetString(num));
				m_ShowBackupType = opType;
				if (m_ShopTipDic.ContainsKey(opType))
				{
					Dictionary<MouseOpMgr.MouseOpCursor, int> shopTipDic;
					Dictionary<MouseOpMgr.MouseOpCursor, int> dictionary = (shopTipDic = m_ShopTipDic);
					MouseOpMgr.MouseOpCursor key;
					MouseOpMgr.MouseOpCursor key2 = (key = opType);
					int num2 = shopTipDic[key];
					dictionary[key2] = num2 + 1;
				}
				else
				{
					m_ShopTipDic.Add(opType, 1);
				}
			}
		}
	}

	private void PlayTipShowTween(string content)
	{
		m_TipLabel.text = content;
		m_TipLabel.MakePixelPerfect();
		float num = m_TipLabel.relativeSize.y * (float)m_TipLabel.font.size;
		Vector3 localScale = m_TipBg.transform.localScale;
		localScale.y = num + m_BorderY * 2f;
		localScale.x = m_TipLabel.relativeSize.x * (float)m_TipLabel.font.size + 10f;
		m_TipBg.transform.localScale = localScale;
		if (!m_TipShow)
		{
			m_TipScaleTween.Play(forward: true);
		}
	}

	private void PlayTipHideTween()
	{
		if (m_TipShow)
		{
			m_TipScaleTween.Play(forward: false);
		}
	}

	private void TipTweenFinish(UITweener tween)
	{
		m_TipShow = !m_TipShow;
		if (!m_TipShow)
		{
			m_ShowBackupType = MouseOpMgr.MouseOpCursor.Null;
			m_WaitBackupType = MouseOpMgr.MouseOpCursor.Null;
		}
	}
}
