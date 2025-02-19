using System.IO;
using System.Xml.Serialization;
using UnityEngine;

public class TutorMgr : MonoBehaviour
{
	private static GameObject tutorObj;

	public UIGrid tutorialGrid;

	public UIPanel stepPanel;

	public UILabel currentStepLabel;

	public UITexture tutorTexture;

	public TextAsset tutorialTextData;

	private TutorData tutorData;

	public GameObject tutorialProtoType;

	public GameObject stepProtoType;

	private int currentLessonIndex;

	private int currentStepIndex;

	public static void Load()
	{
		if (!TutorLoaded())
		{
			GameObject original = Resources.Load("CreationSystemTutor", typeof(GameObject)) as GameObject;
			tutorObj = Object.Instantiate(original, Vector3.zero, Quaternion.identity) as GameObject;
		}
	}

	public static bool TutorLoaded()
	{
		return null != tutorObj;
	}

	public static void Destroy()
	{
		if (TutorLoaded())
		{
			Object.Destroy(tutorObj);
			tutorObj = null;
		}
	}

	private void Awake()
	{
		currentLessonIndex = 0;
		currentStepIndex = 0;
		VCEditor.OnCloseComing += OnVcEditorClose;
	}

	private void Start()
	{
		LoadData();
		UpdateUI();
		InitUI();
	}

	private void OnDestroy()
	{
		VCEditor.OnCloseComing -= OnVcEditorClose;
	}

	private void OnVcEditorClose()
	{
		Destroy();
	}

	private TutorItem CreateItem(Transform parent)
	{
		GameObject gameObject = Object.Instantiate(tutorialProtoType);
		gameObject.transform.parent = parent;
		gameObject.transform.localPosition = Vector3.zero;
		gameObject.transform.localRotation = Quaternion.identity;
		gameObject.transform.localScale = Vector3.one;
		gameObject.SetActive(value: true);
		return gameObject.GetComponentInChildren<TutorItem>();
	}

	private void DeleteAllChildren(Transform trans)
	{
		while (trans.childCount != 0)
		{
			NGUITools.Destroy(trans.GetChild(0).gameObject);
		}
	}

	private void UpdateTutorial()
	{
		DeleteAllChildren(tutorialGrid.transform);
		for (int i = 0; i < tutorData.lessons.Count; i++)
		{
			TutorLessonData tutorLessonData = tutorData.lessons[i];
			TutorItem tutorItem = CreateItem(tutorialGrid.transform);
			tutorItem.SetText(tutorLessonData.lessonName);
			tutorItem.itemId = i;
			UIButtonMessage component = tutorItem.GetComponent<UIButtonMessage>();
			component.functionName = "OnLessonClicked";
			component.target = base.gameObject;
			if (i == currentLessonIndex)
			{
				tutorItem.Checked();
			}
		}
		tutorialGrid.repositionNow = true;
	}

	private TutorStepItem CreateStepItem(Transform parent)
	{
		GameObject gameObject = Object.Instantiate(stepProtoType);
		gameObject.transform.parent = parent;
		gameObject.transform.localPosition = Vector3.zero;
		gameObject.transform.localRotation = Quaternion.identity;
		gameObject.transform.localScale = Vector3.one;
		gameObject.SetActive(value: true);
		return gameObject.GetComponent<TutorStepItem>();
	}

	private void UpdateStep()
	{
		DeleteAllChildren(stepPanel.transform);
		float w = stepPanel.clipRange.w;
		float num = w / 2f;
		float num2 = 0f;
		for (int i = 0; i < tutorData.lessons[currentLessonIndex].steps.Count; i++)
		{
			TutorStepData tutorStepData = tutorData.lessons[currentLessonIndex].steps[i];
			TutorStepItem tutorStepItem = CreateStepItem(stepPanel.transform);
			tutorStepItem.Text = tutorStepData.name.ToLocalizationString() + "\n" + tutorStepData.description.ToLocalizationString();
			tutorStepItem.itemId = i;
			UIButtonMessage component = tutorStepItem.GetComponent<UIButtonMessage>();
			component.functionName = "OnStepClicked";
			component.target = base.gameObject;
			num -= tutorStepItem.Size.y / 2f;
			tutorStepItem.transform.localPosition = new Vector3(0f, num, 0f);
			if (i == currentStepIndex)
			{
				currentStepLabel.text = "STEP:" + (i + 1) + "/" + tutorData.lessons[currentLessonIndex].steps.Count;
				tutorStepItem.Selected();
				num2 = num;
			}
			num -= tutorStepItem.Size.y / 2f + 10f;
		}
		UIDraggablePanel component2 = stepPanel.GetComponent<UIDraggablePanel>();
		component2.ResetPosition();
		float y = num2 / (num + w / 2f + 10f);
		component2.SetDragAmount(0f, y, updateScrollbars: false);
	}

	private void UpdateStepImage()
	{
		string imageFileName = tutorData.lessons[currentLessonIndex].steps[currentStepIndex].imageFileName;
		if (string.IsNullOrEmpty(imageFileName))
		{
			tutorTexture.enabled = false;
			return;
		}
		imageFileName = "TutorTexture/" + imageFileName;
		if (SystemSettingData.Instance.IsChinese)
		{
			imageFileName += "_cn";
		}
		Texture2D texture2D = Resources.Load(imageFileName) as Texture2D;
		if (null == texture2D)
		{
			tutorTexture.enabled = false;
			return;
		}
		tutorTexture.enabled = true;
		tutorTexture.mainTexture = texture2D;
		if (IsFullScreenTex())
		{
			tutorTexture.transform.localPosition = Vector3.zero;
		}
		else
		{
			tutorTexture.transform.localPosition = new Vector3(0f, -133f, 0f);
		}
		tutorTexture.MakePixelPerfect();
	}

	private void InitUI()
	{
		string text = "TutorTexture/1_1";
		if (SystemSettingData.Instance.IsChinese)
		{
			text += "_cn";
		}
		Texture2D texture2D = Resources.Load(text) as Texture2D;
		if (null == texture2D)
		{
			tutorTexture.enabled = false;
			return;
		}
		tutorTexture.enabled = true;
		tutorTexture.mainTexture = texture2D;
	}

	private void UpdateUI()
	{
		UpdateTutorial();
		UpdateStep();
		UpdateStepImage();
	}

	private void OnClose()
	{
		Destroy();
	}

	private void OnStepPre()
	{
		if (currentStepIndex > 0)
		{
			currentStepIndex--;
		}
		else if (currentLessonIndex > 0)
		{
			currentLessonIndex--;
			currentStepIndex = tutorData.lessons[currentLessonIndex].steps.Count - 1;
		}
		UpdateUI();
	}

	private void OnStepNext()
	{
		if (currentStepIndex < tutorData.lessons[currentLessonIndex].steps.Count - 1)
		{
			currentStepIndex++;
		}
		else if (currentLessonIndex < tutorData.lessons.Count - 1)
		{
			currentLessonIndex++;
			currentStepIndex = 0;
		}
		UpdateUI();
	}

	private void OnStepClicked(GameObject arg)
	{
		TutorStepItem component = arg.GetComponent<TutorStepItem>();
		currentStepIndex = component.itemId;
		UpdateStep();
		UpdateStepImage();
	}

	private void OnLessonClicked()
	{
		TutorItem component = UICheckbox.current.GetComponent<TutorItem>();
		if (component == null)
		{
			Debug.LogError("[TutorMgr]Error:TutorItem not found");
			return;
		}
		currentLessonIndex = component.itemId;
		currentStepIndex = 0;
		UpdateStep();
		UpdateStepImage();
	}

	private bool IsFullScreenTex()
	{
		if (currentLessonIndex != 0 || currentStepIndex != 0)
		{
			return false;
		}
		return true;
	}

	private void OnTextureClicked()
	{
		if (IsFullScreenTex())
		{
			Vector3 localPosition = tutorTexture.transform.localPosition;
			localPosition.y = -133f;
			tutorTexture.transform.localPosition = localPosition;
			tutorTexture.transform.localScale = new Vector3(676f, 380f, 1f);
		}
	}

	private void LoadData()
	{
		using MemoryStream memoryStream = new MemoryStream(tutorialTextData.bytes);
		if (memoryStream != null)
		{
			XmlSerializer xmlSerializer = new XmlSerializer(typeof(TutorData));
			tutorData = xmlSerializer.Deserialize(memoryStream) as TutorData;
		}
	}

	private void CreateTemplate()
	{
		TutorData tutorData = new TutorData();
		for (int i = 0; i < 7; i++)
		{
			TutorLessonData tutorLessonData = new TutorLessonData(i);
			for (int j = 0; j < 3; j++)
			{
				tutorLessonData.steps.Add(new TutorStepData(i, j));
			}
			tutorData.lessons.Add(tutorLessonData);
		}
		using FileStream fileStream = new FileStream(Directory.GetCurrentDirectory() + "/Tutorials.xml", FileMode.Create, FileAccess.Write);
		if (fileStream != null)
		{
			XmlSerializer xmlSerializer = new XmlSerializer(typeof(TutorData));
			xmlSerializer.Serialize(fileStream, tutorData);
		}
	}
}
