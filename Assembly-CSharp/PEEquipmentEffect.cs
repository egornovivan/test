using Pathea;
using Pathea.Effect;
using UnityEngine;

public class PEEquipmentEffect : MonoBehaviour
{
	public int effectID = 97;

	public bool hideInFirstPerson;

	private MainPlayerCmpt m_MainPlayer;

	private ControllableEffect m_Effect;

	private void Start()
	{
		m_MainPlayer = GetComponentInParent<MainPlayerCmpt>();
		m_Effect = new ControllableEffect(effectID, base.transform);
	}

	private void Update()
	{
		if (hideInFirstPerson && null != m_MainPlayer && m_Effect != null && m_MainPlayer.firstPersonCtrl == m_Effect.active)
		{
			m_Effect.active = !m_MainPlayer.firstPersonCtrl;
		}
	}

	private void OnDestroy()
	{
		if (m_Effect != null)
		{
			m_Effect.Destory();
			m_Effect = null;
		}
	}
}
