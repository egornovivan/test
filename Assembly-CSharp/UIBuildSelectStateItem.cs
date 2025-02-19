using UnityEngine;

public class UIBuildSelectStateItem : MonoBehaviour
{
	[SerializeField]
	private UISprite m_IconSprite;

	public int type;

	public void SetIcon(string sprit_name)
	{
		m_IconSprite.spriteName = sprit_name;
	}
}
