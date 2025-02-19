using System.Collections;
using UnityEngine;

public class TRBind : Trajectory
{
	public float delayTime;

	public Transform effect;

	private void Start()
	{
		Emit(m_Emitter);
	}

	public void Emit(Transform shooter)
	{
		if (delayTime > 0f)
		{
			StartCoroutine(LengthUp());
		}
	}

	private IEnumerator LengthUp()
	{
		CapsuleCollider coli = GetComponent<Collider>() as CapsuleCollider;
		if (!(coli != null))
		{
			yield break;
		}
		float maxLength = coli.height;
		float unit = (coli.height - coli.radius) / (delayTime / 0.1f);
		coli.height = coli.radius - unit;
		while (true)
		{
			coli.height += unit;
			coli.center = new Vector3(0f, 0f, coli.height / 2f);
			if (coli.height > maxLength)
			{
				break;
			}
			yield return new WaitForSeconds(0.1f);
		}
		coli.height = maxLength;
		coli.center = new Vector3(0f, 0f, maxLength / 2f);
	}
}
