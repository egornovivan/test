using Pathea;
using UnityEngine;

public class EffectLateupdateHelper : MonoBehaviour
{
	public Vector3 local;

	protected Transform m_ParentTrans;

	protected Vector3 m_LateUpdatePos;

	protected Quaternion m_LateUpdateRot;

	private Vector3 m_UpdatePos;

	private PeEntity m_Entity;

	public virtual void Init(Transform parentTrans)
	{
		m_ParentTrans = parentTrans;
		m_LateUpdatePos = m_ParentTrans.position;
		m_LateUpdateRot = m_ParentTrans.rotation;
		m_UpdatePos = Vector3.zero;
		m_Entity = parentTrans.GetComponentInParent<PeEntity>();
	}

	protected virtual void Update()
	{
		if (null == m_ParentTrans)
		{
			Object.Destroy(base.gameObject);
			return;
		}
		bool flag = m_ParentTrans.gameObject.activeInHierarchy;
		if (null != m_Entity)
		{
			flag = flag && !m_Entity.IsDeath();
		}
		if (flag != base.gameObject.activeSelf)
		{
			base.gameObject.SetActive(flag);
		}
		Vector3 vector = m_ParentTrans.position - ((!(m_UpdatePos != Vector3.zero)) ? m_ParentTrans.position : m_UpdatePos);
		m_UpdatePos = m_ParentTrans.position;
		base.transform.position = m_LateUpdatePos + vector;
		base.transform.rotation = m_LateUpdateRot;
	}

	protected virtual void LateUpdate()
	{
		if (null == m_ParentTrans)
		{
			Object.Destroy(base.gameObject);
			return;
		}
		m_LateUpdatePos = m_ParentTrans.position + local;
		m_LateUpdateRot = m_ParentTrans.rotation;
	}
}
