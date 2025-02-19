using System.Collections.Generic;
using SkillSystem;
using UnityEngine;
using WhiteCat;

namespace Pathea;

public class ImpactAtkTriggrer : MonoBehaviour
{
	private PeEntity m_Entity;

	private Rigidbody m_RigidBody;

	private void Awake()
	{
		m_Entity = GetComponentInParent<PeEntity>();
		m_RigidBody = GetComponent<Rigidbody>();
	}

	public static float MinCmpt(Vector3 v)
	{
		float num = v.x;
		if (v.y < num)
		{
			num = v.y;
		}
		if (v.z < num)
		{
			num = v.z;
		}
		return num;
	}

	private void OnCollisionEnter(Collision colInfo)
	{
		GameObject gameObject = colInfo.collider.gameObject;
		if (gameObject.layer != 14)
		{
			if (gameObject.layer == 21)
			{
				if (PeGameMgr.IsStory || PeGameMgr.IsAdventure)
				{
					PeEntity componentInParent = GetComponentInParent<PeEntity>();
					if (PeGameMgr.IsStory)
					{
						if (componentInParent != null)
						{
							if (componentInParent.Field == MovementField.Sky && componentInParent.commonCmpt.entityProto.proto == EEntityProto.Monster)
							{
								if (componentInParent.maxRadius < MinCmpt(colInfo.collider.bounds.extents) * 2f)
								{
									return;
								}
							}
							else if ((bool)componentInParent.carrier && colInfo.relativeVelocity.magnitude * componentInParent.carrier.rigidbody.mass < MinCmpt(colInfo.collider.bounds.extents) * PEVCConfig.instance.treeHardness)
							{
								return;
							}
							GlobalTreeInfo globalTreeInfo = LSubTerrainMgr.TryGetTreeInfo(gameObject);
							if (globalTreeInfo != null)
							{
								LSubTerrainMgr.DeleteTree(gameObject);
								LSubTerrainMgr.RefreshAllLayerTerrains();
								SkEntitySubTerrain.Instance.SetTreeHp(globalTreeInfo.WorldPos, 0f);
								StroyManager.Instance.TreeCutDown(componentInParent.position, globalTreeInfo._treeInfo, globalTreeInfo.WorldPos);
							}
						}
					}
					else if (componentInParent != null)
					{
						if (componentInParent.Field == MovementField.Sky && componentInParent.commonCmpt.entityProto.proto == EEntityProto.Monster)
						{
							if (componentInParent.maxRadius < MinCmpt(colInfo.collider.bounds.extents) * 2f)
							{
								return;
							}
						}
						else if ((bool)componentInParent.carrier && colInfo.relativeVelocity.magnitude * componentInParent.carrier.rigidbody.mass < MinCmpt(colInfo.collider.bounds.extents) * PEVCConfig.instance.treeHardness)
						{
							return;
						}
						TreeInfo treeInfo = RSubTerrainMgr.TryGetTreeInfo(gameObject);
						if (treeInfo != null)
						{
							RSubTerrainMgr.DeleteTree(gameObject);
							RSubTerrainMgr.RefreshAllLayerTerrains();
							SkEntitySubTerrain.Instance.SetTreeHp(treeInfo.m_pos, 0f);
							StroyManager.Instance.TreeCutDown(componentInParent.position, treeInfo, treeInfo.m_pos);
						}
					}
				}
			}
			else if (gameObject.layer == 8 || gameObject.layer == 10)
			{
				PeEntity componentInParent2 = gameObject.GetComponentInParent<PeEntity>();
				if (componentInParent2 != null && m_Entity != null && m_Entity.carrier != null && colInfo.relativeVelocity.sqrMagnitude > 4f)
				{
					NetCmpt component = m_Entity.GetComponent<NetCmpt>();
					if (component == null || component.IsController)
					{
						float num = 8.888889E-05f;
						float num2 = m_Entity.carrier.creationController.creationData.m_Attribute.m_Weight * 0.001f;
						float sqrMagnitude = colInfo.relativeVelocity.sqrMagnitude;
						float num3 = Mathf.Clamp(num2 * sqrMagnitude * num, 0f, 2f);
						if (m_RigidBody == null || m_RigidBody.velocity.sqrMagnitude < 25f || (double)num3 < 0.01)
						{
							num3 = 0f;
						}
						else
						{
							PECapsuleHitResult pECapsuleHitResult = new PECapsuleHitResult();
							pECapsuleHitResult.selfTrans = base.transform;
							pECapsuleHitResult.hitTrans = gameObject.transform;
							pECapsuleHitResult.hitPos = colInfo.contacts[0].point;
							pECapsuleHitResult.hitDir = -colInfo.contacts[0].normal;
							pECapsuleHitResult.damageScale = num3;
							m_Entity.skEntity.CollisionCheck(pECapsuleHitResult);
						}
					}
				}
			}
		}
		if (gameObject.layer != 12 && gameObject.layer != 14 && gameObject.layer != 13 && gameObject.layer != 11 && gameObject.layer != 16 && gameObject.layer != 19)
		{
			return;
		}
		bool flag = false;
		if (gameObject.layer == 12)
		{
			for (int i = 0; i < colInfo.contacts.Length; i++)
			{
				if (colInfo.contacts[i].thisCollider.gameObject.GetComponentInParent<VCPVehicleWheel>() != null)
				{
					flag = true;
				}
			}
		}
		if (flag || !(m_Entity != null) || !(m_Entity.carrier != null) || !(colInfo.relativeVelocity.sqrMagnitude > 4f))
		{
			return;
		}
		NetCmpt component2 = m_Entity.GetComponent<NetCmpt>();
		if (component2 == null || component2.IsController)
		{
			float num4 = 8.888889E-05f;
			float num5 = m_Entity.carrier.creationController.creationData.m_Attribute.m_Weight * 0.001f;
			float sqrMagnitude2 = colInfo.relativeVelocity.sqrMagnitude;
			float num6 = Mathf.Clamp(num5 * sqrMagnitude2 * num4, 0f, 2f);
			Vector3 velocity = m_RigidBody.velocity;
			float num7 = Mathf.Sqrt(velocity.x * velocity.x + velocity.z * velocity.z) * 3.6f;
			if (num7 < 45f || (double)num6 < 0.01)
			{
				num6 = 0f;
				return;
			}
			SkEntity.MountBuff(m_Entity.skEntity, 30200174, new List<int> { 0 }, new List<float> { num6 });
		}
	}
}
