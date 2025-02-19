using UnityEngine;

public class VCERefPlane : MonoBehaviour
{
	public static int XRef;

	public static int YRef;

	public static int ZRef;

	public GLGridPlane m_BasePlane;

	public Transform m_XRefTrans;

	public Transform m_YRefTrans;

	public Transform m_ZRefTrans;

	public static void Reset()
	{
		XRef = (YRef = (ZRef = 0));
	}

	private void Update()
	{
		if (XRef > 0)
		{
			m_XRefTrans.gameObject.SetActive(value: true);
		}
		else
		{
			m_XRefTrans.gameObject.SetActive(value: false);
		}
		if (YRef > 0)
		{
			m_YRefTrans.gameObject.SetActive(value: true);
			m_BasePlane.m_ShowGrid = false;
		}
		else
		{
			m_YRefTrans.gameObject.SetActive(value: false);
			m_BasePlane.m_ShowGrid = true;
		}
		if (ZRef > 0)
		{
			m_ZRefTrans.gameObject.SetActive(value: true);
		}
		else
		{
			m_ZRefTrans.gameObject.SetActive(value: false);
		}
		float num = ((!VCEditor.DocumentOpen()) ? 0.01f : VCEditor.s_Scene.m_Setting.m_VoxelSize);
		m_XRefTrans.localPosition = Vector3.right * ((float)XRef * num);
		m_YRefTrans.localPosition = Vector3.up * ((float)YRef * num);
		m_ZRefTrans.localPosition = Vector3.forward * ((float)ZRef * num);
	}
}
