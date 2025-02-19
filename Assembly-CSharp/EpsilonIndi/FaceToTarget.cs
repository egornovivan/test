using UnityEngine;

namespace EpsilonIndi;

public class FaceToTarget : MonoBehaviour
{
	[SerializeField]
	private Transform target;

	private void Update()
	{
		base.transform.forward = (target.position - base.transform.position).normalized;
	}
}
