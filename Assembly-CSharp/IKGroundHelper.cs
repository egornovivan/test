using PETools;
using UnityEngine;

public class IKGroundHelper : MonoBehaviour
{
	[SerializeField]
	private Transform m_ModeTrans;

	private void Start()
	{
		if (!(null != m_ModeTrans))
		{
			ModeTrans();
		}
	}

	private void Update()
	{
		if (null != m_ModeTrans)
		{
			base.transform.position = m_ModeTrans.position;
			base.transform.rotation = m_ModeTrans.rotation;
		}
	}

	public void ModeTrans()
	{
		PEModelController cmpt = PEUtil.GetCmpt<PEModelController>(base.transform.parent);
		if (null != cmpt)
		{
			m_ModeTrans = cmpt.transform;
		}
	}
}
