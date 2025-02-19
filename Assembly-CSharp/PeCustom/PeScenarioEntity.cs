using Pathea;
using UnityEngine;

namespace PeCustom;

public class PeScenarioEntity : MonoBehaviour
{
	public SpawnPoint spawnPoint;

	private PeEntity m_Entity;

	public PeEntity entity
	{
		get
		{
			if (m_Entity == null)
			{
				m_Entity = base.gameObject.GetComponent<PeEntity>();
			}
			return m_Entity;
		}
	}

	private void Update()
	{
		if (!(entity == null))
		{
			spawnPoint.entityPos = entity.peTrans.position;
		}
	}
}
