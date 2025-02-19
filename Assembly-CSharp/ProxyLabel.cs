using uLink;
using UnityEngine;

[AddComponentMenu("uLink Utilities/Object Label")]
public class ProxyLabel : uLink.MonoBehaviour
{
	public float minDistance = 1f;

	public float maxDistance = 96f;

	public float clampBorderSize = 0.05f;

	public Color color = Color.green;

	public Vector3 offset = new Vector3(0f, 2f, 0f);

	public GUIText prefabLabel;

	private GUIText guiText;

	private void Awake()
	{
		GameObject gameObject = Resources.Load("Prefab/PlayerPrefab/PlayerLabelText") as GameObject;
		prefabLabel = gameObject.GetComponent<GUIText>();
	}

	private void OnDestroy()
	{
		if (guiText != null)
		{
			Object.Destroy(guiText);
			guiText = null;
		}
	}

	private void LateUpdate()
	{
		ManualUpdate();
	}

	public void SetName(string name, int group)
	{
		guiText = Object.Instantiate(prefabLabel, Vector3.zero, Quaternion.identity) as GUIText;
		if (null != guiText)
		{
			guiText.text = name;
			if (group == BaseNetwork.MainPlayer.TeamId)
			{
				guiText.material.color = Color.green;
				return;
			}
			Color32 forceColor = Singleton<ForceSetting>.Instance.GetForceColor(group);
			Color color = new Color((float)(int)forceColor.r / 255f, (float)(int)forceColor.g / 255f, (float)(int)forceColor.b / 255f, (float)(int)forceColor.a / 255f);
			guiText.material.color = color;
		}
	}

	public void ManualUpdate()
	{
		if (!(guiText == null) && !(Camera.main == null))
		{
			Vector3 position = Camera.main.WorldToViewportPoint(base.transform.position + offset);
			guiText.transform.position = position;
			guiText.enabled = position.z >= minDistance && position.z <= maxDistance;
		}
	}

	public static void ManualUpdateAll()
	{
		ProxyLabel[] array = Object.FindObjectsOfType(typeof(ProxyLabel)) as ProxyLabel[];
		ProxyLabel[] array2 = array;
		foreach (ProxyLabel proxyLabel in array2)
		{
			proxyLabel.ManualUpdate();
		}
	}
}
