using UnityEngine;

public class VCEUIColorPickWnd : MonoBehaviour
{
	public TweenScale m_ScaleTweener;

	public VCEUIColorPick m_ColorPick;

	private void Start()
	{
		m_ScaleTweener.Play(forward: true);
	}

	private void Update()
	{
		if (!Input.GetMouseButtonDown(0) || !(base.transform.localScale.sqrMagnitude > 2.99f))
		{
			return;
		}
		bool flag = false;
		if (Physics.Raycast(VCEInput.s_UIRay, out var hitInfo, 100f, VCConfig.s_UILayerMask))
		{
			Transform parent = hitInfo.collider.transform;
			while (parent != null)
			{
				if (parent.gameObject == base.gameObject)
				{
					flag = true;
					break;
				}
				parent = parent.parent;
			}
		}
		if (!flag)
		{
			m_ScaleTweener.Play(forward: false);
			Invoke("SelfDestroy", 0.5f);
		}
	}

	private void SelfDestroy()
	{
		Object.Destroy(base.gameObject);
	}
}
