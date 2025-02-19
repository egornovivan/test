using UnityEngine;

public class TutorStepItem : MonoBehaviour
{
	public UILabel label;

	public int itemId;

	public string Text
	{
		set
		{
			label.text = value;
		}
	}

	public Vector2 Size => label.relativeSize * label.transform.localScale.x;

	public void Selected()
	{
		label.color = new Color32(byte.MaxValue, 200, 70, byte.MaxValue);
	}

	private void Start()
	{
		BoxCollider component = base.gameObject.GetComponent<BoxCollider>();
		component.size = new Vector3(component.size.x, Size.y, component.size.z);
	}
}
