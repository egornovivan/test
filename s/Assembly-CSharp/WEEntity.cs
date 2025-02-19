using UnityEngine;

public abstract class WEEntity : WEObject
{
	[XMLIO(Attr = "player", Order = -7, DefaultValue = 0)]
	[HideInInspector]
	public int PlayerIndex;

	[HideInInspector]
	[XMLIO(Attr = "tag", Order = 99, DefaultValue = 0f)]
	public float Tag;

	[XMLIO(Attr = "istar", Order = -7, DefaultValue = null)]
	public bool IsTarget;

	private bool visible = true;

	[XMLIO(Attr = "visible", Order = -7, DefaultValue = true)]
	public bool Visible
	{
		get
		{
			return visible;
		}
		set
		{
			visible = value;
		}
	}
}
