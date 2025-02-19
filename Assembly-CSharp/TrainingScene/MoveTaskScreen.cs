using System.Collections;
using UnityEngine;

namespace TrainingScene;

public class MoveTaskScreen : MonoBehaviour
{
	[SerializeField]
	private float minWidth = 40f;

	[SerializeField]
	private float minHeight = 20f;

	[SerializeField]
	private float maxWidth;

	[SerializeField]
	private float maxHeight;

	[SerializeField]
	private float fadeOnTime1 = 0.2f;

	[SerializeField]
	private float fadeOnTime2 = 0.2f;

	private float ctime;

	private RectTransform rt;

	private Vector2 wh = Vector2.zero;

	private void Awake()
	{
		rt = GetComponent<RectTransform>();
	}

	public void FadeOnScreen()
	{
		rt.sizeDelta = new Vector2(minWidth, minHeight);
		ctime = 0f;
		StartCoroutine(FadeOnBorder(fadeOnTime1 + fadeOnTime2, maxWidth - minWidth, maxHeight - minHeight));
	}

	private IEnumerator FadeOnBorder(float sumTime, float dx, float dy)
	{
		while (true)
		{
			yield return new WaitForFixedUpdate();
			ctime += Time.fixedDeltaTime;
			if (ctime < fadeOnTime1)
			{
				wh.y = minHeight;
				wh.x = minWidth + dx * ctime / fadeOnTime1;
				rt.sizeDelta = wh;
				continue;
			}
			if (!(ctime < sumTime))
			{
				break;
			}
			wh.y = minHeight + dy * (ctime - fadeOnTime1) / fadeOnTime2;
			wh.x = maxWidth;
			rt.sizeDelta = wh;
		}
		wh.y = maxHeight;
		wh.x = maxWidth;
		rt.sizeDelta = wh;
		Invoke("FadeOutBorder", 4f);
	}

	private void FadeOutBorder()
	{
		base.gameObject.SetActive(value: false);
	}
}
