using System.IO;
using UnityEngine;

public class VCEUIIsoList : VCEUIAssetList
{
	public GameObject m_UplevelBtn;

	public VCEUIIsoItem m_SelectedItem;

	public GameObject m_EmptyItem;

	public string m_Path = string.Empty;

	public VCEUIIsoHeaderInspector m_IsoHeaderInspectorRes;

	public VCEUIIsoHeaderInspector m_IsoHeaderInspector;

	public override void Init()
	{
		base.Init();
		m_Path = VCConfig.s_IsoPath;
	}

	public void CreateInspector()
	{
		DestroyInspector();
		m_IsoHeaderInspector = Object.Instantiate(m_IsoHeaderInspectorRes);
		m_IsoHeaderInspector.transform.parent = VCEditor.Instance.m_UI.m_InspectorGroup;
		m_IsoHeaderInspector.transform.localPosition = Vector3.zero;
		m_IsoHeaderInspector.transform.localScale = Vector3.one;
		m_IsoHeaderInspector.gameObject.SetActive(value: true);
	}

	public void DestroyInspector()
	{
		if (m_IsoHeaderInspector != null)
		{
			Object.Destroy(m_IsoHeaderInspector.gameObject);
			m_IsoHeaderInspector = null;
		}
	}

	private new void Update()
	{
		base.Update();
		if (m_Path.Length <= VCConfig.s_IsoPath.Length)
		{
			m_UplevelBtn.SetActive(value: false);
		}
		else
		{
			m_UplevelBtn.SetActive(value: true);
		}
		if (m_SelectedItem != null)
		{
			m_IsoHeaderInspector.FilePath = ((!m_SelectedItem.m_IsFolder) ? m_SelectedItem.m_FilePath : string.Empty);
		}
		else
		{
			m_IsoHeaderInspector.FilePath = string.Empty;
		}
	}

	public void RefreshIsoList()
	{
		string path = m_Path;
		if (!Directory.Exists(path))
		{
			Directory.CreateDirectory(path);
		}
		string[] directories = Directory.GetDirectories(path);
		string[] files = Directory.GetFiles(path);
		ClearItems();
		m_SelectedItem = null;
		string[] array = directories;
		foreach (string text in array)
		{
			GameObject gameObject = Object.Instantiate(m_ItemRes);
			Vector3 localScale = gameObject.transform.localScale;
			gameObject.name = "[Dir] " + new DirectoryInfo(text).Name;
			gameObject.transform.parent = m_ItemGroup.transform;
			gameObject.transform.localPosition = Vector3.zero;
			gameObject.transform.localScale = localScale;
			VCEUIIsoItem component = gameObject.GetComponent<VCEUIIsoItem>();
			component.m_ParentList = this;
			component.m_HoverBtn.AddComponent<UIDragPanelContents>();
			component.m_IsFolder = true;
			component.m_FilePath = text;
			m_AssetItems.Add(gameObject);
		}
		string[] array2 = files;
		foreach (string text2 in array2)
		{
			GameObject gameObject2 = Object.Instantiate(m_ItemRes);
			Vector3 localScale2 = gameObject2.transform.localScale;
			gameObject2.name = "[File] " + new FileInfo(text2).Name;
			gameObject2.transform.parent = m_ItemGroup.transform;
			gameObject2.transform.localPosition = Vector3.zero;
			gameObject2.transform.localScale = localScale2;
			VCEUIIsoItem component2 = gameObject2.GetComponent<VCEUIIsoItem>();
			component2.m_ParentList = this;
			component2.m_HoverBtn.AddComponent<UIDragPanelContents>();
			component2.m_IsFolder = false;
			component2.m_FilePath = text2;
			m_AssetItems.Add(gameObject2);
		}
		if (directories.Length + files.Length == 0)
		{
			m_EmptyItem.SetActive(value: true);
		}
		else
		{
			m_EmptyItem.SetActive(value: false);
		}
		RepositionGrid();
		RepositionList();
	}

	private void OnUplevelClick()
	{
		if (m_Path.Length > 0)
		{
			m_Path = new DirectoryInfo(m_Path).Parent.FullName;
		}
		RefreshIsoList();
	}
}
