using Pathea;
using Pathea.Effect;
using PETools;
using UnityEngine;

public class PEModelController : MonoBehaviour
{
	public GameObject deathModel;

	public int deathEffectID;

	private AnimatorCmpt m_AnimCmpt;

	[SerializeField]
	private Rigidbody m_Rigidbody;

	[SerializeField]
	private Collider[] m_Colliders;

	private Bounds m_ColliderBounds;

	[SerializeField]
	private Renderer[] m_Renderers;

	[SerializeField]
	private StandardAlphaAnimator m_Alpha;

	public Bounds ColliderBounds => m_ColliderBounds;

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

	private Renderer[] _renderers
	{
		get
		{
			if (m_Renderers == null || m_Renderers.Length == 0)
			{
				InitRenderers();
			}
			return m_Renderers;
		}
	}

	public Rigidbody Rigid => m_Rigidbody;

	private static bool IsDamageCollider(Collider col)
	{
		return col.isTrigger;
	}

	public void ResetModelInfo()
	{
		m_Rigidbody = GetComponent<Rigidbody>();
		m_Colliders = PEUtil.GetCmpts<Collider>(base.transform);
		InitRenderers();
	}

	public Material[] GetMaterials()
	{
		if (m_Renderers != null && m_Renderers.Length > 0)
		{
			int num = 0;
			if (num < m_Renderers.Length)
			{
				return m_Renderers[num].materials;
			}
		}
		return null;
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

	public void ActivateDeathEffect()
	{
		if (deathEffectID > 0)
		{
			Singleton<EffectBuilder>.Instance.Register(deathEffectID, null, base.transform.position, Quaternion.identity);
		}
	}

	public void ActivateDeathMode(bool isDeath)
	{
		if (deathModel != null)
		{
			deathModel.transform.position = base.transform.position;
			deathModel.transform.rotation = base.transform.rotation;
			base.gameObject.SetActive(!isDeath);
			deathModel.SetActive(isDeath);
		}
	}

	private void CalculateColliderBounds()
	{
		m_ColliderBounds = default(Bounds);
		for (int i = 0; i < colliders.Length; i++)
		{
			Collider collider = colliders[i];
			if (collider != null && !IsDamageCollider(collider))
			{
				if (m_ColliderBounds.center != Vector3.zero)
				{
					m_ColliderBounds.Encapsulate(collider.bounds);
					continue;
				}
				m_ColliderBounds.center = collider.bounds.center;
				m_ColliderBounds.size = collider.bounds.size;
			}
		}
	}

	private void Start()
	{
		m_AnimCmpt = GetComponentInParent<AnimatorCmpt>();
	}

	private void Update()
	{
		CalculateColliderBounds();
		if (deathModel != null)
		{
			deathModel.transform.position = base.transform.position;
			deathModel.transform.rotation = base.transform.rotation;
		}
	}

	private void AnimatorEvent(string para)
	{
		if (null != m_AnimCmpt)
		{
			m_AnimCmpt.AnimEvent(para);
		}
	}

	private void InitRenderers()
	{
		m_Renderers = PEUtil.GetCmpts<Renderer>(base.transform);
		for (int i = 0; i < m_Renderers.Length; i++)
		{
			if (m_Renderers[i] is SkinnedMeshRenderer)
			{
				m_Alpha = m_Renderers[i].GetComponent<StandardAlphaAnimator>();
				if (m_Alpha != null)
				{
					break;
				}
			}
		}
	}

	public void Remodel()
	{
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

	public void ActivatePhysics(bool value)
	{
		if (m_Rigidbody != null)
		{
			m_Rigidbody.isKinematic = !value;
		}
	}

	public void ActivateRenderer(bool value)
	{
		for (int i = 0; i < _renderers.Length; i++)
		{
			Renderer renderer = _renderers[i];
			if (renderer != null)
			{
				renderer.enabled = value;
			}
		}
	}

	public void ActivateColliders(bool value)
	{
		for (int i = 0; i < colliders.Length; i++)
		{
			Collider collider = colliders[i];
			if (collider != null && !collider.isTrigger)
			{
				collider.enabled = value;
			}
		}
	}
}
