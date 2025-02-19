using UnityEngine;

public class CreationUILoader : MonoBehaviour
{
	public string m_ResPath = "GUI/Prefabs/Creation/Creation UI";

	private void Awake()
	{
		GameObject gameObject = Resources.Load(m_ResPath) as GameObject;
		if (gameObject != null)
		{
			GameObject gameObject2 = Object.Instantiate(gameObject);
			gameObject2.name = gameObject.name;
			gameObject2.transform.parent = base.transform;
			gameObject2.transform.localPosition = Vector3.zero;
			gameObject2.transform.localRotation = Quaternion.identity;
			gameObject2.transform.localScale = Vector3.one;
		}
	}
}
