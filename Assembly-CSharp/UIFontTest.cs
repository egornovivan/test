using UnityEngine;

public class UIFontTest : MonoBehaviour
{
	private void Start()
	{
		UILabel component = base.gameObject.GetComponent<UILabel>();
		Debug.Log(component.font.size);
	}

	private void Update()
	{
	}
}
