using System;
using PeCustom;
using UnityEngine;

public class UIStopwatchNode : MonoBehaviour
{
	[HideInInspector]
	public UIStopwatchList List;

	[HideInInspector]
	public int StopwatchId;

	[HideInInspector]
	public int Index;

	[SerializeField]
	private UISprite BgSprite;

	[SerializeField]
	private UISprite WarnSprite;

	[SerializeField]
	private UILabel NameLabel;

	[SerializeField]
	private UILabel TimeLabel;

	private float fade;

	private float fadedir;

	private float y;

	private StopwatchMgr mgr => PeCustomScene.Self.scenario.stopwatchMgr;

	private float YTarget => -27 - 52 * Index;

	private void OnEnable()
	{
		if (mgr.stopwatches.ContainsKey(StopwatchId))
		{
			Stopwatch stopwatch = mgr.stopwatches[StopwatchId];
			NameLabel.text = stopwatch.name;
			TimeLabel.text = TimeString(stopwatch.timer.Second);
			base.transform.localPosition = new Vector3(0f, YTarget, 10f);
			y = YTarget;
			FadeIn();
		}
		else
		{
			FadeOut();
		}
	}

	private void Update()
	{
		float num = 0f;
		if (mgr.stopwatches.ContainsKey(StopwatchId))
		{
			Stopwatch stopwatch = mgr.stopwatches[StopwatchId];
			NameLabel.text = stopwatch.name;
			TimeLabel.text = TimeString(stopwatch.timer.Second);
			if (stopwatch.timer.Second < 10.5 && stopwatch.timer.ElapseSpeed < 0f)
			{
				num = 0.5f - Mathf.Cos((float)stopwatch.timer.Second * (float)Math.PI * 4f) * 0.5f;
			}
		}
		else
		{
			FadeOut();
		}
		fade += fadedir * Time.deltaTime * 3f;
		float num2 = Mathf.Clamp01(fade);
		BgSprite.alpha = num2 * 0.6f;
		WarnSprite.alpha = num2 * num * 0.5f;
		NameLabel.alpha = num2;
		TimeLabel.alpha = num2;
		y = Mathf.Lerp(y, YTarget, 0.25f);
		base.transform.localPosition = new Vector3(0f, Mathf.Round(y), 10f);
		if (fade < 0f && fadedir <= 0f)
		{
			List.DeleteNode(StopwatchId);
		}
		if (fade > 1f)
		{
			fade = 1f;
		}
	}

	private string TimeString(double sec)
	{
		if (sec < 0.0)
		{
			sec = 0.0;
		}
		int num = Mathf.RoundToInt((float)sec);
		int num2 = num % 60;
		int num3 = num % 3600 / 60;
		int num4 = num / 3600;
		if (num4 > 0)
		{
			return num4.ToString("00") + ":" + num3.ToString("00") + ":" + num2.ToString("00");
		}
		return num3.ToString("00") + ":" + num2.ToString("00");
	}

	private void FadeIn()
	{
		fadedir = 1f;
	}

	private void FadeOut()
	{
		fadedir = -1f;
	}
}
