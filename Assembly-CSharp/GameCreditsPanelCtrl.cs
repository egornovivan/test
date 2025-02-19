using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Xml;
using UnityEngine;

public class GameCreditsPanelCtrl : UIStaticWnd
{
	[SerializeField]
	private float m_StartWaitTime;

	[SerializeField]
	private float m_LogoSpeed;

	[SerializeField]
	private float m_ContentSpeed;

	[SerializeField]
	private UILabel m_ContentLabel;

	[SerializeField]
	private Transform m_ContentParent;

	[SerializeField]
	private GameObject m_OptionParent;

	[SerializeField]
	private N_ImageButton m_MainMenuBtn;

	[SerializeField]
	private GameObject m_ProfessionItemPrefab;

	[SerializeField]
	private GameObject m_OneNameItemPrefab;

	[SerializeField]
	private Vector2 m_Padding;

	[SerializeField]
	private bool m_UseXmlModel = true;

	[SerializeField]
	private BoxCollider m_BgBoxCollider;

	[SerializeField]
	private bool m_LoadKickstarterBackers;

	[SerializeField]
	private Transform m_TopPos;

	[SerializeField]
	private Transform m_ContentStartPos;

	[SerializeField]
	private Transform m_LogoStartPos;

	[SerializeField]
	private Transform m_LogoTrans;

	[SerializeField]
	private int m_MaxNameWidth = 200;

	[SerializeField]
	private int m_ProfessionItemMaxCapacity = 50;

	[SerializeField]
	private int m_MaxCol = 4;

	private bool m_IsPlayCredites;

	private Vector3 m_TempVector;

	private ManyPeopleItem m_EndManyPeopleItem;

	private AudioController m_CurAudioCtrl;

	private List<int> m_BgMusicIDs;

	private int m_CurBgMusicIndex;

	private void Start()
	{
		Init();
		Show();
	}

	private void Update()
	{
		if (m_IsPlayCredites)
		{
			m_TempVector.y = Time.deltaTime * m_ContentSpeed;
			m_ContentParent.transform.localPosition += m_TempVector;
			if (m_EndManyPeopleItem.transform.position.y > m_TopPos.transform.position.y)
			{
				m_ContentParent.transform.position = m_ContentStartPos.position;
			}
		}
		if (null == m_CurAudioCtrl || (m_CurAudioCtrl.length > 0f && !m_CurAudioCtrl.isPlaying))
		{
			NextBgMusic();
		}
	}

	public override void Show()
	{
		base.Show();
		StartCoroutine(ScrollContentInterator());
	}

	protected override void OnHide()
	{
		base.OnHide();
		StopBgMusic();
	}

	private void Init()
	{
		m_IsPlayCredites = false;
		m_TempVector = Vector3.zero;
		m_LogoTrans.position = m_LogoStartPos.position;
		TrySaveKickstarterBackersToXml();
		m_OptionParent.SetActive(value: false);
		m_ContentParent.position = m_ContentStartPos.position;
		m_CurAudioCtrl = null;
		UIEventListener uIEventListener = UIEventListener.Get(m_MainMenuBtn.gameObject);
		uIEventListener.onClick = (UIEventListener.VoidDelegate)Delegate.Combine(uIEventListener.onClick, (UIEventListener.VoidDelegate)delegate
		{
			OnMainMenuEvent();
		});
		UIEventListener uIEventListener2 = UIEventListener.Get(m_BgBoxCollider.gameObject);
		uIEventListener2.onClick = (UIEventListener.VoidDelegate)Delegate.Combine(uIEventListener2.onClick, (UIEventListener.VoidDelegate)delegate
		{
			ShowMenu();
		});
		FillContent();
		AdaptiveLogo();
		InitBgMusicList();
		PlayBgMusic(m_CurBgMusicIndex);
	}

	private void FillContent()
	{
		if (m_UseXmlModel)
		{
			XmlModeFillContent();
		}
		else
		{
			TxtModeFillContent();
		}
	}

	private void InitBgMusicList()
	{
		m_BgMusicIDs = new List<int>();
		m_BgMusicIDs.Add(1920);
		m_BgMusicIDs.Add(4505);
		m_BgMusicIDs.Add(4506);
		m_CurBgMusicIndex = 0;
	}

	private void PlayBgMusic(int index)
	{
		if (null == m_CurAudioCtrl && index < m_BgMusicIDs.Count && index >= 0)
		{
			m_CurAudioCtrl = AudioManager.instance.Create(base.transform.position, m_BgMusicIDs[index], null, isPlay: false, isDelete: false);
		}
		if (null != m_CurAudioCtrl && !m_CurAudioCtrl.isPlaying)
		{
			m_CurAudioCtrl.PlayAudio();
		}
	}

	private void StopBgMusic()
	{
		if (null != m_CurAudioCtrl)
		{
			m_CurAudioCtrl.Delete();
			m_CurAudioCtrl = null;
		}
	}

	private void NextBgMusic()
	{
		if (m_BgMusicIDs != null && m_BgMusicIDs.Count > 0)
		{
			if (m_CurBgMusicIndex < m_BgMusicIDs.Count - 1)
			{
				m_CurBgMusicIndex++;
			}
			else
			{
				m_CurBgMusicIndex = 0;
			}
			StopBgMusic();
			PlayBgMusic(m_CurBgMusicIndex);
		}
	}

	private void AdaptiveLogo()
	{
		float num = m_LogoTrans.localScale.x / m_LogoTrans.localScale.y;
		m_LogoTrans.localScale = new Vector3(num * (float)Screen.height / 6f, Screen.height / 6, 1f);
	}

	private void ContentToCenter()
	{
		float x = m_ContentParent.transform.localPosition.x;
		Bounds bounds = NGUIMath.CalculateRelativeWidgetBounds(m_ContentParent.transform);
		m_ContentParent.transform.localPosition += new Vector3((x - bounds.size.x) * 0.5f, 0f, 0f);
	}

	private void XmlModeFillContent()
	{
		TextAsset textAsset = Resources.Load("Credits/CreditsXml", typeof(TextAsset)) as TextAsset;
		XmlDocument xmlDocument = new XmlDocument();
		xmlDocument.LoadXml(textAsset.text);
		XmlNode xmlNode = xmlDocument.SelectSingleNode("Root");
		XmlElement xmlElement = (XmlElement)xmlNode.SelectSingleNode("Title");
		ManyPeopleItem newParticipantItem = GetNewParticipantItem();
		newParticipantItem.UpdateNames(xmlElement.GetAttribute("Name"));
		XmlNode xmlNode2 = xmlNode.SelectSingleNode("Participant");
		for (int i = 0; i < xmlNode2.ChildNodes.Count; i++)
		{
			XmlElement xmlElement2 = (XmlElement)xmlNode2.ChildNodes[i];
			if (xmlElement2.GetAttribute("Name").Equals("Kickstarter Backers") || xmlElement2.GetAttribute("Name").Equals("Other Contributors"))
			{
				List<string> list = new List<string>();
				for (int j = 0; j < xmlElement2.ChildNodes.Count; j++)
				{
					XmlElement xmlElement3 = (XmlElement)xmlElement2.ChildNodes[j];
					string empty = string.Empty;
					for (int k = 0; k < m_MaxCol; k++)
					{
						empty = xmlElement3.GetAttribute("Name" + k);
						if (!string.IsNullOrEmpty(empty))
						{
							list.Add(empty);
						}
					}
				}
				List<string> shotNames = new List<string>();
				List<string> longNames = new List<string>();
				list.ForEach(delegate(string name)
				{
					if (name.Length <= m_MaxNameWidth)
					{
						shotNames.Add(name);
					}
					else
					{
						longNames.Add(name);
					}
				});
				shotNames = shotNames.OrderBy((string a) => a.Length).ToList();
				if (shotNames.Count % m_MaxCol != 0 && longNames.Count > 0)
				{
					int num = shotNames.Count - shotNames.Count % m_MaxCol;
					longNames.AddRange(shotNames.GetRange(num, shotNames.Count - num));
					shotNames.RemoveRange(num, shotNames.Count - num);
				}
				longNames = longNames.OrderBy((string a) => a.Length).ToList();
				if (shotNames.Count > 0)
				{
					int num2 = 0;
					ProfessionItem professionItem = null;
					List<ManyPeopleName> list2 = new List<ManyPeopleName>();
					string professionName = xmlElement2.GetAttribute("Name");
					for (int l = 0; l < shotNames.Count; l += m_MaxCol)
					{
						num2++;
						int count = ((l + m_MaxCol >= shotNames.Count) ? (shotNames.Count - l) : m_MaxCol);
						string[] names = shotNames.GetRange(l, count).ToArray();
						list2.Add(new ManyPeopleName(names));
						if (num2 % m_ProfessionItemMaxCapacity == 0 || l + m_MaxCol >= shotNames.Count)
						{
							professionItem = GetNewProfessionItem();
							professionItem.UpdateInfo(new ProfessionInfo(professionName, list2), isShow2Name: false);
							list2.Clear();
							professionName = string.Empty;
						}
					}
				}
				if (longNames.Count <= 0)
				{
					continue;
				}
				ProfessionItem professionItem2 = null;
				List<ManyPeopleName> list3 = new List<ManyPeopleName>();
				int num3 = 0;
				string professionName2 = ((shotNames.Count <= 0) ? xmlElement2.GetAttribute("Name") : string.Empty);
				for (int m = 0; m < longNames.Count; m += 2)
				{
					num3++;
					list3.Add(new ManyPeopleName(longNames[m], (m + 1 >= longNames.Count) ? string.Empty : longNames[m + 1]));
					if (num3 % m_ProfessionItemMaxCapacity == 0 || m + 2 >= longNames.Count)
					{
						professionItem2 = GetNewProfessionItem();
						professionItem2.UpdateInfo(new ProfessionInfo(professionName2, list3));
						list3.Clear();
					}
				}
			}
			else
			{
				ProfessionItem newProfessionItem = GetNewProfessionItem();
				List<ManyPeopleName> list4 = new List<ManyPeopleName>();
				for (int n = 0; n < xmlElement2.ChildNodes.Count; n++)
				{
					XmlElement xmlElement4 = (XmlElement)xmlElement2.ChildNodes[n];
					list4.Add(new ManyPeopleName(xmlElement4.GetAttribute("EnglishName"), xmlElement4.GetAttribute("ChineseName")));
				}
				newProfessionItem.UpdateInfo(new ProfessionInfo(xmlElement2.GetAttribute("Name"), list4));
			}
		}
		XmlElement xmlElement5 = (XmlElement)xmlNode.SelectSingleNode("End");
		m_EndManyPeopleItem = GetNewParticipantItem();
		m_EndManyPeopleItem.UpdateNames(xmlElement5.GetAttribute("Name"));
	}

	private void RepositionVertical()
	{
		int childCount = m_ContentParent.childCount;
		Vector3 zero = Vector3.zero;
		Vector3 vector = new Vector3(0f, m_Padding.y, 0f);
		ProfessionItem professionItem = null;
		float num = 31f;
		for (int i = 0; i < childCount; i++)
		{
			Transform child = m_ContentParent.GetChild(i);
			professionItem = child.GetComponent<ProfessionItem>();
			child.localPosition = zero - vector;
			if (null != professionItem && professionItem.TitleIsNullOrEmpty)
			{
				child.localPosition = zero - new Vector3(0f, ((float)professionItem.CellHeight - num) * 2f, 0f);
			}
			else
			{
				child.localPosition = zero - vector;
			}
			Bounds bounds = NGUIMath.CalculateRelativeWidgetBounds(child);
			zero.y = child.localPosition.y - bounds.size.y;
		}
	}

	private void TxtModeFillContent()
	{
		TextAsset textAsset = Resources.Load("Credits/Credits", typeof(TextAsset)) as TextAsset;
		if (textAsset != null)
		{
			m_ContentLabel.text = textAsset.text;
		}
	}

	private ManyPeopleItem GetNewParticipantItem()
	{
		GameObject gameObject = UnityEngine.Object.Instantiate(m_OneNameItemPrefab);
		gameObject.transform.parent = m_ContentParent.transform;
		gameObject.transform.localPosition = Vector3.zero;
		gameObject.transform.localScale = Vector3.one;
		return gameObject.GetComponent<ManyPeopleItem>();
	}

	private ProfessionItem GetNewProfessionItem()
	{
		GameObject gameObject = UnityEngine.Object.Instantiate(m_ProfessionItemPrefab);
		gameObject.transform.parent = m_ContentParent.transform;
		gameObject.transform.localPosition = Vector3.zero;
		gameObject.transform.localScale = Vector3.one;
		return gameObject.GetComponent<ProfessionItem>();
	}

	private IEnumerator ScrollContentInterator()
	{
		float startTime = Time.realtimeSinceStartup;
		while (Time.realtimeSinceStartup - startTime < m_StartWaitTime)
		{
			yield return null;
		}
		RepositionVertical();
		m_IsPlayCredites = true;
		Vector3 newPos = Vector3.zero;
		m_LogoTrans.localPosition = new Vector3(m_LogoTrans.localPosition.x, m_LogoTrans.localPosition.y, 0f);
		while (m_LogoTrans.localPosition.y < 0f)
		{
			newPos.y = Time.deltaTime * m_LogoSpeed;
			m_LogoTrans.localPosition += newPos;
			yield return null;
		}
	}

	private void OnMainMenuEvent()
	{
		OnHide();
		PeSceneCtrl.Instance.GotoMainMenuScene();
	}

	private void ShowMenu()
	{
		m_OptionParent.SetActive(value: true);
	}

	private void TrySaveKickstarterBackersToXml()
	{
		if (!Application.isEditor || !m_LoadKickstarterBackers)
		{
			return;
		}
		TextAsset textAsset = Resources.Load("Credits/CreditsXml", typeof(TextAsset)) as TextAsset;
		if (null == textAsset)
		{
			return;
		}
		XmlDocument xmlDocument = new XmlDocument();
		xmlDocument.LoadXml(textAsset.text);
		XmlNodeList xmlNodeList = xmlDocument.SelectNodes("Root/Participant/Profession");
		XmlNode xmlNode = null;
		for (int i = 0; i < xmlNodeList.Count; i++)
		{
			if (xmlNodeList[i].Attributes["Name"].Value == "Kickstarter Backers")
			{
				xmlNode = xmlNodeList[i];
				break;
			}
		}
		if (xmlNode != null)
		{
			TextAsset textAsset2 = Resources.Load("Credits/KickstarterBackers", typeof(TextAsset)) as TextAsset;
			if (string.IsNullOrEmpty(textAsset2.text))
			{
				return;
			}
			string[] array = textAsset2.text.Split(new string[1] { "\r\n" }, StringSplitOptions.None);
			if (array.Length <= 0)
			{
				return;
			}
			XmlNode lastChild = xmlNode.LastChild;
			for (int j = 0; j < array.Length; j += m_MaxCol)
			{
				XmlNode xmlNode2 = lastChild.CloneNode(deep: true);
				for (int k = 0; k < m_MaxCol; k++)
				{
					xmlNode2.Attributes["Name" + k].Value = ((k + j >= array.Length) ? string.Empty : array[k + j]);
				}
				xmlNode.AppendChild(xmlNode2);
			}
		}
		string text = Application.dataPath + "/Resources/Credits/NewCreditsXml.xml";
		try
		{
			xmlDocument.Save(text);
			Debug.Log("Save NewCreditsXml Succeed! \nPath:" + text);
		}
		catch (Exception ex)
		{
			Debug.Log("Save Failed! Error:" + ex.Message);
		}
	}
}
