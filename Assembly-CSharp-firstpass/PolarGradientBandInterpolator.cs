using System;
using UnityEngine;

public class PolarGradientBandInterpolator : Interpolator
{
	public PolarGradientBandInterpolator(float[][] samplePoints)
		: base(samplePoints)
	{
		samples = samplePoints;
	}

	public override float[] Interpolate(float[] output, bool normalize)
	{
		float[] array = BasicChecks(output);
		if (array != null)
		{
			return array;
		}
		array = new float[samples.Length];
		Vector3[] array2 = new Vector3[samples.Length];
		Vector3 vector;
		if (output.Length == 2)
		{
			vector = new Vector3(output[0], output[1], 0f);
			for (int i = 0; i < samples.Length; i++)
			{
				ref Vector3 reference = ref array2[i];
				reference = new Vector3(samples[i][0], samples[i][1], 0f);
			}
		}
		else
		{
			if (output.Length != 3)
			{
				return null;
			}
			vector = new Vector3(output[0], output[1], output[2]);
			for (int j = 0; j < samples.Length; j++)
			{
				ref Vector3 reference2 = ref array2[j];
				reference2 = new Vector3(samples[j][0], samples[j][1], samples[j][2]);
			}
		}
		for (int k = 0; k < samples.Length; k++)
		{
			bool flag = false;
			float num = 1f;
			for (int l = 0; l < samples.Length; l++)
			{
				if (k == l)
				{
					continue;
				}
				Vector3 vector2 = array2[k];
				Vector3 vector3 = array2[l];
				float num2 = 2f;
				float num3;
				float num4;
				Vector3 vector4;
				if (vector2 == Vector3.zero)
				{
					num3 = Vector3.Angle(vector, vector3) * ((float)Math.PI / 180f);
					num4 = 0f;
					vector4 = vector;
					num2 = 1f;
				}
				else if (vector3 == Vector3.zero)
				{
					num3 = Vector3.Angle(vector, vector2) * ((float)Math.PI / 180f);
					num4 = num3;
					vector4 = vector;
					num2 = 1f;
				}
				else
				{
					num3 = Vector3.Angle(vector2, vector3) * ((float)Math.PI / 180f);
					if (num3 > 0f)
					{
						if (vector == Vector3.zero)
						{
							num4 = num3;
							vector4 = vector;
						}
						else
						{
							Vector3 vector5 = Vector3.Cross(vector2, vector3);
							vector4 = Util.ProjectOntoPlane(vector, vector5);
							num4 = Vector3.Angle(vector2, vector4) * ((float)Math.PI / 180f);
							if (num3 < (float)Math.PI * 99f / 100f && Vector3.Dot(Vector3.Cross(vector2, vector4), vector5) < 0f)
							{
								num4 *= -1f;
							}
						}
					}
					else
					{
						vector4 = vector;
						num4 = 0f;
					}
				}
				float magnitude = vector2.magnitude;
				float magnitude2 = vector3.magnitude;
				float magnitude3 = vector4.magnitude;
				float num5 = (magnitude + magnitude2) / 2f;
				magnitude /= num5;
				magnitude2 /= num5;
				magnitude3 /= num5;
				Vector3 lhs = new Vector3(num3 * num2, magnitude2 - magnitude, 0f);
				float num6 = 1f - Vector3.Dot(rhs: new Vector3(num4 * num2, magnitude3 - magnitude, 0f), lhs: lhs) / lhs.sqrMagnitude;
				if (num6 < 0f)
				{
					flag = true;
					break;
				}
				num = Mathf.Min(num, num6);
			}
			if (!flag)
			{
				array[k] = num;
			}
		}
		if (normalize)
		{
			float num7 = 0f;
			for (int m = 0; m < samples.Length; m++)
			{
				num7 += array[m];
			}
			if (num7 > 0f)
			{
				for (int n = 0; n < samples.Length; n++)
				{
					array[n] /= num7;
				}
			}
		}
		return array;
	}
}
