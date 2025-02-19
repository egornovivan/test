using UnityEngine;

public abstract class CamModifier : MonoBehaviour
{
	[HideInInspector]
	public CamController m_Controller;

	[HideInInspector]
	public Camera m_TargetCam;

	public int m_Tag;

	public abstract void Do();

	private void OnDestroy()
	{
		Object.Destroy(base.gameObject);
	}

	public static bool MatchName(CamModifier iter, string name)
	{
		if (iter == null)
		{
			return false;
		}
		return iter.name.ToLower() == name.ToLower();
	}
}
