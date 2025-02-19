using System.Collections.Generic;
using UnityEngine;

public class GoPool : MonoBehaviour
{
	public GameObject m_GoPrefab;

	public int MaxPoolSize = 50;

	public int InitedNum = 25;

	private List<GameObject> m_Gos = new List<GameObject>();

	private bool m_Inited;

	private Transform m_Trans;

	public void Init()
	{
		if (!m_Inited)
		{
			m_Trans = base.transform;
			for (int i = 0; i < InitedNum; i++)
			{
				GameObject gameObject = Object.Instantiate(m_GoPrefab);
				gameObject.transform.parent = m_Trans;
				gameObject.transform.localPosition = Vector3.zero;
				gameObject.transform.localRotation = Quaternion.identity;
				gameObject.transform.localScale = Vector3.zero;
				gameObject.SetActive(value: false);
				m_Gos.Add(gameObject);
			}
			m_Inited = true;
		}
	}

	public GameObject GetGo(Transform root, bool show)
	{
		if (m_Gos.Count != 0)
		{
			GameObject gameObject = m_Gos[0];
			m_Gos.RemoveAt(0);
			gameObject.transform.parent = root;
			ZeroSetTrans(gameObject.transform);
			if (show)
			{
				gameObject.gameObject.SetActive(value: true);
			}
			gameObject.transform.localScale = Vector3.one;
			return gameObject;
		}
		GameObject gameObject2 = Object.Instantiate(m_GoPrefab);
		gameObject2.transform.parent = root;
		ZeroSetTrans(gameObject2.transform);
		if (show)
		{
			gameObject2.SetActive(value: true);
		}
		return gameObject2;
	}

	public void GiveBackGo(GameObject go, bool hide)
	{
		if (go == null)
		{
			return;
		}
		if (hide)
		{
			go.SetActive(value: false);
		}
		if (m_Gos.Count > MaxPoolSize)
		{
			go.transform.parent = null;
			ZeroSetTrans(go.transform);
			Object.Destroy(go);
			return;
		}
		go.transform.parent = m_Trans;
		ZeroSetTrans(go.transform);
		if (hide)
		{
			go.SetActive(value: false);
		}
		m_Gos.Add(go);
	}

	public static void ZeroSetTrans(Transform trans)
	{
		trans.localPosition = Vector3.zero;
		trans.localRotation = Quaternion.identity;
		trans.localScale = Vector3.one;
	}

	private void Awake()
	{
		Init();
	}
}
