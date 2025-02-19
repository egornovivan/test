using UnityEngine;

public class HandleCollide : MonoBehaviour
{
	public GameObject bang;

	private void OnCollisionEnter()
	{
		Time.timeScale = 0f;
		bang.SetActive(value: true);
	}
}
