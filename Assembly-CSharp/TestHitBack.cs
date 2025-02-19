using Pathea;
using RootMotion.FinalIK.Demos;
using UnityEngine;

public class TestHitBack : HitReactionCharacter
{
	public LayerMask m_CheckLayer;

	private Motion_Beat m_Beat;

	public float m_HitPower = 100f;

	private void Start()
	{
		if (null != m_Beat)
		{
			m_Beat = GetComponentInParent<Motion_Beat>();
		}
	}

	public void SetHandler(Motion_Move move, ViewCmpt view, Motion_Beat beat)
	{
		m_Beat = beat;
	}

	protected override void Update()
	{
		if (null != cam)
		{
			CheckHit();
		}
	}

	private void CheckHit()
	{
		if (!(null != m_Beat) || !Input.GetMouseButtonDown(0))
		{
			return;
		}
		Ray ray = cam.ScreenPointToRay(Input.mousePosition);
		RaycastHit hitInfo = default(RaycastHit);
		if (Physics.Raycast(ray, out hitInfo, 100f, m_CheckLayer.value))
		{
			PEDefenceTrigger component = hitInfo.collider.GetComponent<PEDefenceTrigger>();
			if (component.RayCast(ray, 1000f, out var result))
			{
				m_Beat.Beat(GetComponentInParent<SkAliveEntity>(), result.hitTrans, ray.direction, m_HitPower);
			}
		}
	}
}
