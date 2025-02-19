using System;
using System.Collections.Generic;
using UnityEngine;

public class VCEUIFastCreateWnd : MonoBehaviour
{
	public GameObject m_Window;

	public UIEventListener m_BackButton;

	public UIEventListener m_CloseButton;

	public VCEUICreationTypeItem m_ItemPrefab;

	private int parentId;

	private List<VCEUICreationTypeItem> items = new List<VCEUICreationTypeItem>();

	private void OnEnable()
	{
		UIEventListener backButton = m_BackButton;
		backButton.onClick = (UIEventListener.VoidDelegate)Delegate.Combine(backButton.onClick, new UIEventListener.VoidDelegate(OnBackClick));
		UIEventListener closeButton = m_CloseButton;
		closeButton.onClick = (UIEventListener.VoidDelegate)Delegate.Combine(closeButton.onClick, new UIEventListener.VoidDelegate(OnCloseClick));
		CreateChildItems();
	}

	private void OnDisable()
	{
		UIEventListener backButton = m_BackButton;
		backButton.onClick = (UIEventListener.VoidDelegate)Delegate.Remove(backButton.onClick, new UIEventListener.VoidDelegate(OnBackClick));
		UIEventListener closeButton = m_CloseButton;
		closeButton.onClick = (UIEventListener.VoidDelegate)Delegate.Remove(closeButton.onClick, new UIEventListener.VoidDelegate(OnCloseClick));
		parentId = 0;
		foreach (VCEUICreationTypeItem item in items)
		{
			item.FadeOut();
			UnityEngine.Object.Destroy(item.gameObject);
		}
		items.Clear();
		m_BackButton.gameObject.SetActive(value: false);
	}

	private void OnDestroy()
	{
	}

	public bool WindowVisible()
	{
		return m_Window.activeInHierarchy;
	}

	public void ShowWindow()
	{
		m_Window.SetActive(value: true);
	}

	public void HideWindow()
	{
		m_Window.SetActive(value: false);
	}

	public void OnBackClick(GameObject sender)
	{
		if (parentId != 0)
		{
			parentId = VCConfig.s_EditorScenes.Find((VCESceneSetting iter) => iter.m_Id == parentId).m_ParentId;
		}
		if (parentId == 0)
		{
			m_BackButton.gameObject.SetActive(value: false);
		}
		CreateChildItems();
	}

	public void OnCloseClick(GameObject sender)
	{
		HideWindow();
	}

	public void OnItemClick(GameObject sender)
	{
		VCEUICreationTypeItem componentInParent = sender.GetComponentInParent<VCEUICreationTypeItem>();
		int usage = componentInParent.usage;
		VCESceneSetting scene = componentInParent.m_Scene;
		switch (usage)
		{
		case 0:
			parentId = scene.m_Id;
			m_BackButton.gameObject.SetActive(value: true);
			CreateChildItems();
			break;
		case 1:
			parentId = scene.m_Id;
			m_BackButton.gameObject.SetActive(value: true);
			CreateChildItems();
			break;
		case 2:
			OnCloseClick(base.gameObject);
			VCEditor.Instance.m_UI.m_ISOTab.isChecked = true;
			VCEditor.Instance.m_UI.m_IsoTip.Show();
			break;
		case 3:
		{
			TextAsset textAsset = Resources.Load<TextAsset>("Isos/" + scene.m_Id + "/index");
			if (textAsset == null)
			{
				OnCloseClick(base.gameObject);
				VCEditor.NewScene(scene);
				break;
			}
			int result = 0;
			int.TryParse(textAsset.text, out result);
			if (result == 0)
			{
				OnCloseClick(base.gameObject);
				VCEditor.NewScene(scene);
			}
			else
			{
				int template = (int)(UnityEngine.Random.value * (float)result - 1E-05f);
				OnCloseClick(base.gameObject);
				VCEditor.NewScene(scene, template);
			}
			break;
		}
		case 4:
			OnCloseClick(base.gameObject);
			VCEditor.NewScene(scene);
			break;
		}
	}

	public void CreateChildItems()
	{
		foreach (VCEUICreationTypeItem item in items)
		{
			item.FadeOut();
		}
		items.Clear();
		if (parentId > 0)
		{
			foreach (VCESceneSetting s_EditorScene in VCConfig.s_EditorScenes)
			{
				if (s_EditorScene.m_ParentId == parentId)
				{
					VCEUICreationTypeItem vCEUICreationTypeItem = UnityEngine.Object.Instantiate(m_ItemPrefab);
					vCEUICreationTypeItem.m_Scene = s_EditorScene;
					vCEUICreationTypeItem.transform.parent = m_ItemPrefab.transform.parent;
					vCEUICreationTypeItem.transform.localScale = m_ItemPrefab.transform.localScale;
					vCEUICreationTypeItem.gameObject.name = s_EditorScene.m_Name;
					vCEUICreationTypeItem.gameObject.SetActive(value: true);
					vCEUICreationTypeItem.FadeIn();
					items.Add(vCEUICreationTypeItem);
				}
			}
		}
		else
		{
			VCEUICreationTypeItem vCEUICreationTypeItem2 = UnityEngine.Object.Instantiate(m_ItemPrefab);
			vCEUICreationTypeItem2.m_Scene = VCConfig.s_EditorScenes.Find((VCESceneSetting iter) => iter.m_Id == 1);
			vCEUICreationTypeItem2.usage = 1;
			vCEUICreationTypeItem2.transform.parent = m_ItemPrefab.transform.parent;
			vCEUICreationTypeItem2.transform.localScale = m_ItemPrefab.transform.localScale;
			vCEUICreationTypeItem2.m_NameLabel.text = "New".ToLocalizationString();
			vCEUICreationTypeItem2.gameObject.name = "New".ToLocalizationString();
			vCEUICreationTypeItem2.gameObject.SetActive(value: true);
			vCEUICreationTypeItem2.FadeIn();
			items.Add(vCEUICreationTypeItem2);
			vCEUICreationTypeItem2 = UnityEngine.Object.Instantiate(m_ItemPrefab);
			vCEUICreationTypeItem2.usage = 2;
			vCEUICreationTypeItem2.m_Scene = null;
			vCEUICreationTypeItem2.transform.parent = m_ItemPrefab.transform.parent;
			vCEUICreationTypeItem2.transform.localScale = m_ItemPrefab.transform.localScale;
			vCEUICreationTypeItem2.m_NameLabel.text = "Open".ToLocalizationString();
			vCEUICreationTypeItem2.gameObject.name = "Open".ToLocalizationString();
			vCEUICreationTypeItem2.gameObject.SetActive(value: true);
			vCEUICreationTypeItem2.FadeIn();
			items.Add(vCEUICreationTypeItem2);
		}
		if (items.Count == 0 && parentId > 0)
		{
			VCEUICreationTypeItem vCEUICreationTypeItem3 = UnityEngine.Object.Instantiate(m_ItemPrefab);
			vCEUICreationTypeItem3.m_Scene = VCConfig.s_EditorScenes.Find((VCESceneSetting iter) => iter.m_Id == parentId);
			vCEUICreationTypeItem3.usage = 3;
			vCEUICreationTypeItem3.transform.parent = m_ItemPrefab.transform.parent;
			vCEUICreationTypeItem3.transform.localScale = m_ItemPrefab.transform.localScale;
			vCEUICreationTypeItem3.m_NameLabel.text = "Template".ToLocalizationString();
			vCEUICreationTypeItem3.gameObject.name = "Template".ToLocalizationString();
			vCEUICreationTypeItem3.gameObject.SetActive(value: true);
			vCEUICreationTypeItem3.FadeIn();
			items.Add(vCEUICreationTypeItem3);
			vCEUICreationTypeItem3 = UnityEngine.Object.Instantiate(m_ItemPrefab);
			vCEUICreationTypeItem3.m_Scene = VCConfig.s_EditorScenes.Find((VCESceneSetting iter) => iter.m_Id == parentId);
			vCEUICreationTypeItem3.usage = 4;
			vCEUICreationTypeItem3.transform.parent = m_ItemPrefab.transform.parent;
			vCEUICreationTypeItem3.transform.localScale = m_ItemPrefab.transform.localScale;
			vCEUICreationTypeItem3.m_NameLabel.text = "Empty".ToLocalizationString();
			vCEUICreationTypeItem3.gameObject.name = "Empty".ToLocalizationString();
			vCEUICreationTypeItem3.gameObject.SetActive(value: true);
			vCEUICreationTypeItem3.FadeIn();
			items.Add(vCEUICreationTypeItem3);
		}
		if (items.Count == 1)
		{
			items[0].transform.localPosition = new Vector3(-100f, 100f);
		}
		else if (items.Count == 2)
		{
			items[0].transform.localPosition = new Vector3(-200f, 100f);
			items[1].transform.localPosition = new Vector3(0f, 100f);
		}
		else if (items.Count == 3)
		{
			items[0].transform.localPosition = new Vector3(-300f, 100f);
			items[1].transform.localPosition = new Vector3(-100f, 100f);
			items[2].transform.localPosition = new Vector3(100f, 100f);
		}
		else if (items.Count == 4)
		{
			items[0].transform.localPosition = new Vector3(-200f, 200f);
			items[1].transform.localPosition = new Vector3(0f, 200f);
			items[2].transform.localPosition = new Vector3(-200f, 0f);
			items[3].transform.localPosition = new Vector3(0f, 0f);
		}
		else if (items.Count == 5)
		{
			items[0].transform.localPosition = new Vector3(-300f, 200f);
			items[1].transform.localPosition = new Vector3(-100f, 200f);
			items[2].transform.localPosition = new Vector3(100f, 200f);
			items[3].transform.localPosition = new Vector3(-200f, 0f);
			items[4].transform.localPosition = new Vector3(0f, 0f);
		}
		else if (items.Count == 6)
		{
			items[0].transform.localPosition = new Vector3(-300f, 200f);
			items[1].transform.localPosition = new Vector3(-100f, 200f);
			items[2].transform.localPosition = new Vector3(100f, 200f);
			items[3].transform.localPosition = new Vector3(-300f, 0f);
			items[4].transform.localPosition = new Vector3(-100f, 0f);
			items[5].transform.localPosition = new Vector3(100f, 0f);
		}
		else if (items.Count == 7)
		{
			items[0].transform.localPosition = new Vector3(-400f, 200f);
			items[1].transform.localPosition = new Vector3(-200f, 200f);
			items[2].transform.localPosition = new Vector3(0f, 200f);
			items[3].transform.localPosition = new Vector3(200f, 200f);
			items[4].transform.localPosition = new Vector3(-300f, 0f);
			items[5].transform.localPosition = new Vector3(-100f, 0f);
			items[6].transform.localPosition = new Vector3(100f, 0f);
		}
		else if (items.Count == 8)
		{
			items[0].transform.localPosition = new Vector3(-400f, 200f);
			items[1].transform.localPosition = new Vector3(-200f, 200f);
			items[2].transform.localPosition = new Vector3(0f, 200f);
			items[3].transform.localPosition = new Vector3(200f, 200f);
			items[4].transform.localPosition = new Vector3(-400f, 0f);
			items[5].transform.localPosition = new Vector3(-200f, 0f);
			items[6].transform.localPosition = new Vector3(0f, 0f);
			items[7].transform.localPosition = new Vector3(200f, 0f);
		}
	}
}
