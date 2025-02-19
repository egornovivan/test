using System;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

public class UIRadioCtrl : UIBaseWidget
{
	[SerializeField]
	private UIButton m_StartOrPauseBtn;

	[SerializeField]
	private UISprite m_StartSprite;

	[SerializeField]
	private UISprite m_StopSprite;

	[SerializeField]
	private UIButton m_NextBtn;

	[SerializeField]
	private UIButton m_PreviousBtn;

	[SerializeField]
	private UISlider m_PlayingSlider;

	[SerializeField]
	private UILabel m_PlayingLb;

	[SerializeField]
	private UILabel m_CurSoundNameLb;

	[SerializeField]
	private UISlider m_VolumeSlider;

	[SerializeField]
	private GameObject m_ListItemPrefab;

	[SerializeField]
	private UIGrid m_ListParentGrid;

	[SerializeField]
	private UIPanel m_ListClipPanel;

	[SerializeField]
	private UIScrollBar m_ContentScrollBar;

	[SerializeField]
	private UIListItem m_BackupSelectItem;

	[SerializeField]
	private UITexture m_UISpectrumTex;

	[SerializeField]
	private UILabel m_Shape0Lb;

	[SerializeField]
	private UILabel m_Shape1Lb;

	[SerializeField]
	private UILabel m_Shape2Lb;

	[SerializeField]
	private UIPopupList m_PlayModePL;

	[SerializeField]
	private int m_SpectrumWidth = 600;

	[SerializeField]
	private int m_SpectrumHeight = 200;

	[SerializeField]
	private Color32 m_SpectrumTopCol;

	[SerializeField]
	private Color32 m_SpectrumBottomCol;

	[SerializeField]
	private int m_SpectrumXGridCount = 20;

	[SerializeField]
	private int m_SpectrumYGridCount = 15;

	[SerializeField]
	private int SpectrumGridBorderX = 5;

	[SerializeField]
	private int SpectrumGridBorderY = 5;

	[SerializeField]
	private int m_SampleLength = 40;

	[SerializeField]
	private float m_LeapInterval = 0.1f;

	[SerializeField]
	private UICheckbox m_OpenBgMusicCK;

	private Color32[] m_RandomCols = new Color32[9]
	{
		new Color32(byte.MaxValue, byte.MaxValue, byte.MaxValue, byte.MaxValue),
		new Color32(75, 210, byte.MaxValue, byte.MaxValue),
		new Color32(122, 231, 252, byte.MaxValue),
		new Color32(103, 248, 95, byte.MaxValue),
		new Color32(byte.MaxValue, byte.MaxValue, 162, byte.MaxValue),
		new Color32(158, 159, 249, byte.MaxValue),
		new Color32(217, 174, byte.MaxValue, byte.MaxValue),
		new Color32(250, 165, 250, byte.MaxValue),
		new Color32(244, 173, 81, byte.MaxValue)
	};

	private Color32 m_TopNextCol;

	private Color32 m_BottomNextCol;

	private float m_LerpT;

	private float m_StartTime;

	private GraphPlotter.GraphShapeType mGraphShapeType;

	private Texture2D m_SpectrumTex2d;

	private Color32[] m_SpectrumTexClos;

	private GraphPlotter m_Plotter;

	private bool m_UpdateSpectrum;

	private float[] m_SampleData;

	private float[] m_PlotData;

	private Queue<UIListItem> m_ListItemPools;

	private List<UIListItem> m_CurListItems;

	private bool m_UpdatePlayProgress;

	private Dictionary<string, RadioManager.SoundPlayMode> m_PlayModeDic;

	private RadioManager.SoundPlayState mPlayState;

	private RadioManager.SoundPlayState m_PlayState
	{
		get
		{
			return mPlayState;
		}
		set
		{
			if (mPlayState == value)
			{
				return;
			}
			mPlayState = value;
			switch (value)
			{
			case RadioManager.SoundPlayState.Playing:
				m_StopSprite.enabled = true;
				m_StartSprite.enabled = false;
				m_StartOrPauseBtn.tweenTarget = m_StopSprite.gameObject;
				InitSpectrumArray();
				m_UpdateSpectrum = true;
				break;
			case RadioManager.SoundPlayState.Stop:
			case RadioManager.SoundPlayState.Pause:
				m_StopSprite.enabled = false;
				m_StartSprite.enabled = true;
				m_StartOrPauseBtn.tweenTarget = m_StartSprite.gameObject;
				m_UpdateSpectrum = false;
				ResetSpectrumArray();
				break;
			}
			if (value == RadioManager.SoundPlayState.Playing || value == RadioManager.SoundPlayState.Pause)
			{
				if (!m_UISpectrumTex.enabled)
				{
					m_UISpectrumTex.enabled = true;
				}
			}
			else
			{
				if (m_UISpectrumTex.enabled)
				{
					m_UISpectrumTex.enabled = false;
				}
				m_PlayingSlider.sliderValue = 0f;
				m_PlayingLb.text = "--:--";
			}
			m_CurSoundNameLb.text = RadioManager.Instance.CurSoundInfo.Name;
		}
	}

	protected override void InitWindow()
	{
		base.InitWindow();
		base.SelfWndType = UIEnum.WndType.Radio;
		RadioManager.Instance.Init();
		Init();
	}

	public override void Show()
	{
		base.Show();
		RadioManager.Instance.RefreshSoundsList();
		LoadSoundsList();
		m_BackupSelectItem = null;
		UpdateSelectItem();
		RadioManager instance = RadioManager.Instance;
		instance.UpdateSelectItemEvent = (Action)Delegate.Combine(instance.UpdateSelectItemEvent, new Action(UpdateSelectItem));
		RadioManager instance2 = RadioManager.Instance;
		instance2.PlayErrorEvent = (Action<int>)Delegate.Combine(instance2.PlayErrorEvent, new Action<int>(PlayErrorEvent));
	}

	protected override void OnHide()
	{
		base.OnHide();
		RecoveryListItem();
		RadioManager instance = RadioManager.Instance;
		instance.UpdateSelectItemEvent = (Action)Delegate.Remove(instance.UpdateSelectItemEvent, new Action(UpdateSelectItem));
		RadioManager instance2 = RadioManager.Instance;
		instance2.PlayErrorEvent = (Action<int>)Delegate.Remove(instance2.PlayErrorEvent, new Action<int>(PlayErrorEvent));
	}

	private void Update()
	{
		m_PlayState = RadioManager.Instance.PlayState;
		if (m_PlayState == RadioManager.SoundPlayState.Playing || m_PlayState == RadioManager.SoundPlayState.Pause || (m_PlayState == RadioManager.SoundPlayState.Stop && !m_UpdatePlayProgress))
		{
			float totalTime = RadioManager.Instance.TotalTime;
			float num = ((!m_UpdatePlayProgress) ? (Mathf.Clamp01(m_PlayingSlider.sliderValue) * totalTime) : RadioManager.Instance.CurTime);
			if (m_UpdatePlayProgress)
			{
				m_PlayingSlider.sliderValue = ((!(totalTime <= 0f)) ? (num / totalTime) : 0f);
			}
			if (totalTime < 3600f)
			{
				m_PlayingLb.text = $"{(int)num / 60:D2}:{(int)num % 60:D2}/{(int)totalTime / 60:D2}:{(int)totalTime % 60:D2}";
			}
			else
			{
				m_PlayingLb.text = $"{(int)num / 3600:D2}:{(int)num / 60:D2}:{(int)num % 60:D2}/{(int)totalTime / 3600:D2}:{(int)totalTime / 60:D2}:{(int)totalTime % 60:D2}";
			}
		}
		PlotSpectrum();
	}

	private void Init()
	{
		SetGraphShapeType(GraphPlotter.GraphShapeType.Grid);
		m_UpdateSpectrum = false;
		m_PlayState = RadioManager.SoundPlayState.Stop;
		m_LerpT = 0f;
		m_StartTime = Time.realtimeSinceStartup;
		m_Plotter = new GraphPlotter();
		m_Plotter.TextureWidth = m_SpectrumWidth;
		m_Plotter.TextureHeight = m_SpectrumHeight;
		m_TopNextCol = m_RandomCols[0];
		m_BottomNextCol = m_RandomCols[1];
		m_SpectrumTopCol = m_TopNextCol;
		m_SpectrumBottomCol = m_BottomNextCol;
		m_PlayingSlider.sliderValue = 0f;
		m_VolumeSlider.sliderValue = 1f;
		m_UpdatePlayProgress = true;
		m_PlayModePL.items.Clear();
		m_PlayModeDic = new Dictionary<string, RadioManager.SoundPlayMode>();
		m_PlayModeDic[PELocalization.GetString(8000972)] = RadioManager.SoundPlayMode.Single;
		m_PlayModeDic[PELocalization.GetString(8000973)] = RadioManager.SoundPlayMode.SingleLoop;
		m_PlayModeDic[PELocalization.GetString(8000974)] = RadioManager.SoundPlayMode.Order;
		m_PlayModeDic[PELocalization.GetString(8000975)] = RadioManager.SoundPlayMode.ListLoop;
		m_PlayModeDic[PELocalization.GetString(8000976)] = RadioManager.SoundPlayMode.Random;
		m_PlayModePL.items.AddRange(m_PlayModeDic.Keys.ToArray());
		m_PlayModePL.selection = PELocalization.GetString(8000975);
		m_PlayModePL.onSelectionChange = delegate(string item)
		{
			if (m_PlayModeDic.ContainsKey(item))
			{
				RadioManager.Instance.PlayMode = m_PlayModeDic[item];
			}
		};
		string @string = PELocalization.GetString(8000970);
		m_Shape0Lb.text = $"{@string} 1";
		m_Shape1Lb.text = $"{@string} 2";
		m_Shape2Lb.text = $"{@string} 3";
		m_ListItemPools = new Queue<UIListItem>();
		m_CurListItems = new List<UIListItem>();
		if (null != m_UISpectrumTex)
		{
			m_SpectrumTex2d = new Texture2D(m_SpectrumWidth, m_SpectrumHeight, TextureFormat.ARGB32, mipmap: false);
			m_UISpectrumTex.transform.localScale = new Vector3(m_SpectrumWidth, m_SpectrumHeight, 1f);
			m_SpectrumTex2d.wrapMode = TextureWrapMode.Clamp;
			m_SpectrumTex2d.filterMode = FilterMode.Point;
			m_SpectrumTex2d.anisoLevel = 0;
			m_UISpectrumTex.mainTexture = m_SpectrumTex2d;
			m_SpectrumTexClos = m_SpectrumTex2d.GetPixels32();
		}
		UIEventListener.Get(m_StartOrPauseBtn.gameObject).onClick = delegate
		{
			if (null != RadioManager.Instance)
			{
				if (m_PlayState == RadioManager.SoundPlayState.Playing)
				{
					RadioManager.Instance.PauseCurSound();
				}
				else
				{
					RadioManager.Instance.ContinueCurSound();
				}
			}
		};
		UIEventListener.Get(m_NextBtn.gameObject).onClick = delegate
		{
			if (null != RadioManager.Instance)
			{
				RadioManager.Instance.PlayNextSound();
			}
		};
		UIEventListener.Get(m_PreviousBtn.gameObject).onClick = delegate
		{
			if (null != RadioManager.Instance)
			{
				RadioManager.Instance.PlayPreviousSounds();
			}
		};
		UISlider volumeSlider = m_VolumeSlider;
		volumeSlider.onValueChange = (UISlider.OnValueChange)Delegate.Combine(volumeSlider.onValueChange, (UISlider.OnValueChange)delegate
		{
			if (null != RadioManager.Instance)
			{
				RadioManager.Instance.SetVolume(m_VolumeSlider.sliderValue);
			}
		});
		UIEventListener uIEventListener = UIEventListener.Get(m_PlayingSlider.gameObject);
		uIEventListener.onPress = (UIEventListener.BoolDelegate)Delegate.Combine(uIEventListener.onPress, (UIEventListener.BoolDelegate)delegate(GameObject go, bool isPress)
		{
			if (isPress)
			{
				m_UpdatePlayProgress = false;
			}
			else
			{
				RadioManager.Instance.SetTime(Mathf.Clamp01(m_PlayingSlider.sliderValue) * RadioManager.Instance.TotalTime);
				m_UpdatePlayProgress = true;
			}
		});
		UIEventListener uIEventListener2 = UIEventListener.Get(m_PlayingSlider.thumb.gameObject);
		uIEventListener2.onPress = (UIEventListener.BoolDelegate)Delegate.Combine(uIEventListener2.onPress, (UIEventListener.BoolDelegate)delegate(GameObject go, bool isPress)
		{
			if (isPress)
			{
				m_UpdatePlayProgress = false;
			}
			else
			{
				RadioManager.Instance.SetTime(Mathf.Clamp01(m_PlayingSlider.sliderValue) * RadioManager.Instance.TotalTime);
				m_UpdatePlayProgress = true;
			}
		});
		m_OpenBgMusicCK.startsChecked = true;
		m_OpenBgMusicCK.onStateChange = delegate(bool isCheck)
		{
			RadioManager.Instance.SetBgMusicState(isCheck);
		};
		RadioManager.Instance.PlayErrorEvent = null;
	}

	private void LoadSoundsList()
	{
		List<RadioManager.RadioFileInfo> soundsInfoList = RadioManager.Instance.SoundsInfoList;
		if (soundsInfoList == null || soundsInfoList.Count <= 0)
		{
			return;
		}
		for (int i = 0; i < soundsInfoList.Count; i++)
		{
			UIListItem uIListItem;
			if (m_ListItemPools.Count > 0)
			{
				uIListItem = m_ListItemPools.Dequeue();
				uIListItem.gameObject.SetActive(value: true);
			}
			else
			{
				uIListItem = GetListItem();
			}
			uIListItem.UpdateInfo(i, soundsInfoList[i].Name, soundsInfoList[i].PlayError);
			uIListItem.SelectEvent = ListItemSelectEvent;
			m_CurListItems.Add(uIListItem);
		}
		Respotion();
	}

	private void PlayErrorEvent(int index)
	{
		if (m_CurListItems != null && index >= 0 && index < m_CurListItems.Count)
		{
			m_CurListItems[index].SetIsPlayError(isPlayError: true);
		}
	}

	private void RecoveryListItem()
	{
		if (m_CurListItems != null && m_CurListItems.Count > 0)
		{
			for (int i = 0; i < m_CurListItems.Count; i++)
			{
				UIListItem uIListItem = m_CurListItems[i];
				uIListItem.ResetItem();
				uIListItem.gameObject.SetActive(value: false);
				m_ListItemPools.Enqueue(uIListItem);
			}
			m_CurListItems.Clear();
		}
	}

	private void UpdateSelectItem()
	{
		if (!(null == m_BackupSelectItem) && m_BackupSelectItem.ID == RadioManager.Instance.CurSoundsIndex)
		{
			return;
		}
		if ((bool)m_BackupSelectItem)
		{
			m_BackupSelectItem.CancelSelect();
		}
		int curSoundsIndex = RadioManager.Instance.CurSoundsIndex;
		if (curSoundsIndex >= 0 && curSoundsIndex < m_CurListItems.Count)
		{
			m_BackupSelectItem = m_CurListItems[curSoundsIndex];
			m_BackupSelectItem.Select();
			if (!m_ListClipPanel.IsVisible(m_BackupSelectItem.transform.position))
			{
				m_ContentScrollBar.scrollValue = curSoundsIndex / (m_CurListItems.Count - 1);
			}
		}
	}

	private void Respotion()
	{
		m_ListParentGrid.Reposition();
		m_ContentScrollBar.scrollValue = 0f;
	}

	private void ListItemSelectEvent(UIListItem item)
	{
		if (item != m_BackupSelectItem)
		{
			if (null != m_BackupSelectItem)
			{
				m_BackupSelectItem.CancelSelect();
			}
			if (null != RadioManager.Instance)
			{
				RadioManager.Instance.PlaySounds(item.ID);
			}
			m_BackupSelectItem = item;
		}
	}

	private UIListItem GetListItem()
	{
		GameObject gameObject = UnityEngine.Object.Instantiate(m_ListItemPrefab.gameObject);
		gameObject.transform.parent = m_ListParentGrid.gameObject.transform;
		gameObject.transform.localRotation = Quaternion.identity;
		gameObject.transform.localScale = Vector3.one;
		gameObject.transform.localPosition = new Vector3(0f, 0f, 0f);
		return gameObject.GetComponent<UIListItem>();
	}

	private void InitSpectrumArray()
	{
		m_SampleData = new float[m_SampleLength];
	}

	private void ResetSpectrumArray()
	{
		m_SampleData = null;
	}

	private void SetGraphShapeType(GraphPlotter.GraphShapeType type)
	{
		if (type == mGraphShapeType)
		{
			return;
		}
		mGraphShapeType = type;
		switch (type)
		{
		case GraphPlotter.GraphShapeType.TopAndBottom:
		case GraphPlotter.GraphShapeType.Top:
			if (m_PlotData == null || m_PlotData.Length != m_SampleLength)
			{
				m_PlotData = new float[m_SampleLength];
			}
			break;
		case GraphPlotter.GraphShapeType.Grid:
			if (m_PlotData == null || m_PlotData.Length != m_SpectrumXGridCount)
			{
				m_PlotData = new float[m_SpectrumXGridCount];
			}
			break;
		}
	}

	private void PlotSpectrum()
	{
		if (!m_UpdateSpectrum)
		{
			return;
		}
		RadioManager.Instance.GetOutputData(m_SampleData, 0);
		if (m_SampleData == null || m_SampleData.Length <= 0)
		{
			return;
		}
		if (m_SpectrumTopCol.Equals(m_TopNextCol))
		{
			int num;
			do
			{
				num = UnityEngine.Random.Range(0, m_RandomCols.Length);
			}
			while (m_TopNextCol.Equals(m_RandomCols[num]));
			m_TopNextCol = m_RandomCols[num];
			m_LerpT = 0f;
		}
		if (m_SpectrumBottomCol.Equals(m_BottomNextCol))
		{
			int num2;
			do
			{
				num2 = UnityEngine.Random.Range(0, m_RandomCols.Length);
			}
			while (m_BottomNextCol.Equals(m_RandomCols[num2]) || m_TopNextCol.Equals(m_RandomCols[num2]));
			m_BottomNextCol = m_RandomCols[num2];
			m_LerpT = 0f;
		}
		m_Plotter.TopColor = m_SpectrumTopCol;
		m_Plotter.BottomColor = m_SpectrumBottomCol;
		if (Time.realtimeSinceStartup - m_StartTime >= m_LeapInterval)
		{
			m_LerpT += Time.deltaTime;
			m_SpectrumTopCol = Color32.Lerp(m_SpectrumTopCol, m_TopNextCol, m_LerpT);
			m_SpectrumBottomCol = Color32.Lerp(m_SpectrumBottomCol, m_BottomNextCol, m_LerpT);
			m_StartTime = Time.realtimeSinceStartup;
		}
		switch (mGraphShapeType)
		{
		case GraphPlotter.GraphShapeType.TopAndBottom:
			m_Plotter.PlotGraph(m_SampleData, m_PlotData, m_SpectrumTexClos);
			break;
		case GraphPlotter.GraphShapeType.Top:
			m_Plotter.PlotGraph2(m_SampleData, m_PlotData, m_SpectrumTexClos);
			break;
		case GraphPlotter.GraphShapeType.Grid:
			m_Plotter.PlotGraph3(m_SampleData, m_PlotData, m_SpectrumXGridCount, m_SpectrumYGridCount, SpectrumGridBorderX, SpectrumGridBorderY, m_SpectrumTexClos);
			break;
		}
		if (null != m_SpectrumTex2d && m_SpectrumTexClos != null)
		{
			m_SpectrumTex2d.SetPixels32(m_SpectrumTexClos);
			m_SpectrumTex2d.Apply();
		}
	}

	private void OnShape0Ck(bool state)
	{
		if (state)
		{
			SetGraphShapeType(GraphPlotter.GraphShapeType.Grid);
		}
	}

	private void OnShape1Ck(bool state)
	{
		if (state)
		{
			SetGraphShapeType(GraphPlotter.GraphShapeType.TopAndBottom);
		}
	}

	private void OnShape2Ck(bool state)
	{
		if (state)
		{
			SetGraphShapeType(GraphPlotter.GraphShapeType.Top);
		}
	}
}
