using UnityEngine;

public class VCEUISceneMenu : MonoBehaviour
{
	public GameObject m_ItemPrefab;

	private void Start()
	{
		foreach (VCESceneSetting s_EditorScene in VCConfig.s_EditorScenes)
		{
			if (s_EditorScene.m_ParentId == 0)
			{
				GameObject gameObject = Object.Instantiate(m_ItemPrefab);
				Vector3 localScale = gameObject.transform.localScale;
				gameObject.transform.parent = base.transform;
				gameObject.transform.localPosition = Vector3.zero;
				gameObject.transform.localScale = localScale;
				gameObject.name = "Scene " + s_EditorScene.m_Id.ToString("00");
				gameObject.GetComponent<VCEUISceneMenuItem>().m_SceneSetting = s_EditorScene;
				gameObject.SetActive(value: true);
			}
		}
		GetComponent<UIGrid>().Reposition();
	}
}
