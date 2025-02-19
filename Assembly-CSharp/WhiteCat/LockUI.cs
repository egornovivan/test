using Pathea;
using UnityEngine;
using UnityEngine.UI;

namespace WhiteCat;

public class LockUI : MonoBehaviour
{
	[SerializeField]
	private Image _background;

	[SerializeField]
	private Image _progress;

	[SerializeField]
	private Image _cross;

	[SerializeField]
	private TweenInterpolator _crossShow;

	[SerializeField]
	private TweenInterpolator _crossLocking;

	[SerializeField]
	private TweenInterpolator _crossLocked;

	private RectTransform rectTrans;

	private RectTransform rectTransParent;

	private int lastShowType;

	private bool visible;

	private PESkEntity lastTarget;

	private PeTrans lastTrans;

	private CreationController lastCreationController;

	private float[] alphas = new float[3];

	private bool hiding;

	private static LockUI _instance;

	public static LockUI instance => _instance;

	private Vector3 targetPosition
	{
		get
		{
			if ((bool)lastTrans)
			{
				return lastTrans.center;
			}
			if ((bool)lastCreationController)
			{
				return lastCreationController.boundsCenterInWorld;
			}
			return lastTarget.transform.position;
		}
	}

	private void Awake()
	{
		_instance = this;
		hiding = false;
		rectTrans = base.transform as RectTransform;
		rectTransParent = base.transform.parent as RectTransform;
		lastShowType = -1;
		visible = false;
		for (int i = 0; i < base.transform.childCount; i++)
		{
			base.transform.GetChild(i).gameObject.SetActive(visible);
		}
	}

	private void LateUpdate()
	{
		int num = 0;
		CarrierController playerDriving = CarrierController.playerDriving;
		PESkEntity pESkEntity = null;
		if ((bool)playerDriving)
		{
			if ((bool)playerDriving.lockedTarget)
			{
				num = 3;
				pESkEntity = playerDriving.lockedTarget;
			}
			else if (playerDriving.timeToLock > 0f)
			{
				num = 2;
				pESkEntity = playerDriving.targetToLock;
			}
			else if ((bool)playerDriving.aimEntity)
			{
				num = 1;
				pESkEntity = playerDriving.aimEntity;
			}
		}
		if (num != lastShowType && !hiding)
		{
			lastShowType = num;
			_background.enabled = num == 1 || num == 2;
			_progress.enabled = num == 2 || num == 3;
			_cross.enabled = true;
			_crossShow.enabled = num == 0 || num == 1;
			_crossLocking.enabled = num == 2;
			_crossLocked.enabled = num == 3;
			if (_crossShow.enabled)
			{
				_crossShow.speed = ((num != 0) ? 1f : (-1f));
			}
			else if (_crossLocking.enabled)
			{
				Color color = _cross.color;
				color.a = 1f;
				_cross.color = color;
				_cross.transform.localScale = Vector3.one;
				_crossLocking.normalizedTime = 0f;
			}
			else if (_crossLocked.enabled)
			{
				Color color2 = _cross.color;
				color2.a = 1f;
				_cross.color = color2;
				_cross.transform.localEulerAngles = Vector3.zero;
			}
		}
		if (!pESkEntity)
		{
			return;
		}
		if (pESkEntity != lastTarget)
		{
			lastTarget = pESkEntity;
			lastTrans = pESkEntity.GetComponent<PeTrans>();
			lastCreationController = pESkEntity.GetComponent<CreationController>();
		}
		Camera main = Camera.main;
		Vector3 vector = targetPosition;
		if (visible != Vector3.Dot(main.transform.forward, vector - main.transform.position) > 1f)
		{
			visible = !visible;
			for (int i = 0; i < base.transform.childCount; i++)
			{
				base.transform.GetChild(i).gameObject.SetActive(visible);
			}
		}
		if (visible)
		{
			_progress.fillAmount = 1f - playerDriving.timeToLock / PEVCConfig.instance.lockTargetDuration;
			rectTrans.anchoredPosition = main.WorldToScreenPoint(vector) / rectTransParent.localScale.x;
		}
	}

	public void HideWhenUIPopup()
	{
		hiding = true;
		Color color = _background.color;
		Color color2 = _progress.color;
		Color color3 = _cross.color;
		alphas[0] = color.a;
		alphas[1] = color2.a;
		alphas[2] = color3.a;
		color.a = 0f;
		color2.a = 0f;
		color3.a = 0f;
		_background.color = color;
		_progress.color = color2;
		_cross.color = color3;
	}

	public void ShowWhenUIDisappear()
	{
		hiding = false;
		Color color = _background.color;
		Color color2 = _progress.color;
		Color color3 = _cross.color;
		color.a = alphas[0];
		color2.a = alphas[1];
		color3.a = alphas[2];
		_background.color = color;
		_progress.color = color2;
		_cross.color = color3;
	}
}
