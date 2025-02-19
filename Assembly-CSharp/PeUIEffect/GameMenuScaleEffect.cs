using UnityEngine;

namespace PeUIEffect;

public class GameMenuScaleEffect : UIEffect
{
	[SerializeField]
	private AcEffect effectScale_x;

	[SerializeField]
	private AcEffect effectScale_y;

	[SerializeField]
	private UIMenuList menulist;

	[SerializeField]
	private GameObject mSpeularPrefab;

	[SerializeField]
	private TweenScale mTweenScale;

	[SerializeField]
	private TweenPosition mTweenPos;

	private Transform target;

	private float time;

	private Vector3 targetScale;

	private bool tweenScaleing;

	private Color _dstCol = new Color(1f, 1f, 1f, -1f);

	private void Start()
	{
		target = menulist.rootPanel.spBg.transform;
		menulist.rootPanel.spBg.pivot = UIWidget.Pivot.Bottom;
		target.localPosition = new Vector3(85f, (0f - menulist.rootPanel.spBg.transform.localScale.y) / 2f, 0f);
		Vector3 localScale = menulist.rootPanel.spBg.transform.localScale;
		menulist.rootPanel.spBg.transform.localScale = new Vector3(localScale.x, localScale.y + 35f, localScale.z);
		localScale = menulist.rootPanel.spBg.transform.localPosition;
		menulist.rootPanel.spBg.transform.localPosition = new Vector3(localScale.x, localScale.y - 35f, localScale.z);
		targetScale = target.localScale;
		foreach (UIMenuPanel panel in menulist.panels)
		{
			AddMenuSpeular(panel.transform);
		}
		mTweenScale.onFinished = TweenScaleFinishEvent;
	}

	private void Update()
	{
		if (m_Runing)
		{
			float x = ((!effectScale_x.bActive) ? 0f : effectScale_x.GetAcValue(time));
			float y = ((!effectScale_y.bActive) ? 0f : effectScale_y.GetAcValue(time));
			target.transform.localScale = targetScale + new Vector3(x, y, 1f);
			time += Time.deltaTime;
			if (time >= effectScale_x.EndTime)
			{
				End();
			}
		}
		if (tweenScaleing)
		{
			UpdateListMenuItemAlpha(menulist.transform.localScale.y);
		}
	}

	private void AddMenuSpeular(Transform parent)
	{
		GameObject gameObject = Object.Instantiate(mSpeularPrefab);
		gameObject.transform.parent = parent;
		gameObject.transform.localPosition = new Vector3(0f, 0f, -2f);
		gameObject.transform.localScale = Vector3.one;
		gameObject.SetActive(value: true);
	}

	private void UpdateListMenuItemAlpha(float alpha)
	{
		alpha *= alpha;
		if (!Mathf.Approximately(alpha, _dstCol.a))
		{
			_dstCol.a = Mathf.Clamp01(alpha);
			int count = menulist.Items.Count;
			for (int i = 0; i < count; i++)
			{
				UIMenuListItem uIMenuListItem = menulist.Items[i];
				uIMenuListItem.LbText.color = _dstCol;
				uIMenuListItem.mIcoSpr.color = _dstCol;
				uIMenuListItem.SpHaveChild.color = _dstCol;
			}
		}
	}

	private void TweenScaleFinishEvent(UITweener tween)
	{
		tweenScaleing = false;
		UpdateListMenuItemAlpha(menulist.transform.localScale.y);
	}

	public override void Play()
	{
		if (!m_Runing)
		{
			mForward = !mForward;
			mTweenScale.Play(mForward);
			mTweenPos.Play(mForward);
			tweenScaleing = true;
			time = 0f;
			base.Play();
		}
	}

	public override void Play(bool forward)
	{
		if (!m_Runing)
		{
			base.Play(forward);
		}
	}

	public override void End()
	{
		base.End();
		target.transform.localScale = targetScale;
	}
}
