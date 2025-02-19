using System;
using UnityEngine;

namespace CameraForge;

public static class Utils
{
	private static System.Random random = new System.Random((int)DateTime.Now.Ticks);

	public static string NewGUID()
	{
		long ticks = DateTime.Now.Ticks;
		byte[] array = new byte[10];
		random.NextBytes(array);
		ushort num = (ushort)ticks;
		ushort num2 = (ushort)(ticks >> 16);
		ushort num3 = (ushort)(ticks >> 32);
		return array[0].ToString("X").PadLeft(2, '0') + array[1].ToString("X").PadLeft(2, '0') + array[2].ToString("X").PadLeft(2, '0') + array[3].ToString("X").PadLeft(2, '0') + "-" + num3.ToString("X").PadLeft(4, '0') + "-" + num2.ToString("X").PadLeft(4, '0') + "-" + num.ToString("X").PadLeft(4, '0') + "-" + array[4].ToString("X").PadLeft(2, '0') + array[5].ToString("X").PadLeft(2, '0') + array[6].ToString("X").PadLeft(2, '0') + array[7].ToString("X").PadLeft(2, '0') + array[8].ToString("X").PadLeft(2, '0') + array[9].ToString("X").PadLeft(2, '0');
	}

	public static float NormalizeDEG(float angle)
	{
		return Mathf.Repeat(angle + 360f, 720f) - 360f;
	}

	public static float EvaluateActivitySpaceSize(Vector3 character, float min, float max, Vector3 weightDirection, float accuracy, LayerMask layerMask)
	{
		float num = 360f / Mathf.Ceil(Mathf.Clamp(accuracy, 0.5f, 5f) * 4f);
		double num2 = 0.0;
		double num3 = 0.0;
		float num4 = max;
		for (float num5 = -90f; num5 < 90.01f; num5 += num)
		{
			if (num5 < -35f)
			{
				continue;
			}
			float num6 = Mathf.Sin(num5 * 1000f) * 180f;
			float num7 = 360f / Mathf.Ceil(Mathf.Clamp(accuracy * Mathf.Cos(num5 * ((float)Math.PI / 180f)), 0.01f, 5f) * 4f);
			for (float num8 = num6; num8 < num6 + 359.99f; num8 += num7)
			{
				Vector3 vector = new Vector3(Mathf.Cos(num5 * ((float)Math.PI / 180f)) * Mathf.Cos(num8 * ((float)Math.PI / 180f)), Mathf.Sin(num5 * ((float)Math.PI / 180f)), Mathf.Cos(num5 * ((float)Math.PI / 180f)) * Mathf.Sin(num8 * ((float)Math.PI / 180f)));
				RaycastHit hitInfo;
				float num9 = ((!Physics.Raycast(new Ray(character, vector), out hitInfo, max, layerMask, QueryTriggerInteraction.Ignore)) ? max : hitInfo.distance);
				float num10 = Mathf.Pow(Mathf.Clamp(Vector3.Dot(vector, weightDirection), -1f, 1f) + 1f, 1.5f);
				float num11 = max / (num9 + max * 0.3f);
				if (num9 < num4)
				{
					num4 = num9;
				}
				num2 += (double)(num10 * num11);
				num3 += (double)(num10 * num11 * num9);
			}
		}
		float num12 = (float)(num3 / num2);
		return Mathf.Max(num4, num12 * 0.5f);
	}

	public static float EvaluateNearclipPlaneRadius(Vector3 character, float min, float max, LayerMask layerMask)
	{
		if (Physics.OverlapSphere(character, min, layerMask, QueryTriggerInteraction.Ignore).Length > 0)
		{
			return min;
		}
		if (Physics.OverlapSphere(character, max, layerMask, QueryTriggerInteraction.Ignore).Length == 0)
		{
			return max;
		}
		while (max - min > 0.005f)
		{
			float num = (min + max) * 0.5f;
			if (Physics.OverlapSphere(character, num, layerMask, QueryTriggerInteraction.Ignore).Length == 0)
			{
				min = num;
			}
			else
			{
				max = num;
			}
		}
		return min;
	}
}
