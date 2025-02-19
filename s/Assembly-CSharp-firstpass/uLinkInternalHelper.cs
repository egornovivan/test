using uLink;
using UnityEngine;

[AddComponentMenu("")]
[ExecuteInEditMode]
public sealed class uLinkInternalHelper : InternalHelper
{
	private void Update()
	{
		base.gameObject.hideFlags = HideFlags.None;
	}
}
