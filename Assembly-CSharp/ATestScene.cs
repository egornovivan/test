using UnityEngine;

public class ATestScene : MonoBehaviour
{
	public GameObject[] disable;

	private void Awake()
	{
		GameObject[] array = disable;
		foreach (GameObject gameObject in array)
		{
			gameObject.SetActive(value: false);
		}
	}
}
