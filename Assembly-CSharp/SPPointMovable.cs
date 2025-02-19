using System.Collections;
using UnityEngine;

public class SPPointMovable : SPPoint
{
	private Transform mTarget;

	private Vector3 mPos;

	public Transform target
	{
		set
		{
			mTarget = value;
		}
	}

	public Vector3 targetPos
	{
		set
		{
			mPos = value;
		}
	}

	private bool IsMove()
	{
		if (base.clone == null && !base.spawning && !base.death)
		{
			return true;
		}
		return false;
	}

	private IEnumerator Move()
	{
		while (true)
		{
			if (IsMove())
			{
				Vector3 targetPosition = ((!(mTarget != null)) ? mPos : mTarget.position);
				if (targetPosition != Vector3.zero)
				{
					Vector3 direction = targetPosition - base.position;
					if (direction.sqrMagnitude > 25f)
					{
						base.position += direction.normalized * 2f;
						AttachEventFromMesh();
						AttachCollider();
					}
				}
			}
			yield return new WaitForSeconds(0.5f);
		}
	}

	public void Start()
	{
		StartCoroutine(Move());
	}
}
