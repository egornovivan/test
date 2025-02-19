using UnityEngine;

public class WeaponEffect : MonoBehaviour
{
	public LayerMask aiLayer;

	public Transform start;

	public Transform end;

	public static int[] EffectID = new int[4] { 24, 25, 26, 93 };

	private bool isPlay;

	public bool mAttackStart;

	private void Start()
	{
	}

	private void LateUpdate()
	{
		ImpactEffectByCollision();
	}

	public void ImpactEffectByCollision()
	{
		float num = 0f;
		if (mAttackStart && num > 0.2f && num < 0.8f)
		{
			if (!(start != null) || !(end != null) || isPlay)
			{
				return;
			}
			Ray ray = new Ray(start.position, end.position - start.position);
			float maxDistance = Vector3.Distance(end.position, start.position);
			if (Physics.Raycast(ray, out var hitInfo, maxDistance, aiLayer))
			{
				isPlay = true;
				int num2 = 0;
				num2 = ((!(Random.value < 0.3f)) ? EffectID[Random.Range(0, EffectID.Length - 1)] : EffectID[EffectID.Length - 1]);
				if (num2 > 0)
				{
					EffectManager.Instance.Instantiate(num2, hitInfo.point, Quaternion.identity);
				}
			}
		}
		else
		{
			isPlay = false;
			if (mAttackStart && num > 0.8f)
			{
				mAttackStart = false;
			}
		}
	}
}
