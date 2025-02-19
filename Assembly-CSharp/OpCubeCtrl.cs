using UnityEngine;

public class OpCubeCtrl : MonoBehaviour
{
	private Color mTargetColor = new Color(0f, 1f, 0f, 0.25f);

	private bool mEnable;

	public bool Active
	{
		get
		{
			return base.gameObject.activeSelf;
		}
		set
		{
			if (base.gameObject.activeSelf != value)
			{
				base.gameObject.SetActive(value);
			}
		}
	}

	public bool Enable
	{
		get
		{
			return mEnable;
		}
		set
		{
			mEnable = value;
		}
	}

	private void Update()
	{
		if (mEnable)
		{
			mTargetColor = Color.Lerp(mTargetColor, new Color(0.156f, 0.553f, 0.518f, Random.Range(0.4f, 0.5f)), 5f * Time.deltaTime);
		}
		else
		{
			mTargetColor = Color.Lerp(mTargetColor, new Color(0.72f, 0f, 0f, Random.Range(0.4f, 0.5f)), 5f * Time.deltaTime);
		}
		GetComponent<Renderer>().material.SetColor("_TintColor", mTargetColor);
	}
}
