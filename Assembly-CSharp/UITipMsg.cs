using UnityEngine;

public class UITipMsg : MonoBehaviour
{
	public UILabel content;

	public UISprite icon;

	public UITexture tex;

	public int musicID;

	public void SetStyle(PeTipMsg.EStyle eStyle)
	{
		switch (eStyle)
		{
		case PeTipMsg.EStyle.Text:
			icon.gameObject.SetActive(value: false);
			tex.gameObject.SetActive(value: false);
			content.transform.localPosition = new Vector3(0f, 0f, 0f);
			break;
		case PeTipMsg.EStyle.Icon:
			icon.gameObject.SetActive(value: true);
			tex.gameObject.SetActive(value: false);
			content.transform.localPosition = new Vector3(40f, 0f, 0f);
			break;
		case PeTipMsg.EStyle.Texture:
			icon.gameObject.SetActive(value: false);
			tex.gameObject.SetActive(value: true);
			content.transform.localPosition = new Vector3(40f, 0f, 0f);
			break;
		}
	}

	public Bounds GetBounds()
	{
		Bounds result = default(Bounds);
		Bounds bounds = NGUIMath.CalculateRelativeWidgetBounds(content.transform);
		Vector3 localScale = content.transform.localScale;
		bounds.min = Vector3.Scale(bounds.min, localScale);
		bounds.max = Vector3.Scale(bounds.max, localScale);
		bounds.center = content.transform.localPosition;
		result.Encapsulate(bounds);
		if (icon != null)
		{
			bounds = NGUIMath.CalculateRelativeWidgetBounds(icon.transform);
			localScale = icon.transform.localScale;
			bounds.min = Vector3.Scale(bounds.min, localScale);
			bounds.max = Vector3.Scale(bounds.max, localScale);
			bounds.center = icon.transform.localPosition;
			result.Encapsulate(bounds);
		}
		return result;
	}

	public void Reset()
	{
		content.text = string.Empty;
		icon.spriteName = string.Empty;
		tex.mainTexture = null;
	}
}
