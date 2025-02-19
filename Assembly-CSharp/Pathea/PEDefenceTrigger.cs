using System;
using PETools;
using UnityEngine;

namespace Pathea;

public class PEDefenceTrigger : MonoBehaviour
{
	[Serializable]
	public class PEDefencePart
	{
		public string name;

		public DefenceType defenceType;

		public DefenceMaterial defenceMaterial;

		public float damageScale;

		public PECapsuleTrigger capsule;

		public void Init()
		{
			capsule.ResetInfo();
			if (damageScale == 0f)
			{
				damageScale = 1f;
			}
		}

		public void Update()
		{
			capsule.Update(Vector3.zero);
		}
	}

	public Transform modelRoot;

	public Transform centerBone;

	public PEDefencePart[] defenceParts;

	private bool m_Active = true;

	private bool m_PartInfoUpdated;

	public bool active
	{
		get
		{
			return m_Active;
		}
		set
		{
			if (m_Active != value)
			{
				m_Active = value;
				Collider component = GetComponent<Collider>();
				if (null != component)
				{
					component.enabled = m_Active;
				}
			}
		}
	}

	public bool GetClosest(Vector3 pos, float maxDistance, out PECapsuleHitResult result)
	{
		UpdateInfo();
		bool result2 = false;
		float num = maxDistance;
		result = null;
		for (int i = 0; i < defenceParts.Length; i++)
		{
			if (!defenceParts[i].capsule.enable)
			{
				continue;
			}
			if (defenceParts[i].capsule.GetClosestPos(pos, out var result3))
			{
				result3.hitDir = Vector3.Normalize(centerBone.position - pos);
				result3.hitDefenceType = defenceParts[i].defenceType;
				result3.damageScale = defenceParts[i].damageScale;
				result = result3;
				return true;
			}
			bool flag = defenceParts[i].capsule.trans == centerBone;
			if (!((!flag) ? (result3.distance > num) : (result3.distance > maxDistance)))
			{
				result3.hitDefenceType = defenceParts[i].defenceType;
				result3.damageScale = defenceParts[i].damageScale;
				result = result3;
				if (flag)
				{
					return true;
				}
				num = result3.distance;
				result2 = true;
			}
		}
		return result2;
	}

	public bool RayCast(Ray castRay, float maxDistance, out PECapsuleHitResult result)
	{
		UpdateInfo();
		bool result2 = false;
		result = null;
		float num = maxDistance;
		Vector3 origin = castRay.origin;
		Vector3 pos = castRay.origin + castRay.direction.normalized * maxDistance;
		for (int i = 0; i < defenceParts.Length; i++)
		{
			if (defenceParts[i].capsule.enable && defenceParts[i].capsule.CheckCollision(origin, pos, out var result3))
			{
				result2 = true;
				float num2 = Vector3.Magnitude(origin - result3.hitPos);
				if (num2 < num)
				{
					num = num2;
					result = result3;
					result.hitDefenceType = defenceParts[i].defenceType;
					result.damageScale = defenceParts[i].damageScale;
					result.distance = Vector3.Distance(result3.hitPos, castRay.origin);
				}
			}
		}
		return result2;
	}

	private void Reset()
	{
		Transform transform = ((!(base.transform.parent != null)) ? base.transform : base.transform.parent);
		PEModelController componentInChildren = transform.GetComponentInChildren<PEModelController>();
		PEInjuredController componentInChildren2 = transform.GetComponentInChildren<PEInjuredController>();
		modelRoot = (centerBone = ((!(null != componentInChildren)) ? base.transform : componentInChildren.transform));
		if (null != componentInChildren2 && null != componentInChildren)
		{
			CheckOldCols(componentInChildren2.transform, componentInChildren.transform);
		}
	}

	private void CheckOldCols(Transform defenceRoot, Transform modelRoot)
	{
		Collider[] componentsInChildren = defenceRoot.GetComponentsInChildren<Collider>(includeInactive: true);
		defenceParts = new PEDefencePart[componentsInChildren.Length];
		for (int i = 0; i < componentsInChildren.Length; i++)
		{
			defenceParts[i] = new PEDefencePart();
			defenceParts[i].name = componentsInChildren[i].name;
			defenceParts[i].defenceType = DefenceType.Flesh;
			defenceParts[i].damageScale = 1f;
			defenceParts[i].capsule = new PECapsuleTrigger();
			defenceParts[i].capsule.axis = PECapsuleTrigger.AxisDir.Inverse_X_Axis;
			defenceParts[i].capsule.trans = PEUtil.GetChild(modelRoot, componentsInChildren[i].name);
		}
	}

	private void Start()
	{
		for (int i = 0; i < defenceParts.Length; i++)
		{
			defenceParts[i].Init();
		}
		if (null == centerBone || null == modelRoot)
		{
			PEModelController componentInChildren = base.transform.GetComponentInChildren<PEModelController>();
			if (null != componentInChildren)
			{
				centerBone = (modelRoot = componentInChildren.transform);
			}
		}
	}

	private void LateUpdate()
	{
		if (this != null && null != modelRoot)
		{
			base.transform.position = modelRoot.position;
			base.transform.rotation = modelRoot.rotation;
			base.transform.localScale = modelRoot.localScale;
		}
		m_PartInfoUpdated = false;
	}

	public void UpdateInfo()
	{
		if (!m_PartInfoUpdated)
		{
			m_PartInfoUpdated = true;
			UpdatePartsInfo();
		}
	}

	private void UpdatePartsInfo()
	{
		for (int i = 0; i < defenceParts.Length; i++)
		{
			defenceParts[i].Update();
		}
	}

	public void SyncBone(PEDefenceTrigger other)
	{
		if (null == other)
		{
			return;
		}
		modelRoot = other.modelRoot;
		centerBone = other.centerBone;
		for (int i = 0; i < defenceParts.Length; i++)
		{
			for (int j = 0; j < other.defenceParts.Length; j++)
			{
				if (defenceParts[i].name == other.defenceParts[j].name)
				{
					defenceParts[i].capsule.trans = other.defenceParts[j].capsule.trans;
				}
			}
		}
		UpdateInfo();
	}
}
