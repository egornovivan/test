using Pathea.Effect;
using SkillSystem;
using UnityEngine;

public class ShootEffectMgr : MonoBehaviour, ISkEffectEntity
{
	public enum EffectType
	{
		EffectType_Shoot,
		EffectType_BowShoot,
		EffectType_LaserShoot,
		EffectType_ShotgunShoot
	}

	public SkInst m_SkInst;

	public EffectType m_Type;

	public SkInst Inst
	{
		set
		{
			m_SkInst = value;
		}
	}

	private void Start()
	{
		int type = (int)m_Type;
		if (null != PECameraMan.Instance && null != EntityCreateMgr.Instance)
		{
			switch (type)
			{
			case 0:
				PECameraMan.Instance.ShootEffect(EntityCreateMgr.Instance.GetPlayerDir(), 0f, 1f);
				break;
			case 1:
				PECameraMan.Instance.BowShootEffect(EntityCreateMgr.Instance.GetPlayerDir(), 0f, 1f);
				break;
			case 2:
				PECameraMan.Instance.LaserShootEffect(EntityCreateMgr.Instance.GetPlayerDir(), 0f, 1f);
				break;
			case 3:
				PECameraMan.Instance.ShotgunShootEffect(EntityCreateMgr.Instance.GetPlayerDir(), 0f, 1f);
				break;
			}
		}
	}
}
