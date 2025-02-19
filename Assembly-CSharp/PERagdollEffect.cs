using System;
using PETools;
using UnityEngine;

public class PERagdollEffect : MonoBehaviour
{
	private static int s_Layer;

	private static int s_TerrainLayer;

	private static GameObject s_ParticleSmall;

	private static GameObject s_ParticleLarge;

	private bool m_IsActive;

	[SerializeField]
	private Rigidbody[] m_Rigibodys;

	[SerializeField]
	private PECollision[] m_Collisions;

	public bool IsActive
	{
		set
		{
			m_IsActive = value;
		}
	}

	public void ResetRagdoll()
	{
		m_Rigibodys = PEUtil.GetCmpts<Rigidbody>(base.transform);
		if (m_Rigibodys == null || m_Rigibodys.Length <= 0)
		{
			return;
		}
		m_Collisions = new PECollision[m_Rigibodys.Length];
		for (int i = 0; i < m_Rigibodys.Length; i++)
		{
			PECollision[] components = m_Rigibodys[i].gameObject.GetComponents<PECollision>();
			for (int j = 0; j < components.Length; j++)
			{
				if (m_Collisions[i] == null)
				{
					m_Collisions[i] = components[j];
				}
				else
				{
					UnityEngine.Object.DestroyImmediate(components[j], allowDestroyingAssets: true);
				}
			}
			if (m_Collisions[i] == null)
			{
				m_Collisions[i] = m_Rigibodys[i].gameObject.AddComponent<PECollision>();
			}
		}
	}

	private void Awake()
	{
		if (s_ParticleSmall == null)
		{
			s_ParticleSmall = AssetsLoader.Instance.LoadPrefabImm("Prefab/Particle/FX_enemyFall") as GameObject;
		}
		if (s_ParticleLarge == null)
		{
			s_ParticleLarge = AssetsLoader.Instance.LoadPrefabImm("Prefab/Particle/FX_enemyFall_large") as GameObject;
		}
		if (s_Layer == 0)
		{
			s_Layer = 71680;
		}
		if (s_TerrainLayer == 0)
		{
			s_TerrainLayer = 4096;
		}
		if (m_Collisions != null && m_Collisions.Length > 0)
		{
			for (int i = 0; i < m_Collisions.Length; i++)
			{
				PECollision obj = m_Collisions[i];
				obj.enter = (Action<Collider, Collision>)Delegate.Combine(obj.enter, new Action<Collider, Collision>(OnCollisionChildEnter));
			}
		}
	}

	private void FixedUpdate()
	{
		if (m_Rigibodys == null || m_Rigibodys.Length == 0 || !m_IsActive)
		{
			return;
		}
		for (int i = 0; i < m_Rigibodys.Length; i++)
		{
			if (!(m_Rigibodys[i] == null) && PEUtil.GetWaterSurfaceHeight(m_Rigibodys[i].worldCenterOfMass, out var waterHeight) && !Physics.Raycast(m_Rigibodys[i].position, Vector3.up, 3f, s_Layer))
			{
				float num = Mathf.Clamp(waterHeight - m_Rigibodys[i].worldCenterOfMass.y, 0f, 2f);
				m_Rigibodys[i].AddForce((-Physics.gravity + Vector3.up * num) * m_Rigibodys[i].mass);
			}
		}
	}

	private void OnDestroy()
	{
		if (m_Collisions != null && m_Collisions.Length > 0)
		{
			for (int i = 0; i < m_Collisions.Length; i++)
			{
				PECollision obj = m_Collisions[i];
				obj.enter = (Action<Collider, Collision>)Delegate.Remove(obj.enter, new Action<Collider, Collision>(OnCollisionChildEnter));
			}
		}
	}

	private float GetRadius(Collider collider)
	{
		BoxCollider boxCollider = collider as BoxCollider;
		SphereCollider sphereCollider = collider as SphereCollider;
		CapsuleCollider capsuleCollider = collider as CapsuleCollider;
		if (boxCollider != null)
		{
			return boxCollider.size.x * boxCollider.size.y * boxCollider.size.z;
		}
		if (sphereCollider != null)
		{
			return sphereCollider.radius * sphereCollider.radius * sphereCollider.radius * (float)Math.PI * 1.334f;
		}
		if (capsuleCollider != null)
		{
			return capsuleCollider.radius * capsuleCollider.radius * (float)Math.PI * capsuleCollider.height;
		}
		return 0f;
	}

	private void OnCollisionChildEnter(Collider col, Collision info)
	{
		int num = 1 << info.gameObject.layer;
		if ((s_Layer & num) != 0 && !(info.relativeVelocity.sqrMagnitude < 25f))
		{
			GameObject gameObject = ((!(GetRadius(col) < 1f)) ? (UnityEngine.Object.Instantiate(s_ParticleLarge, info.contacts[0].point, Quaternion.identity) as GameObject) : (UnityEngine.Object.Instantiate(s_ParticleSmall, info.contacts[0].point, Quaternion.identity) as GameObject));
			if (gameObject != null)
			{
				UnityEngine.Object.Destroy(gameObject, 5f);
			}
		}
	}
}
