using Behave.Runtime;
using Pathea;
using UnityEngine;

public class PETower : MonoBehaviour
{
	public int campid;

	public int damageid;

	public int skillid;

	public string behavePath;

	private PeEntity m_Entity;

	private void Awake()
	{
		BTResolver.RegisterToCache(behavePath);
	}

	private void Start()
	{
		m_Entity = GetComponentInParent<PeEntity>();
		if (!(m_Entity != null))
		{
			return;
		}
		m_Entity.SetAttribute(AttribType.CampID, campid);
		m_Entity.SetAttribute(AttribType.DamageID, damageid);
		TowerCmpt towerCmpt = m_Entity.gameObject.AddComponent<TowerCmpt>();
		towerCmpt.SkillID = skillid;
		towerCmpt.NeedVoxel = false;
		towerCmpt.OnMsg(EMsg.View_Prefab_Build, null, base.transform.parent.GetComponent<BiologyViewRoot>());
		m_Entity.Tower = towerCmpt;
		BehaveCmpt behaveCmpt = m_Entity.gameObject.AddComponent<BehaveCmpt>();
		behaveCmpt.SetAssetPath(behavePath);
		behaveCmpt.OnMsg(EMsg.View_Model_Build);
		if (m_Entity.netCmpt != null)
		{
			m_Entity.netCmpt.OnMsg(EMsg.Net_Instantiate, campid, damageid);
			if (m_Entity.netCmpt.IsController)
			{
				behaveCmpt.OnMsg(EMsg.Net_Controller);
			}
			else
			{
				behaveCmpt.OnMsg(EMsg.Net_Proxy);
			}
		}
		m_Entity.BehaveCmpt = behaveCmpt;
		TargetCmpt targetCmpt = m_Entity.gameObject.AddComponent<TargetCmpt>();
		targetCmpt.OnMsg(EMsg.View_Model_Build, null, base.transform.parent.GetComponent<BiologyViewRoot>());
		m_Entity.target = targetCmpt;
	}
}
