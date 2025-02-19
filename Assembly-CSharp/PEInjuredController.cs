using System.Collections.Generic;
using PETools;
using UnityEngine;

public class PEInjuredController : MonoBehaviour
{
	private struct InjuresPair
	{
		public Transform key;

		public Transform value;
	}

	public GameObject master;

	private ulong m_Frames;

	private ulong m_Frame;

	private bool m_Injured;

	private List<InjuresPair> m_InjuredPairs;

	private List<Collider> m_Coliders;

	public void SetInjuredActive(bool value)
	{
		if (m_Injured == value)
		{
			return;
		}
		foreach (Collider colider in m_Coliders)
		{
			colider.enabled = value;
		}
		m_Injured = value;
	}

	private void Start()
	{
		m_Frame = 3uL;
		m_Injured = true;
		m_InjuredPairs = new List<InjuresPair>();
		m_Coliders = new List<Collider>(GetComponentsInChildren<Collider>());
		if (master != null)
		{
			foreach (Collider colider in m_Coliders)
			{
				Transform child = PEUtil.GetChild(master.transform, colider.name);
				if (child != null)
				{
					InjuresPair item = default(InjuresPair);
					item.key = colider.transform;
					item.value = child;
					m_InjuredPairs.Add(item);
				}
				if (colider.GetComponent<Rigidbody>() == null)
				{
					colider.gameObject.AddComponent<Rigidbody>().isKinematic = true;
				}
				colider.isTrigger = true;
			}
		}
		SetInjuredActive(value: false);
	}

	private void Update()
	{
		if (++m_Frames % m_Frame != 0L)
		{
			return;
		}
		m_Frame = (ulong)Random.Range(3, 6);
		foreach (InjuresPair injuredPair in m_InjuredPairs)
		{
			if (injuredPair.key != null && injuredPair.value != null && ((injuredPair.key.position - injuredPair.value.position).sqrMagnitude > 0.0225f || Quaternion.Angle(injuredPair.key.rotation, injuredPair.value.rotation) > 5f))
			{
				injuredPair.key.position = injuredPair.value.position;
				injuredPair.key.rotation = injuredPair.value.rotation;
			}
		}
	}
}
