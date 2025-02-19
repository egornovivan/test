using System;
using UnityEngine;

namespace EVP;

public class TextureCanvas
{
	public enum LineType
	{
		Solid,
		Dotted,
		Dashed
	}

	private Texture2D m_texture;

	private int m_pixelsWd;

	private int m_pixelsHt;

	private bool m_dirty;

	private Color32[] m_pixels;

	private Color32[] m_buffer;

	private Rect m_canvasRect;

	private float m_scaleX;

	private float m_scaleY;

	private Rect m_clipArea;

	private int m_pixelsXMin;

	private int m_pixelsXMax;

	private int m_pixelsYMin;

	private int m_pixelsYMax;

	private Color32 m_color = Color.white;

	private float m_alpha = -1f;

	private float m_srcAlpha = 1f;

	private float m_dstAlpha;

	private float m_moveX;

	private float m_moveY;

	private int m_step;

	public Rect rect
	{
		get
		{
			return m_canvasRect;
		}
		set
		{
			m_canvasRect = value;
			m_scaleX = (float)m_pixelsWd / m_canvasRect.width;
			m_scaleY = (float)m_pixelsHt / m_canvasRect.height;
			clipArea = m_canvasRect;
		}
	}

	public Rect clipArea
	{
		get
		{
			return m_clipArea;
		}
		set
		{
			m_clipArea.xMin = Mathf.Max(value.xMin, m_canvasRect.xMin);
			m_clipArea.xMax = Mathf.Min(value.xMax, m_canvasRect.xMax);
			m_clipArea.yMin = Mathf.Max(value.yMin, m_canvasRect.yMin);
			m_clipArea.yMax = Mathf.Min(value.yMax, m_canvasRect.yMax);
			m_pixelsXMin = GetPixelX(m_clipArea.xMin);
			m_pixelsXMax = GetPixelX(m_clipArea.xMax);
			m_pixelsYMin = GetPixelY(m_clipArea.yMin);
			m_pixelsYMax = GetPixelY(m_clipArea.yMax);
		}
	}

	public Color32 color
	{
		get
		{
			return m_color;
		}
		set
		{
			m_color = value;
			SetupAlpha();
		}
	}

	public float alpha
	{
		get
		{
			return m_alpha;
		}
		set
		{
			m_alpha = value;
			SetupAlpha();
		}
	}

	public bool alphaBlend { get; set; }

	public LineType lineType { get; set; }

	public int dotInterval { get; set; }

	public int dashInterval { get; set; }

	public int functionResolution { get; set; }

	public Texture2D texture
	{
		get
		{
			ApplyChanges();
			return m_texture;
		}
	}

	public TextureCanvas(int pixelsWd, int pixelsHt, Rect canvasRect)
	{
		SetupCanvas(pixelsWd, pixelsHt);
		rect = canvasRect;
	}

	public TextureCanvas(int pixelsWd, int pixelsHt, float canvasWd, float canvasHt)
	{
		SetupCanvas(pixelsWd, pixelsHt);
		rect = new Rect(0f, 0f, canvasWd, canvasHt);
	}

	public TextureCanvas(int pixelsWd, int pixelsHt)
	{
		SetupCanvas(pixelsWd, pixelsHt);
		rect = new Rect(0f, 0f, 1f, 1f);
	}

	public void DestroyTexture()
	{
		UnityEngine.Object.DestroyImmediate(m_texture);
	}

	public float Pixels2CanvasX(int pixels)
	{
		return (float)pixels / m_scaleX;
	}

	public float Pixels2CanvasY(int pixels)
	{
		return (float)pixels / m_scaleY;
	}

	private int GetPixelX(float x)
	{
		return Mathf.RoundToInt((x - m_canvasRect.xMin) * m_scaleX);
	}

	private int GetPixelY(float y)
	{
		return Mathf.RoundToInt((y - m_canvasRect.yMin) * m_scaleY);
	}

	private int GetPixelWidth(float width)
	{
		return Mathf.RoundToInt(width * m_scaleX);
	}

	private int GetPixelHeight(float height)
	{
		return Mathf.RoundToInt(height * m_scaleY);
	}

	public void MoveTo(float x0, float y0)
	{
		m_moveX = x0;
		m_moveY = y0;
		m_step = 0;
	}

	public void LineTo(float x1, float y1)
	{
		float num = m_moveX;
		float num2 = m_moveY;
		m_moveX = x1;
		m_moveY = y1;
		if (num > x1)
		{
			float num3 = num;
			num = x1;
			x1 = num3;
			num3 = num2;
			num2 = y1;
			y1 = num3;
		}
		float num4 = (y1 - num2) / (x1 - num);
		if (num < m_clipArea.xMin)
		{
			num2 += (m_clipArea.xMin - num) * num4;
			num = m_clipArea.xMin;
		}
		if (x1 > m_clipArea.xMax)
		{
			y1 += (m_clipArea.xMax - x1) * num4;
			x1 = m_clipArea.xMax;
		}
		if (!(num > m_clipArea.xMax) && !(x1 < m_clipArea.xMin) && (!(num2 < m_clipArea.yMin) || !(y1 < m_clipArea.yMin)) && (!(num2 > m_clipArea.yMax) || !(y1 > m_clipArea.yMax)))
		{
			if (num2 < m_clipArea.yMin)
			{
				num += (m_clipArea.yMin - num2) / num4;
				num2 = m_clipArea.yMin;
			}
			if (num2 > m_clipArea.yMax)
			{
				num += (m_clipArea.yMax - num2) / num4;
				num2 = m_clipArea.yMax;
			}
			if (y1 < m_clipArea.yMin)
			{
				x1 += (m_clipArea.yMin - y1) / num4;
				y1 = m_clipArea.yMin;
			}
			if (y1 > m_clipArea.yMax)
			{
				x1 += (m_clipArea.yMax - y1) / num4;
				y1 = m_clipArea.yMax;
			}
			TexLine(GetPixelX(num), GetPixelY(num2), GetPixelX(x1), GetPixelY(y1));
			m_dirty = true;
		}
	}

	public void Line(float x0, float y0, float x1, float y1)
	{
		MoveTo(x0, y0);
		LineTo(x1, y1);
	}

	public void HorizontalLine(float y)
	{
		m_step = 0;
		TexSegmentH(m_pixelsXMin, m_pixelsXMax, GetPixelY(y));
		m_dirty = true;
	}

	public void VerticalLine(float x)
	{
		m_step = 0;
		TexSegmentV(GetPixelX(x), m_pixelsYMin, m_pixelsYMax);
		m_dirty = true;
	}

	public void Circumference(float x, float y, float radius)
	{
		m_step = 0;
		int pixelWidth = GetPixelWidth(radius);
		TexEllipse(GetPixelX(x), GetPixelY(y), pixelWidth, pixelWidth);
		m_dirty = true;
	}

	public void Circle(float x, float y, float radius)
	{
		m_step = 0;
		int pixelWidth = GetPixelWidth(radius);
		TexFillEllipse(GetPixelX(x), GetPixelY(y), pixelWidth, pixelWidth);
		m_dirty = true;
	}

	public void Ellipse(float x, float y, float rx, float ry)
	{
		m_step = 0;
		TexEllipse(GetPixelX(x), GetPixelY(y), GetPixelWidth(rx), GetPixelHeight(ry));
		m_dirty = true;
	}

	public void FillEllipse(float x, float y, float rx, float ry)
	{
		m_step = 0;
		TexFillEllipse(GetPixelX(x), GetPixelY(y), GetPixelWidth(rx), GetPixelHeight(ry));
		m_dirty = true;
	}

	public void Clear()
	{
		int i = 0;
		for (int num = m_pixels.Length; i < num; i++)
		{
			ref Color32 reference = ref m_pixels[i];
			reference = m_color;
		}
		m_dirty = true;
	}

	public void Grid(float stepX, float stepY)
	{
		if (stepX < Pixels2CanvasX(2))
		{
			stepX = Pixels2CanvasX(2);
		}
		if (stepY < Pixels2CanvasY(2))
		{
			stepY = Pixels2CanvasY(2);
		}
		float num = (float)(int)(m_canvasRect.x / stepX) * stepX;
		float num2 = (float)(int)(m_canvasRect.y / stepY) * stepY;
		for (float num3 = num; num3 <= m_canvasRect.xMax; num3 += stepX)
		{
			VerticalLine(num3);
		}
		for (float num3 = num2; num3 <= m_canvasRect.yMax; num3 += stepY)
		{
			HorizontalLine(num3);
		}
	}

	public void Dot(float x, float y)
	{
		int pixelX = GetPixelX(x);
		int pixelY = GetPixelY(y);
		TexPixel(pixelX, pixelY - 1);
		TexPixel(pixelX - 1, pixelY);
		TexPixel(pixelX, pixelY);
		TexPixel(pixelX + 1, pixelY);
		TexPixel(pixelX, pixelY + 1);
		m_dirty = true;
	}

	public void Cross(float x, float y, int radiusX, int radiusY)
	{
		int pixelX = GetPixelX(x);
		int pixelY = GetPixelY(y);
		for (int i = pixelX - radiusX; i <= pixelX + radiusX; i++)
		{
			TexPixel(i, pixelY);
		}
		for (int j = pixelY - radiusY; j <= pixelY + radiusY; j++)
		{
			TexPixel(pixelX, j);
		}
		m_dirty = true;
	}

	public void FillRect(float x, float y, float width, float height)
	{
		int pixelX = GetPixelX(x);
		int num = GetPixelY(y);
		int pixelX2 = GetPixelX(x + width);
		int num2 = GetPixelY(y + height);
		if (num2 < num)
		{
			int num3 = num;
			num = num2;
			num2 = num3;
		}
		for (int i = num; i <= num2; i++)
		{
			m_step = 0;
			TexSegmentH(pixelX, pixelX2, i);
		}
		m_dirty = true;
	}

	public void Function(Func<float, float> func, float x0, float x1)
	{
		float num = Pixels2CanvasX(functionResolution);
		MoveTo(x0, func(x0));
		float num2;
		for (num2 = x0; num2 <= x1; num2 += num)
		{
			LineTo(num2, func(num2));
		}
		if (num2 < x1)
		{
			LineTo(x1, func(x1));
		}
	}

	public void Function(Func<float, float> func)
	{
		Function(func, m_canvasRect.xMin, m_canvasRect.xMax);
	}

	public void SolidFunction(Func<float, float> func, float x0, float x1)
	{
		int pixelX = GetPixelX(x0);
		int pixelX2 = GetPixelX(x1);
		int num = pixelX2 - pixelX;
		int pixelY = GetPixelY(0f);
		for (int i = 0; i <= num; i++)
		{
			m_step = 0;
			int pixelHeight = GetPixelHeight(func(x0 + Pixels2CanvasX(i)));
			TexSegmentV(pixelX + i, pixelY, pixelY + pixelHeight);
		}
		m_dirty = true;
	}

	public void SolidFunction(Func<float, float> func)
	{
		SolidFunction(func, m_canvasRect.xMin, m_canvasRect.xMax);
	}

	public void Save()
	{
		if (m_buffer == null)
		{
			m_buffer = m_pixels.Clone() as Color32[];
		}
		else
		{
			m_pixels.CopyTo(m_buffer, 0);
		}
	}

	public void Restore()
	{
		if (m_buffer != null)
		{
			m_buffer.CopyTo(m_pixels, 0);
			m_dirty = true;
		}
	}

	public void GUIDraw(int x, int y)
	{
		ApplyChanges();
		GUI.DrawTexture(new Rect(x, y, m_pixelsWd, m_pixelsHt), m_texture);
	}

	public void GUIStretchDraw(int x, int y, int width, int height)
	{
		ApplyChanges();
		GUI.DrawTexture(new Rect(x, y, width, height), m_texture);
	}

	public void GUIStretchDraw(int x, int y, int width)
	{
		ApplyChanges();
		float num = (float)m_pixelsHt / (float)m_pixelsWd;
		GUI.DrawTexture(new Rect(x, y, width, (float)width * num), m_texture);
	}

	private void ApplyChanges()
	{
		if (m_dirty)
		{
			m_texture.SetPixels32(m_pixels);
			m_texture.Apply(updateMipmaps: false);
			m_dirty = false;
		}
	}

	private void SetupCanvas(int pixelsWd, int pixelsHt)
	{
		m_texture = new Texture2D(pixelsWd, pixelsHt, TextureFormat.ARGB32, mipmap: false, linear: true);
		m_texture.hideFlags = HideFlags.HideAndDontSave;
		m_pixelsWd = pixelsWd;
		m_pixelsHt = pixelsHt;
		m_pixels = new Color32[pixelsWd * pixelsHt];
		alphaBlend = false;
		dotInterval = 5;
		dashInterval = 5;
		functionResolution = 3;
	}

	private void SetupAlpha()
	{
		if (m_alpha >= 0f)
		{
			m_color.a = (byte)(Mathf.Clamp01(m_alpha) * 255f);
		}
		m_srcAlpha = (float)(int)m_color.a / 255f;
		m_dstAlpha = 1f - m_srcAlpha;
	}

	private Color32 GetAlphaBlendedPixel(Color32 dst)
	{
		Color32 result = default(Color32);
		result.r = (byte)((float)(int)m_color.r * m_srcAlpha + (float)(int)dst.r * m_dstAlpha);
		result.g = (byte)((float)(int)m_color.g * m_srcAlpha + (float)(int)dst.g * m_dstAlpha);
		result.b = (byte)((float)(int)m_color.b * m_srcAlpha + (float)(int)dst.b * m_dstAlpha);
		result.a = (byte)((float)(int)m_color.a * m_srcAlpha + (float)(int)dst.a * m_dstAlpha);
		return result;
	}

	private bool CheckForPixel()
	{
		if (lineType == LineType.Solid)
		{
			return true;
		}
		if (lineType == LineType.Dotted)
		{
			return m_step++ % dotInterval == 0;
		}
		if (lineType == LineType.Dashed)
		{
			int num = dashInterval;
			return m_step++ % (num * 2) < num;
		}
		return true;
	}

	private void TexPixel(int x, int y)
	{
		if (x >= m_pixelsXMin && x < m_pixelsXMax && y >= m_pixelsYMin && y < m_pixelsYMax)
		{
			int num = y * m_pixelsWd + x;
			ref Color32 reference = ref m_pixels[num];
			reference = ((!alphaBlend) ? m_color : GetAlphaBlendedPixel(m_pixels[num]));
		}
	}

	private void TexLine(int x0, int y0, int x1, int y1)
	{
		int num = y1 - y0;
		int num2 = x1 - x0;
		if (num2 == 0)
		{
			TexSegmentV(x0, y0, y1);
			return;
		}
		if (num == 0)
		{
			TexSegmentH(x0, x1, y0);
			return;
		}
		int num3;
		if (num < 0)
		{
			num = -num;
			num3 = -1;
		}
		else
		{
			num3 = 1;
		}
		int num4;
		if (num2 < 0)
		{
			num2 = -num2;
			num4 = -1;
		}
		else
		{
			num4 = 1;
		}
		num <<= 1;
		num2 <<= 1;
		if (CheckForPixel())
		{
			TexPixel(x0, y0);
		}
		if (num2 > num)
		{
			int num5 = num - (num2 >> 1);
			while (x0 != x1)
			{
				if (num5 >= 0)
				{
					y0 += num3;
					num5 -= num2;
				}
				x0 += num4;
				num5 += num;
				if (CheckForPixel())
				{
					TexPixel(x0, y0);
				}
			}
			return;
		}
		int num6 = num2 - (num >> 1);
		while (y0 != y1)
		{
			if (num6 >= 0)
			{
				x0 += num4;
				num6 -= num;
			}
			y0 += num3;
			num6 += num2;
			if (CheckForPixel())
			{
				TexPixel(x0, y0);
			}
		}
	}

	private void TexSegmentV(int x, int y0, int y1)
	{
		if (y0 > y1)
		{
			int num = y0;
			y0 = y1;
			y1 = num;
		}
		if (x < m_pixelsXMin || x >= m_pixelsXMax || y1 < m_pixelsYMin || y0 >= m_pixelsYMax)
		{
			return;
		}
		if (y0 < m_pixelsYMin)
		{
			y0 = m_pixelsYMin;
		}
		if (y1 >= m_pixelsYMax)
		{
			y1 = m_pixelsYMax;
		}
		int num2 = y0 * m_pixelsWd + x;
		if (!alphaBlend)
		{
			if (lineType == LineType.Solid)
			{
				for (int i = y0; i < y1; i++)
				{
					ref Color32 reference = ref m_pixels[num2];
					reference = m_color;
					num2 += m_pixelsWd;
				}
				return;
			}
			for (int j = y0; j < y1; j++)
			{
				if (CheckForPixel())
				{
					ref Color32 reference2 = ref m_pixels[num2];
					reference2 = m_color;
				}
				num2 += m_pixelsWd;
			}
			return;
		}
		if (lineType == LineType.Solid)
		{
			for (int k = y0; k < y1; k++)
			{
				ref Color32 reference3 = ref m_pixels[num2];
				reference3 = GetAlphaBlendedPixel(m_pixels[num2]);
				num2 += m_pixelsWd;
			}
			return;
		}
		for (int l = y0; l < y1; l++)
		{
			if (CheckForPixel())
			{
				ref Color32 reference4 = ref m_pixels[num2];
				reference4 = GetAlphaBlendedPixel(m_pixels[num2]);
			}
			num2 += m_pixelsWd;
		}
	}

	private void TexSegmentH(int x0, int x1, int y)
	{
		if (x0 > x1)
		{
			int num = x0;
			x0 = x1;
			x1 = num;
		}
		if (y < m_pixelsYMin || y >= m_pixelsYMax || x1 < m_pixelsXMin || x0 >= m_pixelsXMax)
		{
			return;
		}
		if (x0 < m_pixelsXMin)
		{
			x0 = m_pixelsXMin;
		}
		if (x1 > m_pixelsXMax)
		{
			x1 = m_pixelsXMax;
		}
		int num2 = y * m_pixelsWd + x0;
		if (!alphaBlend)
		{
			if (lineType == LineType.Solid)
			{
				for (int i = x0; i < x1; i++)
				{
					ref Color32 reference = ref m_pixels[num2++];
					reference = m_color;
				}
				return;
			}
			for (int j = x0; j < x1; j++)
			{
				if (CheckForPixel())
				{
					ref Color32 reference2 = ref m_pixels[num2];
					reference2 = m_color;
				}
				num2++;
			}
			return;
		}
		if (lineType == LineType.Solid)
		{
			for (int k = x0; k < x1; k++)
			{
				ref Color32 reference3 = ref m_pixels[num2];
				reference3 = GetAlphaBlendedPixel(m_pixels[num2]);
				num2++;
			}
			return;
		}
		for (int l = x0; l < x1; l++)
		{
			if (CheckForPixel())
			{
				ref Color32 reference4 = ref m_pixels[num2];
				reference4 = GetAlphaBlendedPixel(m_pixels[num2]);
			}
			num2++;
		}
	}

	private void TexEllipse(int cx, int cy, int rx, int ry)
	{
		if (rx >= ry)
		{
			int num = rx;
			int num2 = -rx;
			int num3 = (int)Mathf.Ceil((float)rx / Mathf.Sqrt(2f));
			float num4 = (float)ry / (float)rx;
			for (int i = 0; i <= num3; i++)
			{
				TexPixel(cx + i, (int)((float)cy + (float)num * num4));
				TexPixel(cx + i, (int)((float)cy - (float)num * num4));
				TexPixel(cx - i, (int)((float)cy + (float)num * num4));
				TexPixel(cx - i, (int)((float)cy - (float)num * num4));
				TexPixel(cx + num, (int)((float)cy + (float)i * num4));
				TexPixel(cx - num, (int)((float)cy + (float)i * num4));
				TexPixel(cx + num, (int)((float)cy - (float)i * num4));
				TexPixel(cx - num, (int)((float)cy - (float)i * num4));
				num2 += 2 * i + 1;
				if (num2 > 0)
				{
					num2 += 2 - 2 * num--;
				}
			}
			return;
		}
		int num5 = ry;
		int num6 = -ry;
		int num7 = (int)Mathf.Ceil((float)ry / Mathf.Sqrt(2f));
		float num8 = (float)rx / (float)ry;
		for (int j = 0; j <= num7; j++)
		{
			TexPixel((int)((float)cx + (float)j * num8), cy + num5);
			TexPixel((int)((float)cx + (float)j * num8), cy - num5);
			TexPixel((int)((float)cx - (float)j * num8), cy + num5);
			TexPixel((int)((float)cx - (float)j * num8), cy - num5);
			TexPixel((int)((float)cx + (float)num5 * num8), cy + j);
			TexPixel((int)((float)cx - (float)num5 * num8), cy + j);
			TexPixel((int)((float)cx + (float)num5 * num8), cy - j);
			TexPixel((int)((float)cx - (float)num5 * num8), cy - j);
			num6 += 2 * j + 1;
			if (num6 > 0)
			{
				num6 += 2 - 2 * num5--;
			}
		}
	}

	private void TexFillEllipse(int cx, int cy, int rx, int ry)
	{
		if (rx >= ry)
		{
			int num = rx;
			int num2 = -rx;
			int num3 = (int)Mathf.Ceil((float)rx / Mathf.Sqrt(2f));
			float num4 = (float)ry / (float)rx;
			for (int i = 0; i <= num3; i++)
			{
				TexSegmentV(cx + i, cy, (int)((float)cy + (float)num * num4));
				TexSegmentV(cx + i, cy, (int)((float)cy - (float)num * num4));
				TexSegmentV(cx - i, cy, (int)((float)cy + (float)num * num4));
				TexSegmentV(cx - i, cy, (int)((float)cy - (float)num * num4));
				TexSegmentV(cx + num, cy, (int)((float)cy + (float)i * num4));
				TexSegmentV(cx - num, cy, (int)((float)cy + (float)i * num4));
				TexSegmentV(cx + num, cy, (int)((float)cy - (float)i * num4));
				TexSegmentV(cx - num, cy, (int)((float)cy - (float)i * num4));
				num2 += 2 * i + 1;
				if (num2 > 0)
				{
					num2 += 2 - 2 * num--;
				}
			}
			return;
		}
		int num5 = ry;
		int num6 = -ry;
		int num7 = (int)Mathf.Ceil((float)ry / Mathf.Sqrt(2f));
		float num8 = (float)rx / (float)ry;
		for (int j = 0; j <= num7; j++)
		{
			TexSegmentH((int)((float)cx + (float)j * num8), cx, cy + num5);
			TexSegmentH((int)((float)cx + (float)j * num8), cx, cy - num5);
			TexSegmentH((int)((float)cx - (float)j * num8), cx, cy + num5);
			TexSegmentH((int)((float)cx - (float)j * num8), cx, cy - num5);
			TexSegmentH((int)((float)cx + (float)num5 * num8), cx, cy + j);
			TexSegmentH((int)((float)cx - (float)num5 * num8), cx, cy + j);
			TexSegmentH((int)((float)cx + (float)num5 * num8), cx, cy - j);
			TexSegmentH((int)((float)cx - (float)num5 * num8), cx, cy - j);
			num6 += 2 * j + 1;
			if (num6 > 0)
			{
				num6 += 2 - 2 * num5--;
			}
		}
	}
}
