using UnityEngine;

namespace PeCustom.Ext;

public static class TransformExt
{
	public static void ResetLocal(this Transform trans)
	{
		trans.localPosition = Vector3.zero;
		trans.localRotation = Quaternion.identity;
		trans.localScale = Vector3.one;
	}
}
