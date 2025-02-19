using UnityEngine;

public class VCEUITips : MonoBehaviour
{
	public float showTime = 10f;

	public float lifeTime;

	public void Show()
	{
		lifeTime = 0f;
		base.gameObject.SetActive(value: true);
	}

	public void Show(float time)
	{
		showTime = time;
		lifeTime = 0f;
		base.gameObject.SetActive(value: true);
	}

	public void Hide()
	{
		lifeTime = 0f;
		base.gameObject.SetActive(value: false);
	}

	private void Update()
	{
		lifeTime += Time.deltaTime;
		if (lifeTime > showTime)
		{
			base.gameObject.SetActive(value: false);
		}
	}
}
