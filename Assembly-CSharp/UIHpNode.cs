using Pathea;
using PETools;
using UnityEngine;

public class UIHpNode : MonoBehaviour
{
	[SerializeField]
	private UILabel mLabel;

	public Color color;

	public Vector3 worldPostion;

	public bool isHurt = true;

	[SerializeField]
	private AnimationCurve acScale;

	[SerializeField]
	private AnimationCurve acAlpha;

	[SerializeField]
	private float speed = 0.5f;

	[SerializeField]
	private float maxShowTime = 0.8f;

	private float time;

	public string text
	{
		set
		{
			mLabel.text = value;
		}
	}

	private void Start()
	{
		time = 0f;
		mLabel.enabled = true;
		mLabel.color = color;
		mLabel.enabled = false;
	}

	private void Update()
	{
		if (!CanShow())
		{
			time = maxShowTime + 1f;
		}
		if (color.a < 0.01f && time > maxShowTime)
		{
			mLabel.enabled = false;
			base.enabled = false;
			UIHpChange.Instance.RemoveNode(this);
			return;
		}
		float num = Mathf.Pow(Mathf.Clamp(Vector3.Distance(PEUtil.MainCamTransform.position, worldPostion), 12f, 1000f), 0.7f);
		worldPostion.y += Time.deltaTime * speed * num;
		color.a = acAlpha.Evaluate(time);
		Vector3 localPosition = Camera.main.WorldToScreenPoint(worldPostion);
		if (localPosition.z > 1f)
		{
			mLabel.enabled = true;
			mLabel.color = color;
			localPosition.z = -1f;
			base.transform.localPosition = localPosition;
			if (isHurt)
			{
				float num2 = 1f + acScale.Evaluate(time);
				base.transform.localScale = new Vector3(num2, num2, 1f);
			}
		}
		else
		{
			mLabel.enabled = false;
		}
		time += Time.deltaTime;
	}

	private bool CanShow()
	{
		if (GameUI.Instance == null)
		{
			return false;
		}
		if (!UIHpChange.Instance.m_ShowHPChange || GameConfig.IsInVCE || GameUI.Instance.mMainPlayer == null || PeGameMgr.gamePause)
		{
			return false;
		}
		return true;
	}
}
