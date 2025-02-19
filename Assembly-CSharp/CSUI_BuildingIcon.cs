using UnityEngine;

public class CSUI_BuildingIcon : MonoBehaviour
{
	[SerializeField]
	private UILabel m_Description;

	[SerializeField]
	private UISlicedSprite m_Icon;

	public string Description
	{
		get
		{
			return m_Description.text;
		}
		set
		{
			m_Description.text = value;
		}
	}

	public string IconName
	{
		get
		{
			return m_Icon.spriteName;
		}
		set
		{
			m_Icon.spriteName = value;
		}
	}

	private void Start()
	{
	}

	private void Update()
	{
	}
}
