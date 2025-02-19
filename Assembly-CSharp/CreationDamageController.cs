using System.Collections.Generic;
using Pathea;
using UnityEngine;
using WhiteCat;

public class CreationDamageController : MonoBehaviour
{
	private VCESceneSetting m_SceneSetting;

	private BehaviourController m_Controller;

	private List<VCParticlePlayer> m_DamageParticlePlayers;

	private List<VCParticlePlayer> m_ExplodeParticlePlayers;

	private List<Rigidbody> m_ThrowoutList;

	private float m_DestructTick;

	private bool m_IsWreckage;

	private bool m_Sinked;

	public virtual void Init(BehaviourController controller)
	{
		m_Controller = controller;
		m_SceneSetting = controller.creationController.creationData.m_IsoData.m_HeadInfo.FindSceneSetting();
		m_DamageParticlePlayers = new List<VCParticlePlayer>();
		m_ExplodeParticlePlayers = new List<VCParticlePlayer>();
		VCParticlePlayer[] componentsInChildren = GetComponentsInChildren<VCParticlePlayer>(includeInactive: false);
		VCParticlePlayer[] array = componentsInChildren;
		foreach (VCParticlePlayer vCParticlePlayer in array)
		{
			if (vCParticlePlayer.FunctionTag == 1)
			{
				m_DamageParticlePlayers.Add(vCParticlePlayer);
			}
			else if (vCParticlePlayer.FunctionTag == 2)
			{
				m_ExplodeParticlePlayers.Add(vCParticlePlayer);
			}
		}
		m_DestructTick = 120f;
		m_IsWreckage = false;
		m_Sinked = false;
	}

	protected virtual void Update()
	{
		UpdateDamageParticles();
		UpdateDestructTick();
	}

	private void UpdateDamageParticles()
	{
		if (!m_IsWreckage && m_Controller.isDead)
		{
			Explode();
			m_IsWreckage = true;
		}
		for (int i = 0; i < m_DamageParticlePlayers.Count; i++)
		{
			VCParticlePlayer vCParticlePlayer = m_DamageParticlePlayers[i];
			if (!vCParticlePlayer)
			{
				continue;
			}
			if (m_Controller.isDead)
			{
				if (Random.value < (vCParticlePlayer.ReferenceValue - 0.3f) * 0.05f)
				{
					vCParticlePlayer.Effect = VCParticleMgr.GetEffect("Wreckage Spurt", m_SceneSetting);
				}
			}
			else if (m_Controller.hpPercentage < vCParticlePlayer.ReferenceValue * 0.25f)
			{
				vCParticlePlayer.Effect = VCParticleMgr.GetEffect("Fire", m_SceneSetting);
			}
			else if (m_Controller.hpPercentage < vCParticlePlayer.ReferenceValue * 0.5f)
			{
				vCParticlePlayer.Effect = VCParticleMgr.GetEffect("Smoke", m_SceneSetting);
			}
			else
			{
				vCParticlePlayer.Effect = null;
			}
		}
	}

	private void UpdateDestructTick()
	{
		if (m_IsWreckage)
		{
			m_DestructTick -= Time.deltaTime;
		}
		else
		{
			m_DestructTick = 120f;
		}
		if (m_DestructTick < 30f)
		{
			GetComponent<Rigidbody>().isKinematic = false;
		}
		if (m_DestructTick < 0f)
		{
			m_Controller.Destroy();
		}
	}

	protected virtual void FixedUpdate()
	{
		if (m_ThrowoutList != null)
		{
			if (m_ThrowoutList.Count > 0)
			{
				ThrowOutRigidbodies();
				m_ThrowoutList.Clear();
			}
			if (m_DestructTick < 30f && !m_Sinked)
			{
				SinkWreckage();
				m_Sinked = true;
			}
		}
	}

	private void ThrowOutRigidbodies()
	{
		Vector3 vector = base.transform.position;
		if (m_ThrowoutList.Count > 0)
		{
			float num = 0f;
			vector = Vector3.zero;
			Vector3 position = base.transform.position;
			if (m_ThrowoutList[0] != null)
			{
				position = m_ThrowoutList[0].transform.position;
			}
			foreach (Rigidbody throwout in m_ThrowoutList)
			{
				if (throwout != null)
				{
					vector += throwout.transform.position - position;
					num += 1f;
				}
			}
			vector /= num;
			vector += position;
		}
		foreach (Rigidbody throwout2 in m_ThrowoutList)
		{
			if (throwout2 != null)
			{
				Vector3 vector2 = (throwout2.transform.position - vector).normalized * Random.value + Random.insideUnitSphere * 0.3f;
				float num2 = Mathf.Pow(m_SceneSetting.EditorWorldSize.sqrMagnitude, 0.2f) * 5f;
				throwout2.velocity = vector2 * num2;
			}
		}
	}

	private void SinkWreckage()
	{
		Rigidbody[] componentsInChildren = GetComponentsInChildren<Rigidbody>(includeInactive: true);
		Rigidbody[] array = componentsInChildren;
		foreach (Rigidbody rigidbody in array)
		{
			if (rigidbody != GetComponent<Rigidbody>())
			{
				rigidbody.detectCollisions = false;
				rigidbody.useGravity = false;
				rigidbody.angularVelocity = Vector3.zero;
				float num = m_SceneSetting.m_VoxelSize * 3f;
				rigidbody.velocity = Vector3.down * num;
			}
		}
		Collider[] componentsInChildren2 = GetComponentsInChildren<Collider>(includeInactive: true);
		Collider[] array2 = componentsInChildren2;
		foreach (Collider obj in array2)
		{
			Object.Destroy(obj);
		}
	}

	public void Explode()
	{
		ParticleSystem[] componentsInChildren = m_Controller.creationController.partRoot.GetComponentsInChildren<ParticleSystem>();
		if (componentsInChildren != null)
		{
			for (int i = 0; i < componentsInChildren.Length; i++)
			{
				Object.Destroy(componentsInChildren[i]);
			}
		}
		foreach (VCParticlePlayer explodeParticlePlayer in m_ExplodeParticlePlayers)
		{
			explodeParticlePlayer.Effect = VCParticleMgr.GetEffect("Explode", m_SceneSetting);
		}
		List<GameObject> list = new List<GameObject>();
		List<GameObject> list2 = new List<GameObject>();
		MeshRenderer[] componentsInChildren2 = GetComponentsInChildren<MeshRenderer>(includeInactive: false);
		MeshRenderer[] array = componentsInChildren2;
		foreach (MeshRenderer meshRenderer in array)
		{
			if (!list.Contains(meshRenderer.gameObject))
			{
				list.Add(meshRenderer.gameObject);
			}
		}
		VCPart[] componentsInChildren3 = GetComponentsInChildren<VCPart>(includeInactive: true);
		VCPart[] array2 = componentsInChildren3;
		foreach (VCPart vCPart in array2)
		{
			if (!list2.Contains(vCPart.gameObject))
			{
				list2.Add(vCPart.gameObject);
			}
		}
		SkinnedMeshRenderer[] componentsInChildren4 = GetComponentsInChildren<SkinnedMeshRenderer>(includeInactive: false);
		SkinnedMeshRenderer[] array3 = componentsInChildren4;
		foreach (SkinnedMeshRenderer obj in array3)
		{
			Object.Destroy(obj);
		}
		MonoBehaviour[] components = GetComponents<MonoBehaviour>();
		MonoBehaviour[] array4 = components;
		foreach (MonoBehaviour monoBehaviour in array4)
		{
			if (monoBehaviour is CreationDamageController)
			{
				continue;
			}
			if (monoBehaviour is CreationController)
			{
				monoBehaviour.enabled = false;
			}
			else if (!(monoBehaviour is PeEntity))
			{
				if (monoBehaviour is BehaviourController)
				{
					monoBehaviour.enabled = false;
					(monoBehaviour as BehaviourController).audioSource.PlayOneShot(PEVCConfig.instance.explotionSound, PEVCConfig.instance.explotionVolume * SystemSettingData.Instance.AbsEffectVolume);
				}
				else
				{
					monoBehaviour.enabled = false;
					Object.Destroy(monoBehaviour);
				}
			}
		}
		for (int n = 0; n < 3; n++)
		{
			foreach (GameObject item in list2)
			{
				components = item.GetComponents<MonoBehaviour>();
				MonoBehaviour[] array5 = components;
				foreach (MonoBehaviour monoBehaviour2 in array5)
				{
					if (!(monoBehaviour2 is VCParticlePlayer))
					{
						Object.Destroy(monoBehaviour2);
					}
				}
			}
		}
		Light[] componentsInChildren5 = GetComponentsInChildren<Light>();
		Light[] array6 = componentsInChildren5;
		foreach (Light obj2 in array6)
		{
			Object.Destroy(obj2);
		}
		ParticlePlayer[] componentsInChildren6 = GetComponentsInChildren<ParticlePlayer>();
		ParticlePlayer[] array7 = componentsInChildren6;
		foreach (ParticlePlayer obj3 in array7)
		{
			Object.Destroy(obj3);
		}
		WheelCollider[] componentsInChildren7 = GetComponentsInChildren<WheelCollider>(includeInactive: true);
		WheelCollider[] array8 = componentsInChildren7;
		foreach (WheelCollider obj4 in array8)
		{
			Object.Destroy(obj4);
		}
		AudioSource[] componentsInChildren8 = GetComponentsInChildren<AudioSource>(includeInactive: true);
		BehaviourController component = GetComponent<BehaviourController>();
		AudioSource audioSource = null;
		if ((bool)component)
		{
			audioSource = component.audioSource;
		}
		AudioSource[] array9 = componentsInChildren8;
		foreach (AudioSource audioSource2 in array9)
		{
			if (audioSource2 != audioSource)
			{
				audioSource2.enabled = false;
			}
		}
		CreationWaterMask[] componentsInChildren9 = GetComponentsInChildren<CreationWaterMask>(includeInactive: true);
		CreationWaterMask[] array10 = componentsInChildren9;
		foreach (CreationWaterMask creationWaterMask in array10)
		{
			Object.Destroy(creationWaterMask.m_WaterMask);
		}
		if (GetComponent<Rigidbody>() != null)
		{
			GetComponent<Rigidbody>().isKinematic = true;
			GetComponent<Rigidbody>().useGravity = false;
			GetComponent<Rigidbody>().detectCollisions = false;
		}
		else
		{
			base.gameObject.AddComponent<Rigidbody>();
			GetComponent<Rigidbody>().isKinematic = true;
			GetComponent<Rigidbody>().useGravity = false;
			GetComponent<Rigidbody>().detectCollisions = false;
		}
		foreach (GameObject item2 in list)
		{
			if (item2.GetComponent<Collider>() == null)
			{
				item2.AddComponent<BoxCollider>();
			}
			else
			{
				item2.GetComponent<Collider>().enabled = true;
			}
			if (item2.GetComponent<Collider>() is MeshCollider)
			{
				((MeshCollider)item2.GetComponent<Collider>()).convex = true;
			}
			if (item2.GetComponent<Rigidbody>() == null && Random.value < 0.7f)
			{
				item2.AddComponent<Rigidbody>();
			}
			if (item2.GetComponent<Rigidbody>() != null)
			{
				item2.GetComponent<Rigidbody>().mass = 10f;
			}
		}
		m_ThrowoutList = new List<Rigidbody>();
		foreach (GameObject item3 in list)
		{
			if (item3.GetComponent<Rigidbody>() != null)
			{
				item3.GetComponent<Rigidbody>().isKinematic = false;
				m_ThrowoutList.Add(item3.GetComponent<Rigidbody>());
			}
			else
			{
				Object.Destroy(item3);
			}
		}
	}
}
