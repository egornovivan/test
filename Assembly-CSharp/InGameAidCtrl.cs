using System.Collections;
using System.Collections.Generic;
using System.Linq;
using Pathea;
using UnityEngine;

public class InGameAidCtrl : MonoBehaviour
{
	[SerializeField]
	private UIButton m_HideAndShowBtn;

	[SerializeField]
	private Transform m_ContentParent;

	[SerializeField]
	private UIPanel m_Panel;

	[SerializeField]
	private UIScrollBar m_ScrollBar;

	[SerializeField]
	private GameObject m_LabelPrefab;

	[SerializeField]
	private TweenPosition m_Tween;

	[SerializeField]
	private BoxCollider m_MouseHoverCollider;

	[SerializeField]
	private int m_ShowCount = 2;

	[SerializeField]
	private float m_PaddingY = 10f;

	[SerializeField]
	private float m_ItemShowTime = 10f;

	[SerializeField]
	private float m_HideUITime = 10f;

	[SerializeField]
	private Color m_ReadColor = new Color(0.5f, 0.5f, 0.5f);

	private bool m_FirstShow;

	private bool m_Show;

	private bool m_ShowFinish;

	private bool m_NeedReposition;

	private List<int> m_CurShow = new List<int>();

	private List<GameObject> m_CurShowLabelGos = new List<GameObject>();

	private Queue<int> m_WaitShowQueue = new Queue<int>();

	private Queue<GameObject> m_PrefabPools = new Queue<GameObject>();

	private bool m_EnableUI;

	private bool m_PlayQueueing;

	private float m_StartWaitHideTime;

	private bool m_MouseHover;

	private void Start()
	{
		Init();
		if (PeGameMgr.sceneMode == PeGameMgr.ESceneMode.Story && m_EnableUI)
		{
			EnableUI(show: true);
			FirstShow();
		}
		else
		{
			EnableUI(show: false);
		}
	}

	private void Init()
	{
		m_EnableUI = SystemSettingData.Instance.AndyGuidance;
		m_FirstShow = false;
		m_Show = InGameAidData.ShowInGameAidCtrl;
		m_ShowFinish = false;
		m_NeedReposition = false;
		m_CurShow.Clear();
		m_CurShowLabelGos.Clear();
		m_WaitShowQueue.Clear();
		m_PrefabPools.Clear();
		m_PlayQueueing = false;
		m_StartWaitHideTime = 0f;
		m_MouseHover = false;
		for (int i = 0; i < m_ContentParent.childCount; i++)
		{
			Transform child = m_ContentParent.GetChild(i);
			if (null != child && (bool)child.gameObject)
			{
				child.gameObject.SetActive(value: false);
				Object.Destroy(child.gameObject);
			}
		}
		m_Tween.onFinished = delegate
		{
			TweenFinish();
		};
		UIEventListener.Get(m_HideAndShowBtn.gameObject).onClick = delegate
		{
			PlayTween(!m_Show);
		};
		UIEventListener.Get(m_MouseHoverCollider.gameObject).onHover = delegate(GameObject go, bool isHover)
		{
			m_MouseHover = isHover;
		};
	}

	private void Update()
	{
		if (!m_FirstShow)
		{
			FirstShow();
		}
		if (PeGameMgr.sceneMode == PeGameMgr.ESceneMode.Story && SystemSettingData.Instance.AndyGuidance != m_EnableUI)
		{
			m_EnableUI = SystemSettingData.Instance.AndyGuidance;
			EnableUI(m_EnableUI);
		}
	}

	private void EnableUI(bool show)
	{
		if (null != m_Tween)
		{
			m_Tween.gameObject.SetActive(show);
			m_Tween.enabled = false;
		}
		m_HideAndShowBtn.gameObject.SetActive(show);
		if (show)
		{
			m_HideAndShowBtn.isEnabled = true;
			RepositionVertical();
		}
	}

	private void AddNewMsg(int id)
	{
		if (!m_WaitShowQueue.Contains(id))
		{
			m_WaitShowQueue.Enqueue(id);
			if (!m_Show)
			{
				PlayTween(show: true);
			}
			if (!m_PlayQueueing)
			{
				StartCoroutine("ShowTipsIterator");
			}
		}
	}

	private void SetTipIsNew(GameObject go, bool isNew, bool showLine)
	{
		UILabel[] componentsInChildren = go.GetComponentsInChildren<UILabel>(includeInactive: true);
		if (componentsInChildren != null && componentsInChildren.Count() > 0)
		{
			componentsInChildren[0].color = ((!isNew) ? m_ReadColor : Color.white);
		}
		UISprite[] componentsInChildren2 = go.GetComponentsInChildren<UISprite>(includeInactive: true);
		if (componentsInChildren2 != null && componentsInChildren2.Count() > 0)
		{
			componentsInChildren2[0].gameObject.SetActive(showLine);
		}
	}

	private void ShowAllMsg()
	{
		if (InGameAidData.CurShowIDs.Count > m_ShowCount)
		{
			InGameAidData.CurShowIDs.RemoveRange(0, InGameAidData.CurShowIDs.Count - m_ShowCount);
		}
		for (int i = 0; i < InGameAidData.CurShowIDs.Count; i++)
		{
			AddItemByID(InGameAidData.CurShowIDs[i]);
		}
		if (!m_PlayQueueing)
		{
			StartCoroutine("ShowTipsIterator");
		}
	}

	private IEnumerator ShowTipsIterator()
	{
		m_PlayQueueing = true;
		float startTime = 0f;
		bool inWaitHide = false;
		while (true)
		{
			if (Time.realtimeSinceStartup - startTime >= m_ItemShowTime)
			{
				if (m_WaitShowQueue.Count > 0)
				{
					AddItemByID(m_WaitShowQueue.Dequeue());
					startTime = Time.realtimeSinceStartup;
					inWaitHide = false;
				}
				else if (m_MouseHover)
				{
					if (inWaitHide)
					{
						inWaitHide = false;
					}
				}
				else if (!inWaitHide)
				{
					inWaitHide = true;
					m_StartWaitHideTime = Time.realtimeSinceStartup;
				}
			}
			if (inWaitHide && m_ShowFinish && Time.realtimeSinceStartup - m_StartWaitHideTime >= m_HideUITime)
			{
				if (m_Show)
				{
					PlayTween(show: false);
				}
				inWaitHide = false;
			}
			yield return null;
		}
	}

	private void RepositionVertical()
	{
		if (m_CurShowLabelGos == null || m_CurShowLabelGos.Count <= 0)
		{
			return;
		}
		Bounds bounds = default(Bounds);
		Vector3 zero = Vector3.zero;
		Vector3 zero2 = Vector3.zero;
		SetTipIsNew(m_CurShowLabelGos[0], m_CurShowLabelGos.Count == 1, showLine: false);
		if (m_CurShowLabelGos.Count > 1)
		{
			for (int i = 1; i < m_CurShowLabelGos.Count; i++)
			{
				SetTipIsNew(m_CurShowLabelGos[i], i == m_CurShowLabelGos.Count - 1, showLine: true);
			}
		}
		for (int j = 0; j < m_CurShowLabelGos.Count; j++)
		{
			Transform transform = m_CurShowLabelGos[j].transform;
			transform.localPosition = zero - zero2;
			zero2.y = m_PaddingY;
			bounds = NGUIMath.CalculateRelativeWidgetBounds(transform);
			zero.y = transform.localPosition.y - bounds.size.y;
		}
		Bounds bounds2 = NGUIMath.CalculateRelativeWidgetBounds(m_ContentParent);
		Vector3 localPosition = m_ContentParent.transform.localPosition;
		float num = m_Panel.clipRange.w - m_Panel.clipSoftness.y * 2f;
		m_Panel.transform.localPosition = Vector3.zero;
		Vector4 clipRange = m_Panel.clipRange;
		clipRange.x = 0f;
		clipRange.y = 0f - (m_Panel.clipRange.w - m_Panel.clipSoftness.y) * 0.5f;
		m_Panel.clipRange = clipRange;
		if (bounds2.size.y - num > 0f)
		{
			localPosition.y = bounds2.size.y - bounds.size.y;
		}
		else
		{
			localPosition.y = 0f;
		}
		m_ContentParent.transform.localPosition = localPosition;
	}

	private void AddItemByID(int id)
	{
		if (InGameAidData.CurShowIDs.Count > m_ShowCount && m_ShowCount > 0 && InGameAidData.CurShowIDs.Count > 0 && m_CurShowLabelGos.Count > 0)
		{
			InGameAidData.CurShowIDs.RemoveAt(0);
			GameObject gameObject = m_CurShowLabelGos[0];
			SetTipIsNew(gameObject, isNew: true, showLine: true);
			gameObject.SetActive(value: false);
			m_CurShowLabelGos.RemoveAt(0);
			m_PrefabPools.Enqueue(gameObject);
		}
		if (!InGameAidData.AllData.ContainsKey(id) || m_CurShow.Contains(id))
		{
			return;
		}
		int countentID = InGameAidData.AllData[id].CountentID;
		GameObject gameObject2 = ((m_PrefabPools.Count <= 0) ? GetNewLabel() : m_PrefabPools.Dequeue());
		if (!(null == gameObject2))
		{
			gameObject2.SetActive(value: true);
			UILabel[] componentsInChildren = gameObject2.GetComponentsInChildren<UILabel>(includeInactive: true);
			if (componentsInChildren != null || componentsInChildren.Count() > 0)
			{
				componentsInChildren[0].text = PELocalization.GetString(countentID);
				componentsInChildren[0].MakePixelPerfect();
				m_CurShowLabelGos.Add(gameObject2);
				RepositionVertical();
				m_CurShow.Add(id);
			}
		}
	}

	private void FirstShow()
	{
		if (!m_FirstShow && PeGameMgr.sceneMode == PeGameMgr.ESceneMode.Story && HasContent() && m_EnableUI)
		{
			ShowAllMsg();
			PlayTween(m_Show);
			InGameAidData.AddEvent += AddNewMsg;
			m_FirstShow = true;
		}
	}

	private void PlayTween(bool show)
	{
		if (null != m_Tween)
		{
			InGameAidData.ShowInGameAidCtrl = show;
			m_Show = show;
			m_Tween.Play(show);
			m_HideAndShowBtn.isEnabled = false;
			if (show)
			{
				StartCoroutine(PlayOpenAudio(Time.realtimeSinceStartup, m_Tween.duration * 0.5f));
			}
		}
	}

	private void TweenFinish()
	{
		m_ShowFinish = m_Show;
		if (m_ShowFinish)
		{
			m_StartWaitHideTime = Time.realtimeSinceStartup;
		}
		m_HideAndShowBtn.isEnabled = true;
		m_HideAndShowBtn.transform.rotation = Quaternion.Euler((!m_Show) ? new Vector3(0f, 0f, 180f) : Vector3.zero);
	}

	private GameObject GetNewLabel()
	{
		GameObject gameObject = Object.Instantiate(m_LabelPrefab);
		if (null != gameObject)
		{
			gameObject.transform.parent = m_ContentParent;
			gameObject.transform.localPosition = Vector3.zero;
			gameObject.transform.localScale = Vector3.one;
			gameObject.transform.localRotation = Quaternion.identity;
			return gameObject;
		}
		return null;
	}

	private bool HasContent()
	{
		return InGameAidData.CurShowIDs.Count > 0;
	}

	private IEnumerator PlayOpenAudio(float startTime, float waitTime)
	{
		while (Time.realtimeSinceStartup - startTime < waitTime)
		{
			yield return null;
		}
		GameUI.Instance.PlayWndOpenAudioEffect();
	}
}
