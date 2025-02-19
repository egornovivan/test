using Pathea.Effect;
using SkillSystem;
using UnityEngine;

public class PEDeleteCaster : MonoBehaviour, ISkEffectEntity
{
	private Transform m_Delete;

	public SkInst Inst
	{
		set
		{
			if (value != null)
			{
				m_Delete = value._caster.transform;
			}
		}
	}

	private void Start()
	{
		if (m_Delete != null)
		{
			Object.Destroy(m_Delete.gameObject);
		}
		Object.Destroy(base.gameObject);
	}
}
