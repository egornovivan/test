using System.Collections.Generic;
using PETools;
using SkillSystem;
using UnityEngine;
using WhiteCat;

namespace Pathea.Projectile;

public class ProjectileBuilder : Singleton<ProjectileBuilder>
{
	public class ProjectileRequest
	{
		public int m_ID;

		public int m_Index;

		public Transform m_Caster;

		public Transform m_Emitter;

		public Vector3 m_Position;

		public Quaternion m_Rotation;

		public Transform m_TargetTrans;

		public Vector3 m_TargetPos;

		public ProjectileRequest(int _id, Transform _caster, Transform _emitter, Vector3 _pos, Quaternion _rot, Transform _target, Vector3 _targetPos, int _index)
		{
			m_ID = _id;
			m_Caster = _caster;
			m_Emitter = _emitter;
			m_Position = _pos;
			m_Rotation = _rot;
			m_TargetTrans = _target;
			m_TargetPos = _targetPos;
			m_Index = _index;
		}

		private Transform GetEmit()
		{
			return m_Emitter;
		}

		private Vector3 GetPosition()
		{
			return (!(m_Emitter != null)) ? m_Position : m_Emitter.position;
		}

		private Quaternion GetRotation()
		{
			return (!(m_Emitter != null)) ? m_Rotation : m_Emitter.rotation;
		}

		private Transform GetTargetTrans()
		{
			return m_TargetTrans;
		}

		private Vector3 GetTargetPosition()
		{
			return m_TargetPos;
		}

		public void Create(Transform parent)
		{
			ProjectileData projectileData = ProjectileData.GetProjectileData(m_ID);
			if (projectileData == null || m_Caster == null)
			{
				return;
			}
			GameObject gameObject = Resources.Load(projectileData._path) as GameObject;
			if (gameObject != null)
			{
				GameObject gameObject2 = Object.Instantiate(gameObject);
				gameObject2.transform.parent = parent;
				gameObject2.transform.position = GetPosition();
				gameObject2.transform.rotation = GetRotation();
				SkProjectile component = gameObject2.GetComponent<SkProjectile>();
				if (component != null)
				{
					component.SetData(projectileData, m_Caster, m_Emitter, GetTargetTrans(), GetTargetPosition(), m_Index);
				}
				EffectLateupdateHelper component2 = gameObject2.GetComponent<EffectLateupdateHelper>();
				if (null != component2)
				{
					component2.Init(m_Emitter);
				}
			}
		}
	}

	private List<ProjectileRequest> m_ReqList = new List<ProjectileRequest>();

	private List<ProjectileRequest> m_Reqs = new List<ProjectileRequest>();

	private void LateUpdate()
	{
		if (m_ReqList.Count > 0)
		{
			m_Reqs.Clear();
			m_Reqs.AddRange(m_ReqList);
			for (int num = m_Reqs.Count - 1; num >= 0; num--)
			{
				ProjectileRequest projectileRequest = m_ReqList[num];
				projectileRequest.Create(base.transform);
				m_ReqList.Remove(projectileRequest);
			}
		}
	}

	public void Register(int id, Transform caster, Transform target, int index = 0, bool immediately = false)
	{
		ProjectileData projectileData = ProjectileData.GetProjectileData(id);
		if (projectileData != null && !(caster == null))
		{
			Transform emitter = null;
			if (!string.IsNullOrEmpty(projectileData._bone) && "0" != projectileData._bone)
			{
				PeEntity component = caster.GetComponent<PeEntity>();
				emitter = ((!(null != component)) ? PEUtil.GetChild(caster, projectileData._bone) : component.GetChild(projectileData._bone));
			}
			ProjectileRequest projectileRequest = new ProjectileRequest(id, caster, emitter, Vector3.zero, Quaternion.identity, target, Vector3.zero, index);
			if (immediately)
			{
				projectileRequest.Create(base.transform);
			}
			else
			{
				m_ReqList.Add(projectileRequest);
			}
		}
	}

	public void Register(int id, Transform caster, Transform emitter, Transform target, int index = 0, bool immediately = false)
	{
		ProjectileRequest projectileRequest = new ProjectileRequest(id, caster, emitter, Vector3.zero, Quaternion.identity, target, Vector3.zero, index);
		if (immediately)
		{
			projectileRequest.Create(base.transform);
		}
		else
		{
			m_ReqList.Add(projectileRequest);
		}
	}

	public void Register(int id, Transform caster, Transform emitter, Vector3 targetPosition, int index = 0, bool immediately = false)
	{
		ProjectileRequest projectileRequest = new ProjectileRequest(id, caster, emitter, Vector3.zero, Quaternion.identity, null, targetPosition, index);
		if (immediately)
		{
			projectileRequest.Create(base.transform);
		}
		else
		{
			m_ReqList.Add(projectileRequest);
		}
	}

	public void Register(int id, Transform caster, Vector3 pos, Quaternion rot, Transform target, int index = 0, bool immediately = false)
	{
		ProjectileRequest projectileRequest = new ProjectileRequest(id, caster, null, pos, rot, target, Vector3.zero, index);
		if (immediately)
		{
			projectileRequest.Create(base.transform);
		}
		else
		{
			m_ReqList.Add(projectileRequest);
		}
	}

	public void Register(int id, Transform caster, Vector3 pos, Quaternion rot, Vector3 targetPosition, int index = 0, bool immediately = false)
	{
		ProjectileRequest projectileRequest = new ProjectileRequest(id, caster, null, pos, rot, null, targetPosition, index);
		if (immediately)
		{
			projectileRequest.Create(base.transform);
		}
		else
		{
			m_ReqList.Add(projectileRequest);
		}
	}

	public void Register(int id, Transform caster, SkRuntimeInfo info, int index = 0, bool immediately = false)
	{
		ProjectileData projectileData = ProjectileData.GetProjectileData(id);
		if (projectileData == null)
		{
			return;
		}
		Transform transform = null;
		if (!string.IsNullOrEmpty(projectileData._bone) && "0" != projectileData._bone)
		{
			PeEntity component = caster.GetComponent<PeEntity>();
			transform = ((!(null != component)) ? PEUtil.GetChild(caster, projectileData._bone) : component.GetChild(projectileData._bone));
		}
		Transform transform2 = null;
		if (info.Target != null)
		{
			PeTrans component2 = info.Target.GetComponent<PeTrans>();
			if (component2 != null)
			{
				transform2 = component2.trans;
			}
		}
		ShootTargetPara shootTargetPara = info.Para as ShootTargetPara;
		SkCarrierCanonPara skCarrierCanonPara = info.Para as SkCarrierCanonPara;
		if (shootTargetPara != null)
		{
			if (transform != null)
			{
				Register(id, caster, transform, shootTargetPara.m_TargetPos, index, immediately);
			}
		}
		else if (skCarrierCanonPara != null)
		{
			BehaviourController component3 = info.Caster.GetComponent<BehaviourController>();
			IProjectileData projectileData2 = component3.GetProjectileData(skCarrierCanonPara);
			if (transform2 != null)
			{
				Register(id, caster, projectileData2.emissionTransform, transform2, index, immediately);
			}
			else
			{
				Register(id, caster, projectileData2.emissionTransform, projectileData2.targetPosition, index, immediately);
			}
		}
		else if (transform != null && transform != null)
		{
			Register(id, caster, transform, transform2, index, immediately);
		}
	}
}
