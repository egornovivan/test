using System.Collections;
using System.Collections.Generic;
using Pathea;
using PETools;
using UnityEngine;

public class UITameMonsterCtrl : UIStaticWnd
{
	private static UITameMonsterCtrl mInstance;

	[SerializeField]
	private GameObject _tamingRoot;

	[SerializeField]
	private GameObject _tameFailedRoot;

	[SerializeField]
	private GameObject _tameSucceedRoot;

	[SerializeField]
	private Transform _checkRangeRoot;

	[SerializeField]
	private UISprite _centerImg;

	[SerializeField]
	private UISprite _roundnessImg;

	[SerializeField]
	private UISprite _borderImg;

	[SerializeField]
	private UISlider _tameProgress;

	[SerializeField]
	private UILabel _progressLbl;

	[SerializeField]
	private TweenScale _tameSucceedTween;

	[SerializeField]
	private TweenScale _tameFailedTween;

	[SerializeField]
	private float _maxRadius = 75f;

	[SerializeField]
	private float _effectShowTime = 0.2f;

	[SerializeField]
	private TweenPosition _directionArrayTween;

	[SerializeField]
	private int _arrayTweenOffset = 8;

	[SerializeField]
	private float _arrayDuration = 0.2f;

	private Vector3 _localPos;

	private PeTrans _targetTrans;

	private float _maxCheckRadius;

	private List<GameObject> _rangeList;

	private float _safeRadius;

	private RangeAttribute _zRange;

	private RangeAttribute _xRange;

	private TameMonsterManager.TameState _curTameState;

	private float _curWiatTime;

	private TweenScale _curTween;

	private GameObject _curRoot;

	public static UITameMonsterCtrl Instance => mInstance;

	private void Update()
	{
		if ((bool)_targetTrans)
		{
			UpdateCenterImg();
			CheckShowArray(_localPos);
		}
	}

	public override void Show()
	{
		base.Show();
	}

	public override void OnCreate()
	{
		base.OnCreate();
		mInstance = this;
		Init();
	}

	protected override void OnHide()
	{
		base.OnHide();
		_targetTrans = null;
		_tamingRoot.SetActive(value: false);
		_tameFailedRoot.SetActive(value: false);
		_tameSucceedRoot.SetActive(value: false);
	}

	[ContextMenu("ReCreateCheckRange")]
	private void Init()
	{
		if (_rangeList == null)
		{
			_rangeList = new List<GameObject>();
		}
		if (_rangeList.Count > 0)
		{
			for (int i = 0; i < _rangeList.Count; i++)
			{
				Object.Destroy(_rangeList[i]);
			}
		}
		if (TameMonsterConfig.instance.IfRangeList.Count > 0)
		{
			TameMonsterConfig.DropIfRangeInfo dropIfRangeInfo = default(TameMonsterConfig.DropIfRangeInfo);
			TameMonsterConfig.DropIfRangeInfo dropIfRangeInfo2 = default(TameMonsterConfig.DropIfRangeInfo);
			int num = TameMonsterConfig.instance.IfRangeList.Count;
			float radius;
			for (int j = 0; j < TameMonsterConfig.instance.IfRangeList.Count; j++)
			{
				dropIfRangeInfo = TameMonsterConfig.instance.IfRangeList[j];
				radius = AngleRadiusConvertToRadius(dropIfRangeInfo.AngleRadius, TameMonsterConfig.instance.MaxRotate) * 2f;
				InstantiateNewRangeImg(isRoundness: true, radius, dropIfRangeInfo.Color, num);
				num--;
				if (dropIfRangeInfo.AllowStayTime == -1f)
				{
					dropIfRangeInfo2 = dropIfRangeInfo;
				}
			}
			_safeRadius = AngleRadiusConvertToRadius(dropIfRangeInfo2.AngleRadius, TameMonsterConfig.instance.MaxRotate);
			radius = AngleRadiusConvertToRadius(dropIfRangeInfo.AngleRadius, TameMonsterConfig.instance.MaxRotate) * 2f;
			InstantiateNewRangeImg(isRoundness: false, radius, dropIfRangeInfo.Color, num);
			_maxCheckRadius = radius * 0.5f;
		}
		_directionArrayTween.duration = _arrayDuration;
		_directionArrayTween.gameObject.SetActive(value: false);
		_centerImg.transform.localPosition = Vector3.zero;
	}

	private float GetRotateToPos(float rotate, RangeAttribute range, float maxRadius)
	{
		rotate = GetDirectionRotate(rotate);
		rotate = Mathf.Clamp(rotate, range.min, range.max);
		return (rotate - range.min) / (range.max - range.min) * (maxRadius * 2f) - maxRadius;
	}

	private void CheckShowArray(Vector3 direction)
	{
		if (Mathf.Abs(direction.x) > _safeRadius || Mathf.Abs(direction.y) > _safeRadius)
		{
			_directionArrayTween.gameObject.SetActive(value: true);
			_directionArrayTween.transform.parent.localPosition = -direction.normalized * (_maxCheckRadius + 7f);
			float num = PEUtil.AngleZ(new Vector3(0f, _safeRadius), -direction);
			if (0f - direction.x > 0f)
			{
				num = 360f - num;
			}
			_directionArrayTween.transform.parent.localEulerAngles = new Vector3(0f, 0f, num);
		}
		else
		{
			_directionArrayTween.gameObject.SetActive(value: false);
		}
	}

	private RangeAttribute GetRange(float initRotate, float max)
	{
		initRotate = GetDirectionRotate(initRotate);
		return new RangeAttribute(initRotate - max, initRotate + max);
	}

	private float GetDirectionRotate(float rotate)
	{
		return (!(rotate < 180f)) ? (rotate - 360f) : rotate;
	}

	private void UpdateCenterImg()
	{
		if (_curTameState == TameMonsterManager.TameState.Taming && (bool)_targetTrans && (bool)_centerImg)
		{
			_localPos.x = 0f - GetRotateToPos(_targetTrans.rotation.eulerAngles.z, _zRange, _maxCheckRadius);
			_localPos.y = GetRotateToPos(_targetTrans.rotation.eulerAngles.x, _xRange, _maxCheckRadius);
			_localPos.z = _centerImg.transform.localPosition.z;
			_centerImg.transform.localPosition = _localPos;
		}
	}

	private void InstantiateNewRangeImg(bool isRoundness, float radius, Color col, int depth)
	{
		GameObject gameObject = Object.Instantiate((!isRoundness) ? _borderImg.gameObject : _roundnessImg.gameObject);
		gameObject.transform.parent = _checkRangeRoot;
		gameObject.transform.localPosition = Vector3.zero;
		gameObject.transform.localScale = new Vector3(radius, radius);
		UISprite component = gameObject.GetComponent<UISprite>();
		if ((bool)component)
		{
			component.depth = depth;
			component.color = ((!isRoundness) ? Color.white : col);
		}
		_rangeList.Add(gameObject);
	}

	private float AngleRadiusConvertToRadius(float angle, float maxAngle)
	{
		angle = Mathf.Clamp(angle, 0f, maxAngle);
		return angle / maxAngle * _maxRadius;
	}

	private IEnumerator WaitEffectTimeIterator()
	{
		yield return new WaitForSeconds(_curWiatTime);
		if (null != _curTween)
		{
			ResetTween(_curTween);
			_curRoot.SetActive(value: false);
		}
	}

	private void ResetTween(TweenScale tween)
	{
		tween.transform.localScale = tween.from;
		tween.Reset();
	}

	private void PlayTween(TweenScale tween, GameObject root)
	{
		ResetTween(tween);
		_curTween = tween;
		_curRoot = root;
		_curWiatTime = _curTween.duration + _effectShowTime;
		_curTween.Play(forward: true);
		StartCoroutine("WaitEffectTimeIterator");
	}

	public void SetInfo(PeTrans trans)
	{
		if ((bool)trans)
		{
			_targetTrans = trans;
			_zRange = GetRange(_targetTrans.rotation.eulerAngles.z, TameMonsterConfig.instance.MaxRotate);
			_xRange = GetRange(_targetTrans.rotation.eulerAngles.x, TameMonsterConfig.instance.MaxRotate);
		}
	}

	public void UpdateTameProgress(int curProgress, int totalProgress)
	{
		if (_curTameState == TameMonsterManager.TameState.Taming)
		{
			_tameProgress.sliderValue = ((totalProgress > 0) ? ((float)curProgress * 1f / (float)totalProgress) : 0f);
			_progressLbl.text = ((totalProgress > 0) ? Mathf.FloorToInt(_tameProgress.sliderValue * 100f).ToString() : "--");
		}
	}

	public void SetTameState(TameMonsterManager.TameState tameState)
	{
		_curTameState = tameState;
		switch (tameState)
		{
		case TameMonsterManager.TameState.None:
			_tamingRoot.SetActive(value: false);
			_targetTrans = null;
			break;
		case TameMonsterManager.TameState.Taming:
			StopAllCoroutines();
			_tamingRoot.SetActive(value: true);
			_tameFailedRoot.SetActive(value: false);
			_tameSucceedRoot.SetActive(value: false);
			break;
		case TameMonsterManager.TameState.TameSucceed:
			_tamingRoot.SetActive(value: false);
			_tameFailedRoot.SetActive(value: false);
			_tameSucceedRoot.SetActive(value: true);
			PlayTween(_tameSucceedTween, _tameSucceedRoot);
			break;
		case TameMonsterManager.TameState.TameFailed:
			_tamingRoot.SetActive(value: false);
			_tameFailedRoot.SetActive(value: true);
			_tameSucceedRoot.SetActive(value: false);
			PlayTween(_tameFailedTween, _tameFailedRoot);
			_targetTrans = null;
			break;
		}
	}
}
