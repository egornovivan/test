using UnityEngine;

public class UIListSpeularEffect : MonoBehaviour
{
	private UIMenuPanel panel;

	[SerializeField]
	private UITexture texSPeaular;

	[SerializeField]
	private UISprite sprBg_1;

	private void Start()
	{
		panel = base.transform.parent.GetComponent<UIMenuPanel>();
		if (!(panel == null))
		{
			sprBg_1.pivot = panel.spBg.pivot;
			texSPeaular.pivot = panel.spBg.pivot;
			Vector3 localPosition = panel.spBg.transform.localPosition;
			texSPeaular.transform.localPosition = localPosition;
			sprBg_1.transform.localPosition = localPosition;
		}
	}

	private void Update()
	{
		if (panel != null)
		{
			Vector3 localScale = panel.spBg.transform.localScale;
			texSPeaular.transform.localScale = localScale;
			sprBg_1.transform.localScale = localScale;
		}
	}
}
