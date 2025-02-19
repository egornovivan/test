using Pathea;
using UnityEngine;

public class TeammateAnimationCtrl : MonoBehaviour
{
	public UISprite ArrayBtnBg;

	public UIButton ArrayBtn;

	public TweenPosition TweenPos;

	public TweenScale TweenScale;

	public GameObject TeammateGo;

	public TeammateCtrl MyTeammateCtrl;

	private bool m_Tweening;

	private bool m_Open;

	private void Awake()
	{
		TeammateGo.SetActive(PeGameMgr.IsMulti);
		m_Open = false;
		m_Tweening = false;
	}

	private void Start()
	{
		UIEventListener.Get(ArrayBtn.gameObject).onClick = delegate
		{
			ArrayClickEvent();
		};
		TweenScale.onFinished = delegate
		{
			FinishedCalled();
		};
	}

	private void FinishedCalled()
	{
		m_Tweening = false;
		m_Open = !m_Open;
	}

	private void PlayTween(bool play)
	{
		TweenPos.Play(play);
		TweenScale.Play(play);
	}

	private void UpdateBtnState(bool isOpen)
	{
		ArrayBtnBg.spriteName = ((!isOpen) ? "rightbutton" : "leftbutton");
		ArrayBtnBg.MakePixelPerfect();
	}

	private void ArrayClickEvent()
	{
		if (!m_Tweening)
		{
			m_Tweening = true;
			PlayTween(m_Open);
			UpdateBtnState(m_Open);
			if (m_Open)
			{
				MyTeammateCtrl.RefreshTeammate();
			}
		}
	}
}
