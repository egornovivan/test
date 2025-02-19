using UnityEngine;

public class TutorItem : MonoBehaviour
{
	public int itemId;

	private void Start()
	{
		Reset();
	}

	private void Reset()
	{
		UIGrid component = base.transform.parent.GetComponent<UIGrid>();
		Vector2 vector = new Vector2(component.cellWidth, component.cellHeight);
		BoxCollider component2 = base.gameObject.GetComponent<BoxCollider>();
		component2.size = new Vector3(vector.x, vector.y, 1f);
		UILabel componentInChildren = base.gameObject.GetComponentInChildren<UILabel>();
		componentInChildren.transform.localPosition = new Vector3((0f - vector.x) / 2f + 10f, 0f, 0f);
		UISlicedSprite componentInChildren2 = base.gameObject.GetComponentInChildren<UISlicedSprite>();
		componentInChildren2.transform.localScale = new Vector3(vector.x, vector.y, 1f);
		UICheckbox component3 = base.gameObject.GetComponent<UICheckbox>();
		component3.radioButtonRoot = base.transform.parent;
	}

	public void SetText(string text)
	{
		UILabel componentInChildren = base.gameObject.GetComponentInChildren<UILabel>();
		componentInChildren.text = text.ToLocalizationString();
	}

	public void Checked()
	{
		UICheckbox component = base.gameObject.GetComponent<UICheckbox>();
		component.isChecked = true;
	}
}
