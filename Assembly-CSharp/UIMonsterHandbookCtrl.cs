using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using Pathea;
using UnityEngine;
using WhiteCat;

public class UIMonsterHandbookCtrl : UIBaseWidget
{
	[SerializeField]
	private UIListItem m_ListItemPrefab;

	[SerializeField]
	private UIGrid m_ListGrid;

	[SerializeField]
	private UIScrollBar m_ListScrollBar;

	[SerializeField]
	private UILabel m_NameLabel;

	[SerializeField]
	private UILabel m_SizeInfoLabel;

	[SerializeField]
	private UILabel m_DescriptionLabel;

	[SerializeField]
	private UIScrollBar m_DescriptionScrollBar;

	[SerializeField]
	private UIViewController m_UIViewCtrl;

	[SerializeField]
	private float m_CameraScaleFactor = 0.4f;

	[SerializeField]
	private UITexture m_CameraTxuture;

	private Queue<UIListItem> m_MsgListItemPool = new Queue<UIListItem>();

	private List<UIListItem> m_CurListItems = new List<UIListItem>();

	private UIListItem m_BackupSelectItem;

	private PeViewController m_PeViewCtrl;

	private int m_OpCameraStep;

	private Vector2 m_ViewWndSize;

	private Animator m_CurAnimator;

	private MovementField m_MovementField;

	private string m_CurIdleAnimName;

	private int m_CurIdleIndex;

	private int m_CurMonsterID;

	private GameObject m_CurMonsterGo;

	private void LateUpdate()
	{
		if (m_OpCameraStep == 1)
		{
			Bounds modelBounds = SpeciesViewStudio.GetModelBounds(m_CurMonsterGo);
			UpdateSizeInfo(modelBounds);
			SpeciesViewStudio.GetCanViewModelFullDistance(m_CurMonsterGo, m_PeViewCtrl.viewCam, m_ViewWndSize, out var distance, out var yaw);
			m_UIViewCtrl.SetCameraToBastView(modelBounds.center, distance, yaw, m_CameraScaleFactor);
			PlayAnimator();
			m_OpCameraStep = 2;
		}
		else if (m_OpCameraStep == 2)
		{
			m_PeViewCtrl.viewCam.enabled = true;
			m_OpCameraStep = 0;
		}
	}

	public override void OnCreate()
	{
		m_MsgListItemPool.Clear();
		m_CurListItems.Clear();
		MonsterHandbookData.AddMhEvent = (Action<int>)Delegate.Combine(MonsterHandbookData.AddMhEvent, new Action<int>(AddMhByData));
		m_PeViewCtrl = SpeciesViewStudio.CreateViewController(ViewControllerParam.DefaultSpicies);
		m_UIViewCtrl.Init(m_PeViewCtrl);
		m_CameraTxuture.mainTexture = m_PeViewCtrl.RenderTex;
		m_ViewWndSize = new Vector2(m_CameraTxuture.mainTexture.width, m_CameraTxuture.mainTexture.height);
	}

	protected override void InitWindow()
	{
		base.InitWindow();
		CheckGameMode();
	}

	public override void Show()
	{
		base.Show();
		ResetWnd();
		UpdateMhList();
	}

	protected override void OnHide()
	{
		base.OnHide();
		DestoryCurModel();
		RecycleMhListItem();
	}

	protected override void OnClose()
	{
		base.OnClose();
		DestoryCurModel();
		RecycleMhListItem();
	}

	private void ResetWnd()
	{
		m_OpCameraStep = 0;
		m_CurMonsterGo = null;
		m_CurMonsterID = -1;
		m_ListScrollBar.scrollValue = 0f;
		m_DescriptionScrollBar.scrollValue = 0f;
		m_BackupSelectItem = null;
		ResetAllLabel();
	}

	private void AddMhByData(int mhID)
	{
		if (null != this && base.gameObject.activeInHierarchy)
		{
			AddNewListItem(mhID);
			CurListItemOderByID();
		}
	}

	private void UpdateMhList()
	{
		if (MonsterHandbookData.ActiveMhDataID == null || MonsterHandbookData.ActiveMhDataID.Count <= 0)
		{
			return;
		}
		int i;
		for (i = 0; i < MonsterHandbookData.ActiveMhDataID.Count; i++)
		{
			if (!m_CurListItems.Any((UIListItem a) => a.ID == MonsterHandbookData.ActiveMhDataID[i]))
			{
				AddNewListItem(MonsterHandbookData.ActiveMhDataID[i]);
			}
		}
		CurListItemOderByID();
	}

	private void CurListItemOderByID()
	{
		if (m_CurListItems != null && m_CurListItems.Count > 0)
		{
			m_CurListItems = m_CurListItems.OrderBy((UIListItem item) => item.ID).ToList();
			for (int i = 0; i < m_CurListItems.Count; i++)
			{
				m_CurListItems[i].gameObject.name = i.ToString("D3") + "_ListItem";
			}
			m_ListGrid.repositionNow = true;
			m_CurListItems[0].Select();
		}
	}

	private void AddNewListItem(int msgID)
	{
		UIListItem newMsgListItem = GetNewMsgListItem();
		newMsgListItem.UpdateInfo(msgID, MonsterHandbookData.AllMonsterHandbookDataDic[msgID].Name);
		newMsgListItem.SelectEvent = MhListItemSelectEvent;
		m_CurListItems.Add(newMsgListItem);
	}

	private void MhListItemSelectEvent(UIListItem item)
	{
		if (!(item == m_BackupSelectItem))
		{
			if (null != m_BackupSelectItem)
			{
				m_BackupSelectItem.CancelSelect();
			}
			m_BackupSelectItem = item;
			if (MonsterHandbookData.ActiveMhDataID.Contains(item.ID))
			{
				MonsterHandbookData monsterHandbookData = MonsterHandbookData.AllMonsterHandbookDataDic[item.ID];
				SetNewModel(monsterHandbookData.ModelID);
				UpdateDescription(monsterHandbookData.Description);
				UpdateName(monsterHandbookData.Name);
			}
			else
			{
				ResetAllLabel();
			}
		}
	}

	private void ResetAllLabel()
	{
		UpdateName(string.Empty);
		UpdateDescription(string.Empty);
	}

	private void UpdateName(string name)
	{
		m_NameLabel.text = PELocalization.GetString(10019) + name;
	}

	private void UpdateSizeInfo(Bounds bounds)
	{
		m_SizeInfoLabel.text = $"{PELocalization.GetString(8000597)}{bounds.size.z:F2}\n{PELocalization.GetString(8000598)}{bounds.size.x:F2}\n{PELocalization.GetString(8000599)}{bounds.size.y:F2}";
	}

	private void UpdateDescription(string decription)
	{
		if (decription == "0" || string.IsNullOrEmpty(decription))
		{
			m_DescriptionLabel.gameObject.SetActive(value: false);
		}
		else
		{
			m_DescriptionLabel.gameObject.SetActive(value: true);
			decription = decription.Replace("<\\n>", "\n");
			m_DescriptionLabel.text = decription;
		}
		m_DescriptionScrollBar.scrollValue = 0f;
	}

	private void SetNewModel(int modelID)
	{
		if (modelID != m_CurMonsterID)
		{
			m_CurAnimator = null;
			m_MovementField = MovementField.None;
			DestoryCurModel();
			m_PeViewCtrl.viewCam.enabled = false;
			m_CurMonsterGo = SpeciesViewStudio.LoadMonsterModelByID(modelID, ref m_MovementField);
			m_CurMonsterID = modelID;
			if (!(null == m_CurMonsterGo))
			{
				m_CurAnimator = m_CurMonsterGo.GetComponent<Animator>();
				m_PeViewCtrl.SetTarget(m_CurMonsterGo.transform);
				m_OpCameraStep = 1;
			}
		}
	}

	private void PlayAnimator()
	{
		if (null != m_CurAnimator)
		{
			m_CurIdleAnimName = ((m_MovementField != MovementField.Sky) ? "normal_leisure" : "normalsky_leisure");
			m_CurIdleIndex = 0;
			if (m_MovementField == MovementField.Sky)
			{
				m_CurAnimator.SetBool("Fly", value: true);
			}
			m_CurAnimator.SetTrigger(m_CurIdleAnimName + m_CurIdleIndex);
			AnimatorClipInfo[] currentAnimatorClipInfo = m_CurAnimator.GetCurrentAnimatorClipInfo(0);
			if (currentAnimatorClipInfo != null && currentAnimatorClipInfo.Length > 0)
			{
				currentAnimatorClipInfo[0].clip.SampleAnimation(m_CurAnimator.gameObject, 0f);
			}
			StopCoroutine("PlayIdelAnimaIterator");
			StartCoroutine("PlayIdelAnimaIterator");
		}
	}

	private IEnumerator PlayIdelAnimaIterator()
	{
		float startTime = Time.realtimeSinceStartup;
		float waitTime = UnityEngine.Random.Range(6, 12);
		while (null != m_CurAnimator)
		{
			if (Time.realtimeSinceStartup - startTime >= waitTime && !m_CurAnimator.GetBool("Leisureing"))
			{
				if (++m_CurIdleIndex > 2)
				{
					m_CurIdleIndex = 0;
				}
				m_CurAnimator.SetTrigger(m_CurIdleAnimName + ++m_CurIdleIndex);
				startTime = Time.realtimeSinceStartup;
			}
			yield return null;
		}
	}

	private void DestoryCurModel()
	{
		UnityEngine.Object.Destroy(m_CurMonsterGo);
		m_CurMonsterID = -1;
	}

	private UIListItem GetNewMsgListItem()
	{
		UIListItem uIListItem = null;
		if (m_MsgListItemPool.Count > 0)
		{
			uIListItem = m_MsgListItemPool.Dequeue();
			uIListItem.ResetItem();
			uIListItem.gameObject.SetActive(value: true);
		}
		else
		{
			GameObject gameObject = UnityEngine.Object.Instantiate(m_ListItemPrefab.gameObject);
			gameObject.transform.parent = m_ListGrid.gameObject.transform;
			gameObject.transform.localRotation = Quaternion.identity;
			gameObject.transform.localScale = Vector3.one;
			gameObject.transform.localPosition = new Vector3(0f, 0f, 0f);
			uIListItem = gameObject.GetComponent<UIListItem>();
		}
		return uIListItem;
	}

	private void RecycleMhListItem()
	{
		if (m_CurListItems != null && m_CurListItems.Count > 0)
		{
			for (int i = 0; i < m_CurListItems.Count; i++)
			{
				m_CurListItems[i].gameObject.SetActive(value: false);
				m_MsgListItemPool.Enqueue(m_CurListItems[i]);
			}
			m_CurListItems.Clear();
		}
	}

	private void CheckGameMode()
	{
		if (!PeGameMgr.IsAdventure)
		{
			return;
		}
		int[] array = MonsterHandbookData.AllMonsterHandbookDataDic.Keys.ToArray();
		for (int i = 0; i < array.Length; i++)
		{
			if (!MonsterHandbookData.ActiveMhDataID.Contains(array[i]))
			{
				MonsterHandbookData.ActiveMhDataID.Add(array[i]);
			}
		}
	}
}
