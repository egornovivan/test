using System;
using UnityEngine;

[AddComponentMenu("NGUI/UI/Sprite (Filled)")]
[ExecuteInEditMode]
public class UIFilledSprite : UISprite
{
	public enum FillDirection
	{
		Horizontal,
		Vertical,
		Radial90,
		Radial180,
		Radial360
	}

	[HideInInspector]
	[SerializeField]
	private FillDirection mFillDirection = FillDirection.Radial360;

	[HideInInspector]
	[SerializeField]
	private float mFillAmount = 1f;

	[HideInInspector]
	[SerializeField]
	private bool mInvert;

	public FillDirection fillDirection
	{
		get
		{
			return mFillDirection;
		}
		set
		{
			if (mFillDirection != value)
			{
				mFillDirection = value;
				mChanged = true;
			}
		}
	}

	public float fillAmount
	{
		get
		{
			return mFillAmount;
		}
		set
		{
			float num = Mathf.Clamp01(value);
			if (mFillAmount != num)
			{
				mFillAmount = num;
				mChanged = true;
			}
		}
	}

	public bool invert
	{
		get
		{
			return mInvert;
		}
		set
		{
			if (mInvert != value)
			{
				mInvert = value;
				mChanged = true;
			}
		}
	}

	private bool AdjustRadial(Vector2[] xy, Vector2[] uv, float fill, bool invert)
	{
		if (fill < 0.001f)
		{
			return false;
		}
		if (!invert && fill > 0.999f)
		{
			return true;
		}
		float num = Mathf.Clamp01(fill);
		if (!invert)
		{
			num = 1f - num;
		}
		num *= (float)Math.PI / 2f;
		float num2 = Mathf.Sin(num);
		float num3 = Mathf.Cos(num);
		if (num2 > num3)
		{
			num3 *= 1f / num2;
			num2 = 1f;
			if (!invert)
			{
				xy[0].y = Mathf.Lerp(xy[2].y, xy[0].y, num3);
				xy[3].y = xy[0].y;
				uv[0].y = Mathf.Lerp(uv[2].y, uv[0].y, num3);
				uv[3].y = uv[0].y;
			}
		}
		else if (num3 > num2)
		{
			num2 *= 1f / num3;
			num3 = 1f;
			if (invert)
			{
				xy[0].x = Mathf.Lerp(xy[2].x, xy[0].x, num2);
				xy[1].x = xy[0].x;
				uv[0].x = Mathf.Lerp(uv[2].x, uv[0].x, num2);
				uv[1].x = uv[0].x;
			}
		}
		else
		{
			num2 = 1f;
			num3 = 1f;
		}
		if (invert)
		{
			xy[1].y = Mathf.Lerp(xy[2].y, xy[0].y, num3);
			uv[1].y = Mathf.Lerp(uv[2].y, uv[0].y, num3);
		}
		else
		{
			xy[3].x = Mathf.Lerp(xy[2].x, xy[0].x, num2);
			uv[3].x = Mathf.Lerp(uv[2].x, uv[0].x, num2);
		}
		return true;
	}

	private void Rotate(Vector2[] v, int offset)
	{
		for (int i = 0; i < offset; i++)
		{
			Vector2 vector = new Vector2(v[3].x, v[3].y);
			v[3].x = v[2].y;
			v[3].y = v[2].x;
			v[2].x = v[1].y;
			v[2].y = v[1].x;
			v[1].x = v[0].y;
			v[1].y = v[0].x;
			v[0].x = vector.y;
			v[0].y = vector.x;
		}
	}

	public override void OnFill(BetterList<Vector3> verts, BetterList<Vector2> uvs, BetterList<Color32> cols)
	{
		float x = 0f;
		float y = 0f;
		float num = 1f;
		float num2 = -1f;
		float num3 = mOuterUV.xMin;
		float num4 = mOuterUV.yMin;
		float num5 = mOuterUV.xMax;
		float num6 = mOuterUV.yMax;
		if (mFillDirection == FillDirection.Horizontal || mFillDirection == FillDirection.Vertical)
		{
			float num7 = (num5 - num3) * mFillAmount;
			float num8 = (num6 - num4) * mFillAmount;
			if (fillDirection == FillDirection.Horizontal)
			{
				if (mInvert)
				{
					x = 1f - mFillAmount;
					num3 = num5 - num7;
				}
				else
				{
					num *= mFillAmount;
					num5 = num3 + num7;
				}
			}
			else if (fillDirection == FillDirection.Vertical)
			{
				if (mInvert)
				{
					num2 *= mFillAmount;
					num4 = num6 - num8;
				}
				else
				{
					y = 0f - (1f - mFillAmount);
					num6 = num4 + num8;
				}
			}
		}
		Vector2[] array = UIWidget.s_4v20;
		Vector2[] array2 = UIWidget.s_4v21;
		ref Vector2 reference = ref array[0];
		reference = new Vector2(num, y);
		ref Vector2 reference2 = ref array[1];
		reference2 = new Vector2(num, num2);
		ref Vector2 reference3 = ref array[2];
		reference3 = new Vector2(x, num2);
		ref Vector2 reference4 = ref array[3];
		reference4 = new Vector2(x, y);
		ref Vector2 reference5 = ref array2[0];
		reference5 = new Vector2(num5, num6);
		ref Vector2 reference6 = ref array2[1];
		reference6 = new Vector2(num5, num4);
		ref Vector2 reference7 = ref array2[2];
		reference7 = new Vector2(num3, num4);
		ref Vector2 reference8 = ref array2[3];
		reference8 = new Vector2(num3, num6);
		Color color = base.color;
		color.a = base.finalAlpha;
		Color32 item = color;
		if (fillDirection == FillDirection.Radial90)
		{
			if (!AdjustRadial(array, array2, mFillAmount, mInvert))
			{
				return;
			}
		}
		else
		{
			if (fillDirection == FillDirection.Radial180)
			{
				Vector2[] array3 = UIWidget.s_4v22;
				Vector2[] array4 = UIWidget.s_4v23;
				for (int i = 0; i < 2; i++)
				{
					ref Vector2 reference9 = ref array3[0];
					reference9 = new Vector2(0f, 0f);
					ref Vector2 reference10 = ref array3[1];
					reference10 = new Vector2(0f, 1f);
					ref Vector2 reference11 = ref array3[2];
					reference11 = new Vector2(1f, 1f);
					ref Vector2 reference12 = ref array3[3];
					reference12 = new Vector2(1f, 0f);
					ref Vector2 reference13 = ref array4[0];
					reference13 = new Vector2(0f, 0f);
					ref Vector2 reference14 = ref array4[1];
					reference14 = new Vector2(0f, 1f);
					ref Vector2 reference15 = ref array4[2];
					reference15 = new Vector2(1f, 1f);
					ref Vector2 reference16 = ref array4[3];
					reference16 = new Vector2(1f, 0f);
					if (mInvert)
					{
						if (i > 0)
						{
							Rotate(array3, i);
							Rotate(array4, i);
						}
					}
					else if (i < 1)
					{
						Rotate(array3, 1 - i);
						Rotate(array4, 1 - i);
					}
					float a;
					float b;
					if (i == 1)
					{
						a = ((!mInvert) ? 1f : 0.5f);
						b = ((!mInvert) ? 0.5f : 1f);
					}
					else
					{
						a = ((!mInvert) ? 0.5f : 1f);
						b = ((!mInvert) ? 1f : 0.5f);
					}
					array3[1].y = Mathf.Lerp(a, b, array3[1].y);
					array3[2].y = Mathf.Lerp(a, b, array3[2].y);
					array4[1].y = Mathf.Lerp(a, b, array4[1].y);
					array4[2].y = Mathf.Lerp(a, b, array4[2].y);
					float fill = mFillAmount * 2f - (float)i;
					bool flag = i % 2 == 1;
					if (!AdjustRadial(array3, array4, fill, !flag))
					{
						continue;
					}
					if (mInvert)
					{
						flag = !flag;
					}
					if (flag)
					{
						for (int j = 0; j < 4; j++)
						{
							a = Mathf.Lerp(array[0].x, array[2].x, array3[j].x);
							b = Mathf.Lerp(array[0].y, array[2].y, array3[j].y);
							float x2 = Mathf.Lerp(array2[0].x, array2[2].x, array4[j].x);
							float y2 = Mathf.Lerp(array2[0].y, array2[2].y, array4[j].y);
							verts.Add(new Vector3(a, b, 0f));
							uvs.Add(new Vector2(x2, y2));
							cols.Add(item);
						}
						continue;
					}
					for (int num9 = 3; num9 > -1; num9--)
					{
						a = Mathf.Lerp(array[0].x, array[2].x, array3[num9].x);
						b = Mathf.Lerp(array[0].y, array[2].y, array3[num9].y);
						float x3 = Mathf.Lerp(array2[0].x, array2[2].x, array4[num9].x);
						float y3 = Mathf.Lerp(array2[0].y, array2[2].y, array4[num9].y);
						verts.Add(new Vector3(a, b, 0f));
						uvs.Add(new Vector2(x3, y3));
						cols.Add(item);
					}
				}
				return;
			}
			if (fillDirection == FillDirection.Radial360)
			{
				Vector2[] array5 = UIWidget.s_4v22;
				Vector2[] array6 = UIWidget.s_4v23;
				for (int k = 0; k < 4; k++)
				{
					ref Vector2 reference17 = ref array5[0];
					reference17 = new Vector2(0f, 0f);
					ref Vector2 reference18 = ref array5[1];
					reference18 = new Vector2(0f, 1f);
					ref Vector2 reference19 = ref array5[2];
					reference19 = new Vector2(1f, 1f);
					ref Vector2 reference20 = ref array5[3];
					reference20 = new Vector2(1f, 0f);
					ref Vector2 reference21 = ref array6[0];
					reference21 = new Vector2(0f, 0f);
					ref Vector2 reference22 = ref array6[1];
					reference22 = new Vector2(0f, 1f);
					ref Vector2 reference23 = ref array6[2];
					reference23 = new Vector2(1f, 1f);
					ref Vector2 reference24 = ref array6[3];
					reference24 = new Vector2(1f, 0f);
					if (mInvert)
					{
						if (k > 0)
						{
							Rotate(array5, k);
							Rotate(array6, k);
						}
					}
					else if (k < 3)
					{
						Rotate(array5, 3 - k);
						Rotate(array6, 3 - k);
					}
					for (int l = 0; l < 4; l++)
					{
						int num10 = ((!mInvert) ? (k * 4) : ((3 - k) * 4));
						float a2 = UIWidget.c_matrix[num10];
						float b2 = UIWidget.c_matrix[num10 + 1];
						float a3 = UIWidget.c_matrix[num10 + 2];
						float b3 = UIWidget.c_matrix[num10 + 3];
						array5[l].x = Mathf.Lerp(a2, b2, array5[l].x);
						array5[l].y = Mathf.Lerp(a3, b3, array5[l].y);
						array6[l].x = Mathf.Lerp(a2, b2, array6[l].x);
						array6[l].y = Mathf.Lerp(a3, b3, array6[l].y);
					}
					float fill2 = mFillAmount * 4f - (float)k;
					bool flag2 = k % 2 == 1;
					if (!AdjustRadial(array5, array6, fill2, !flag2))
					{
						continue;
					}
					if (mInvert)
					{
						flag2 = !flag2;
					}
					if (flag2)
					{
						for (int m = 0; m < 4; m++)
						{
							float x4 = Mathf.Lerp(array[0].x, array[2].x, array5[m].x);
							float y4 = Mathf.Lerp(array[0].y, array[2].y, array5[m].y);
							float x5 = Mathf.Lerp(array2[0].x, array2[2].x, array6[m].x);
							float y5 = Mathf.Lerp(array2[0].y, array2[2].y, array6[m].y);
							verts.Add(new Vector3(x4, y4, 0f));
							uvs.Add(new Vector2(x5, y5));
							cols.Add(item);
						}
						continue;
					}
					for (int num11 = 3; num11 > -1; num11--)
					{
						float x6 = Mathf.Lerp(array[0].x, array[2].x, array5[num11].x);
						float y6 = Mathf.Lerp(array[0].y, array[2].y, array5[num11].y);
						float x7 = Mathf.Lerp(array2[0].x, array2[2].x, array6[num11].x);
						float y7 = Mathf.Lerp(array2[0].y, array2[2].y, array6[num11].y);
						verts.Add(new Vector3(x6, y6, 0f));
						uvs.Add(new Vector2(x7, y7));
						cols.Add(item);
					}
				}
				return;
			}
		}
		for (int n = 0; n < 4; n++)
		{
			verts.Add(array[n]);
			uvs.Add(array2[n]);
			cols.Add(item);
		}
	}
}
