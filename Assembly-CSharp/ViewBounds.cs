using UnityEngine;

public class ViewBounds : MonoBehaviour
{
	[SerializeField]
	private LineRenderer[] mLineRenderers;

	[SerializeField]
	private MeshRenderer mBoxRenderer;

	private Material mBoxMat;

	private Material mLineMat;

	public void SetPos(Vector3 centerPos)
	{
		base.transform.position = centerPos;
	}

	public void SetSize(Vector3 size)
	{
		base.transform.localScale = size;
		float num = size.x + size.y + size.z;
		for (int i = 0; i < mLineRenderers.Length; i++)
		{
			mLineRenderers[i].SetWidth(num * 0.01f, num * 0.01f);
		}
	}

	public void SetColor(Color color)
	{
		mBoxMat.SetColor("_TintColor", color);
		mLineMat.color = color;
	}

	private void Awake()
	{
		mBoxMat = Object.Instantiate(mBoxRenderer.material);
		mBoxRenderer.material = mBoxMat;
		mLineMat = Object.Instantiate(mLineRenderers[0].material);
		for (int i = 0; i < mLineRenderers.Length; i++)
		{
			mLineRenderers[i].material = mLineMat;
		}
	}

	private void Reset()
	{
		mBoxRenderer = GetComponentInChildren<MeshRenderer>();
		mLineRenderers = GetComponentsInChildren<LineRenderer>();
	}

	private void OnDestroy()
	{
		Object.Destroy(mBoxMat);
		Object.Destroy(mLineMat);
	}
}
