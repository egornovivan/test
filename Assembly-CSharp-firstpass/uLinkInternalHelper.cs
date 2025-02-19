using uLink;
using UnityEngine;

[ExecuteInEditMode]
[AddComponentMenu("")]
public sealed class uLinkInternalHelper : InternalHelper
{
	private void Update()
	{
		base.gameObject.hideFlags = HideFlags.None;
	}
}
