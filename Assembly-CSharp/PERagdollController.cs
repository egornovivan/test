using System;
using System.Collections;
using AnimFollow;
using PETools;
using UnityEngine;

public class PERagdollController : RagdollControl_AF
{
	private bool m_GetupReady;

	private bool m_RagdollActive;

	private IRagdollHandler m_Handler;

	[SerializeField]
	private Renderer[] m_Renderers;

	[SerializeField]
	private StandardAlphaAnimator m_Alpha;

	[SerializeField]
	private PERagdollEffect m_RagdollEffect;

	[SerializeField]
	private Collider[] m_Colliders;

	public Collider[] colliders
	{
		get
		{
			if (m_Colliders == null || m_Colliders.Length == 0)
			{
				m_Colliders = PEUtil.GetCmpts<Collider>(base.transform);
			}
			return m_Colliders;
		}
	}

	public IRagdollHandler Handler => m_Handler;

	public void ResetRagdoll()
	{
		m_Alpha = PEUtil.GetCmpt<StandardAlphaAnimator>(base.transform);
		m_Renderers = PEUtil.GetCmpts<Renderer>(base.transform);
		m_Colliders = PEUtil.GetCmpts<Collider>(base.transform);
		slaveRigidBodies = PEUtil.GetCmpts<Rigidbody>(base.transform);
		if (0 < slaveRigidBodies.Length)
		{
			ragdollRootBone = slaveRigidBodies[0].transform;
		}
		animFollow = GetComponent<AnimFollow_AF>();
		if (null != animFollow)
		{
			master = animFollow.master;
		}
		if (null != master)
		{
			masterRootBone = PEUtil.GetChild(master.transform, ragdollRootBone.name);
			anim = PEUtil.GetCmpt<Animator>(master.transform);
		}
		if (m_RagdollEffect == null)
		{
			m_RagdollEffect = base.gameObject.AddComponent<PERagdollEffect>();
		}
		if (m_RagdollEffect != null)
		{
			m_RagdollEffect.ResetRagdoll();
		}
	}

	public void SetHandler(IRagdollHandler handler)
	{
		m_Handler = handler;
		m_Handler.OnRagdollBuild(base.gameObject);
	}

	public void SetMaterials(Material[] materials)
	{
		if (materials != null && materials.Length > 0)
		{
			for (int i = 0; i < m_Renderers.Length; i++)
			{
				m_Renderers[i].materials = materials;
			}
		}
	}

	public bool IsRagdoll()
	{
		return m_RagdollActive;
	}

	public void FadeIn(float time = 2f)
	{
		if (m_Alpha != null)
		{
			m_Alpha.FadeIn(time);
		}
	}

	public void FadeOut(float time = 2f)
	{
		if (m_Alpha != null)
		{
			m_Alpha.FadeOut(time);
		}
	}

	public void HideView(float time)
	{
		if (m_Alpha != null)
		{
			m_Alpha.SetAlpha(0f);
			m_Alpha.FadeOut(time);
		}
	}

	public void ActivateRenderer(bool value)
	{
		ResetRagdoll();
		Renderer[] renderers = m_Renderers;
		foreach (Renderer renderer in renderers)
		{
			if (null != renderer)
			{
				renderer.enabled = value;
			}
		}
	}

	public void ActivatePhysics(bool value)
	{
		try
		{
			for (int i = 0; i < slaveRigidBodies.Length; i++)
			{
				if (slaveRigidBodies[i] != null)
				{
					if (animFollow.active && value)
					{
						slaveRigidBodies[i].isKinematic = false;
					}
					else
					{
						slaveRigidBodies[i].isKinematic = true;
					}
				}
			}
		}
		catch
		{
			throw new NullReferenceException(base.name);
		}
	}

	public void Activate(RagdollHitInfo hitInfo, bool isGetupReady)
	{
		shotByBullet = true;
		animFollow.active = true;
		m_GetupReady = isGetupReady;
		StartCoroutine(AddForce(hitInfo));
	}

	public void Deactivate(bool immediately = false)
	{
		if (immediately)
		{
			falling = false;
			gettingUp = false;
			animFollow.active = false;
			OnGetupFinished();
		}
		else
		{
			m_GetupReady = true;
		}
	}

	public void SmrBuild(SkinnedMeshRenderer renderer)
	{
		if (renderer != null)
		{
			m_Renderers = new Renderer[1] { renderer };
			renderer.rootBone = ragdollRootBone;
			renderer.enabled = animFollow.active;
		}
	}

	private IEnumerator AddForce(RagdollHitInfo hitInfo)
	{
		if (hitInfo != null && !(hitInfo.hitTransform == null))
		{
			yield return new WaitForFixedUpdate();
			Rigidbody r = hitInfo.hitTransform.GetComponent<Rigidbody>();
			if (r != null)
			{
				r.AddForceAtPosition(hitInfo.hitForce * 100f, hitInfo.hitPoint);
			}
		}
	}

	public bool IsReadyGetUp()
	{
		return falling && ragdollRootBone.GetComponent<Rigidbody>().velocity.magnitude < settledSpeed;
	}

	protected override bool IsGetupReady()
	{
		return m_GetupReady;
	}

	protected override void OnFallBegin()
	{
		if (m_RagdollEffect != null)
		{
			m_RagdollEffect.IsActive = true;
		}
		m_RagdollActive = true;
		m_Handler.OnFallBegin(base.gameObject);
	}

	protected override void OnFallFinished()
	{
		m_Handler.OnFallFinished(base.gameObject);
	}

	protected override void OnGetupBegin()
	{
		m_Handler.OnGetupBegin(base.gameObject);
	}

	protected override void OnGetupFinished()
	{
		if (m_RagdollEffect != null)
		{
			m_RagdollEffect.IsActive = false;
		}
		m_RagdollActive = false;
		m_Handler.OnGetupFinished(base.gameObject);
	}
}
