using UnityEngine;

public class IKSimple : IKSolver
{
	public int maxIterations = 100;

	public override void Solve(Transform[] bones, Vector3 target)
	{
		Transform transform = bones[bones.Length - 1];
		Vector3[] array = new Vector3[bones.Length - 2];
		float[] array2 = new float[bones.Length - 2];
		Quaternion[] array3 = new Quaternion[bones.Length - 2];
		for (int i = 0; i < bones.Length - 2; i++)
		{
			ref Vector3 reference = ref array[i];
			reference = Vector3.Cross(bones[i + 1].position - bones[i].position, bones[i + 2].position - bones[i + 1].position);
			ref Vector3 reference2 = ref array[i];
			reference2 = Quaternion.Inverse(bones[i].rotation) * array[i];
			ref Vector3 reference3 = ref array[i];
			reference3 = array[i].normalized;
			array2[i] = Vector3.Angle(bones[i + 1].position - bones[i].position, bones[i + 1].position - bones[i + 2].position);
			ref Quaternion reference4 = ref array3[i];
			reference4 = bones[i + 1].localRotation;
		}
		float[] array4 = new float[bones.Length - 1];
		float num = 0f;
		for (int j = 0; j < bones.Length - 1; j++)
		{
			array4[j] = (bones[j + 1].position - bones[j].position).magnitude;
			num += array4[j];
		}
		positionAccuracy = num * 0.001f;
		float magnitude = (transform.position - bones[0].position).magnitude;
		float magnitude2 = (target - bones[0].position).magnitude;
		bool flag = false;
		bool flag2 = false;
		float num2;
		float num3;
		if (magnitude2 > magnitude)
		{
			flag = true;
			num2 = 1f;
			num3 = 0f;
		}
		else
		{
			flag2 = true;
			num2 = 1f;
			num3 = 0f;
		}
		int num4 = 0;
		while (Mathf.Abs(magnitude - magnitude2) > positionAccuracy && num4 < maxIterations)
		{
			num4++;
			float num5 = (flag ? ((num3 + num2) / 2f) : num2);
			for (int k = 0; k < bones.Length - 2; k++)
			{
				float num6 = (flag2 ? (array2[k] * (1f - num5) + (array2[k] - 30f) * num5) : Mathf.Lerp(180f, array2[k], num5));
				float angle = array2[k] - num6;
				Quaternion quaternion = Quaternion.AngleAxis(angle, array[k]);
				Quaternion localRotation = quaternion * array3[k];
				bones[k + 1].localRotation = localRotation;
			}
			magnitude = (transform.position - bones[0].position).magnitude;
			if (magnitude2 > magnitude)
			{
				flag = true;
			}
			if (flag)
			{
				if (magnitude2 > magnitude)
				{
					num2 = num5;
				}
				else
				{
					num3 = num5;
				}
				if (num2 < 0.01f)
				{
					break;
				}
			}
			else
			{
				num3 = num2;
				num2 += 1f;
			}
		}
		bones[0].rotation = Quaternion.AngleAxis(Vector3.Angle(transform.position - bones[0].position, target - bones[0].position), Vector3.Cross(transform.position - bones[0].position, target - bones[0].position)) * bones[0].rotation;
	}
}
