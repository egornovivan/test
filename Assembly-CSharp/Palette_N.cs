using UnityEngine;

public class Palette_N : MonoBehaviour
{
	public delegate void ChangeColorEvent(Color col);

	public UITexture mMask;

	public Camera mUICam;

	public UIScrollBar mAlphaScrollBar;

	private float mClolorScale_x;

	private Texture2D mColorTex;

	private Vector3 mMaskPos;

	[HideInInspector]
	public Color CurCol;

	public event ChangeColorEvent e_ChangeColor;

	private void Start()
	{
		mColorTex = GetComponent<UITexture>().mainTexture as Texture2D;
		mClolorScale_x = base.gameObject.transform.localScale.x / (float)mColorTex.width;
		mMaskPos = mMask.transform.localPosition;
		mAlphaScrollBar.onChange = OnAlphaScroll;
	}

	private void OnAlphaScroll(UIScrollBar sb)
	{
		ChangeColor();
	}

	private void OnPress(bool isDown)
	{
		if (isDown && Input.GetMouseButtonDown(0))
		{
			GetColor();
		}
	}

	private void OnDrag(Vector2 delta)
	{
		GetColor();
	}

	private void GetColor()
	{
		Ray ray = mUICam.ScreenPointToRay(Input.mousePosition);
		if (GetComponent<BoxCollider>().Raycast(ray, out var hitInfo, 100f))
		{
			Vector3 vector = base.transform.InverseTransformPoint(hitInfo.point);
			vector.x *= mColorTex.width;
			vector.y *= mColorTex.height;
			mMask.transform.localPosition = new Vector3((int)(mMaskPos.x + vector.x * mClolorScale_x), (int)(mMaskPos.y + vector.y - base.transform.localScale.y), -6f);
			Color pixel = mColorTex.GetPixel((int)vector.x, (int)vector.y);
			pixel.a = 1f;
			CurCol = pixel;
			ChangeColor();
		}
	}

	private void ChangeColor()
	{
		float num = Mathf.Clamp01(mAlphaScrollBar.scrollValue);
		Color curCol = CurCol;
		curCol.r *= num;
		curCol.g *= num;
		curCol.b *= num;
		curCol.a = 1f;
		if (this.e_ChangeColor != null)
		{
			this.e_ChangeColor(curCol);
		}
	}
}
