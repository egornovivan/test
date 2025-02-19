using System.Linq;
using Pathea;
using UnityEngine;

public class TameMonsterManager : MonoBehaviour
{
	public class DropIfRange
	{
		private bool _inRange;

		public float AngleRadius { get; private set; }

		public Color32 Color { get; private set; }

		public float AllowStayTime { get; private set; }

		public float DropOutRandomPercent { get; private set; }

		public float CurAddupTime { get; set; }

		public bool InRange
		{
			get
			{
				return _inRange;
			}
			set
			{
				if (_inRange != value)
				{
					if (!value)
					{
						CurAddupTime = 0f;
					}
					_inRange = value;
				}
			}
		}

		public DropIfRange(TameMonsterConfig.DropIfRangeInfo info)
		{
			AngleRadius = info.AngleRadius;
			Color = info.Color;
			AllowStayTime = info.AllowStayTime;
			DropOutRandomPercent = Mathf.Clamp(info.DropOutRandomPercent, 0f, 1f);
		}
	}

	public enum TameState
	{
		None,
		Taming,
		TameSucceed,
		TameFailed
	}

	private PeEntity _monsterEntity;

	private PeEntity _playerEntity;

	private PeTrans _playerTrans;

	private Transform _ridePosTrans;

	private TameMonsterInputCtrl _inputCtrl;

	private AnalogMonsterStruggle _analogStruggle;

	private TameState _curTameState;

	private Vector3 _frameOffsetRotate;

	private Vector3 _offsetRotate;

	private Vector3 _initRotate;

	private DropIfRange[] _dropIfRanges;

	private float _xRotate;

	private float _zRotate;

	private DropIfRange _preRangeTemp;

	private DropIfRange _curRangeTemp;

	private void Start()
	{
	}

	private void Update()
	{
		if (_curTameState == TameState.Taming && (bool)_analogStruggle)
		{
			UITameMonsterCtrl.Instance.UpdateTameProgress(_analogStruggle.CurTameProgress, _analogStruggle.StruggleActionTotal);
			if (_analogStruggle.IsTameSucceed)
			{
				TameSucceed();
			}
		}
	}

	private void LateUpdate()
	{
		if (_curTameState != TameState.TameFailed && null != _ridePosTrans && (bool)_playerTrans)
		{
			_playerTrans.position = _ridePosTrans.position + TameMonsterConfig.instance.IkRideOffset;
		}
		if (_curTameState == TameState.Taming)
		{
			if ((bool)_inputCtrl && (bool)_analogStruggle && (bool)_playerTrans && (bool)_ridePosTrans)
			{
				if (_offsetRotate.sqrMagnitude > 0.01f)
				{
					Vector3 vector = Vector3.Project(_analogStruggle.OffsetRotate, _offsetRotate);
					Vector3 vector2 = _analogStruggle.OffsetRotate - vector;
					if (Vector3.Dot(vector, _offsetRotate) < 0f)
					{
						vector *= TameMonsterConfig.instance.PositiveFactor;
					}
					_offsetRotate += vector + vector2;
				}
				else
				{
					_offsetRotate += _analogStruggle.OffsetRotate;
				}
				_offsetRotate += _inputCtrl.OffsetRotate;
				_offsetRotate.y = 0f;
				_offsetRotate = Mathf.Clamp(_offsetRotate.magnitude, 0f, TameMonsterConfig.instance.MaxRotate) * _offsetRotate.normalized;
				_initRotate.y = _ridePosTrans.rotation.eulerAngles.y;
				_playerTrans.rotation = Quaternion.Euler(_initRotate + _offsetRotate);
				if (PeGameMgr.IsMulti && (bool)PlayerNetwork.mainPlayer)
				{
					PlayerNetwork.mainPlayer.RequestSyncRotation(_playerTrans.rotation.eulerAngles);
				}
			}
			CheckInRange();
		}
		if (_curTameState == TameState.TameSucceed && (bool)_playerTrans && (bool)_ridePosTrans)
		{
			_playerTrans.rotation = _ridePosTrans.rotation;
			if (PeGameMgr.IsMulti && (bool)PlayerNetwork.mainPlayer)
			{
				PlayerNetwork.mainPlayer.RequestSyncRotation(_playerTrans.rotation.eulerAngles);
			}
		}
	}

	private bool PointInRadius(float x, float z, float max)
	{
		x = Mathf.Abs(x);
		z = Mathf.Abs(z);
		max = Mathf.Abs(max);
		return x * x + z * z <= max * max;
	}

	private float GetDirectionRotate(float rotate)
	{
		return (!(rotate < 180f)) ? (rotate - 360f) : rotate;
	}

	private void Taming()
	{
		if (_curTameState != TameState.Taming)
		{
			_curTameState = TameState.Taming;
			if ((bool)_monsterEntity && (bool)_playerEntity)
			{
				SetMonsterAiState(isOpen: false);
				_monsterEntity.monstermountCtrl.Taming();
				UITameMonsterCtrl.Instance.SetTameState(TameState.Taming);
			}
		}
	}

	private void TameSucceed()
	{
		if (_curTameState == TameState.TameSucceed)
		{
			return;
		}
		_curTameState = TameState.TameSucceed;
		if ((bool)_monsterEntity && (bool)_playerEntity)
		{
			SetMonsterAiState(isOpen: false);
			_monsterEntity.monstermountCtrl.TameSucceed();
		}
		UITameMonsterCtrl.Instance.SetTameState(TameState.TameSucceed);
		DestoryinputCtrl();
		DestoryAnalogStruggle();
		if ((bool)_playerEntity && (bool)_monsterEntity)
		{
			if ((bool)_playerEntity.mountCmpt)
			{
				_playerEntity.mountCmpt.SetMount(_monsterEntity);
			}
		}
		else
		{
			Debug.LogFormat("RelationshipDataMgr.AddRelationship failed! player is null:{0} , monster is null:{1}", null == _playerEntity, null == _monsterEntity);
		}
	}

	public void TameFailed()
	{
		if (_curTameState != TameState.TameFailed)
		{
			_curTameState = TameState.TameFailed;
			if ((bool)_monsterEntity && (bool)_playerEntity)
			{
				SetMonsterAiState(isOpen: true);
				_monsterEntity.monstermountCtrl.TameFailure();
			}
			UITameMonsterCtrl.Instance.SetTameState(TameState.TameFailed);
			DestoryinputCtrl();
			DestoryAnalogStruggle();
		}
	}

	private void DestoryinputCtrl()
	{
		if ((bool)_inputCtrl)
		{
			_inputCtrl.SetTrans(null);
			Object.Destroy(_inputCtrl);
		}
	}

	private void DestoryAnalogStruggle()
	{
		if ((bool)_analogStruggle)
		{
			_analogStruggle.SetInfo(null, null);
			Object.Destroy(_analogStruggle);
		}
	}

	private void SetMonsterAiState(bool isOpen)
	{
		if ((bool)_monsterEntity && (bool)_monsterEntity.BehaveCmpt)
		{
			if (isOpen)
			{
				_monsterEntity.BehaveCmpt.Excute();
			}
			else
			{
				_monsterEntity.BehaveCmpt.Stop();
			}
		}
	}

	private void InitDropIsRanges()
	{
		if (TameMonsterConfig.instance.IfRangeList.Count > 0)
		{
			TameMonsterConfig.instance.IfRangeList = TameMonsterConfig.instance.IfRangeList.OrderBy((TameMonsterConfig.DropIfRangeInfo a) => a.AngleRadius).ToList();
			_dropIfRanges = new DropIfRange[TameMonsterConfig.instance.IfRangeList.Count];
			for (int i = 0; i < TameMonsterConfig.instance.IfRangeList.Count; i++)
			{
				_dropIfRanges[i] = new DropIfRange(TameMonsterConfig.instance.IfRangeList[i]);
			}
		}
	}

	private void CheckInRange()
	{
		if (_dropIfRanges == null || _dropIfRanges.Length <= 0)
		{
			return;
		}
		_xRotate = Mathf.Abs(_offsetRotate.x);
		_zRotate = Mathf.Abs(_offsetRotate.z);
		for (int i = 0; i < _dropIfRanges.Length; i++)
		{
			if (i >= 1)
			{
				_preRangeTemp = _dropIfRanges[i - 1];
			}
			_curRangeTemp = _dropIfRanges[i];
			if (_curRangeTemp.InRange && _curRangeTemp.AllowStayTime != -1f)
			{
				_curRangeTemp.CurAddupTime += Time.deltaTime;
				if (_curRangeTemp.CurAddupTime >= _curRangeTemp.AllowStayTime)
				{
					_curRangeTemp.InRange = false;
					float num = Random.Range(0f, 1f);
					if (num <= _curRangeTemp.DropOutRandomPercent)
					{
						PlayerDrop();
						Debug.LogFormat("TameMonsterManager.PlayerDrop Succeed: RandomValue:{0} DropOutRandomPercent:{1}", num, _curRangeTemp.DropOutRandomPercent);
					}
					else
					{
						Debug.LogFormat("TameMonsterManager.PlayerDrop Failing: RandomValue:{0} DropOutRandomPercent:{1}", num, _curRangeTemp.DropOutRandomPercent);
					}
				}
			}
			if (_curRangeTemp.AllowStayTime != -1f)
			{
				if (((_preRangeTemp == null || _zRotate > _preRangeTemp.AngleRadius) && _zRotate <= _curRangeTemp.AngleRadius) || ((_preRangeTemp == null || _xRotate > _preRangeTemp.AngleRadius) && _xRotate <= _curRangeTemp.AngleRadius))
				{
					_curRangeTemp.InRange = true;
				}
				else
				{
					_curRangeTemp.InRange = false;
				}
			}
			_preRangeTemp = null;
			_curRangeTemp = null;
		}
	}

	private void PlayerDrop()
	{
		TameFailed();
		if ((bool)_playerEntity && (bool)_playerEntity.biologyViewCmpt)
		{
			_playerEntity.biologyViewCmpt.ActivateCollider(value: true);
		}
		if ((bool)_monsterEntity)
		{
			_monsterEntity.monstermountCtrl.HitFly(Vector3.up);
		}
	}

	public void StartTame(PeEntity playerEntity, PeEntity monsterEntity, Transform ridePosTrans)
	{
		if ((bool)monsterEntity && (bool)playerEntity)
		{
			_monsterEntity = monsterEntity;
			_playerEntity = playerEntity;
			_playerTrans = _playerEntity.peTrans;
			_ridePosTrans = ridePosTrans;
			InitDropIsRanges();
			_monsterEntity.monstermountCtrl.InitTame(_playerEntity);
			_initRotate = _playerTrans.rotation.eulerAngles;
			_inputCtrl = _ridePosTrans.gameObject.AddComponent<TameMonsterInputCtrl>();
			_inputCtrl.SetTrans(_playerEntity.peTrans);
			if ((bool)UITameMonsterCtrl.Instance)
			{
				UITameMonsterCtrl.Instance.Show();
				UITameMonsterCtrl.Instance.SetInfo(_playerEntity.peTrans);
			}
			_analogStruggle = _ridePosTrans.gameObject.AddComponent<AnalogMonsterStruggle>();
			_analogStruggle.SetInfo(_monsterEntity, ridePosTrans);
			Taming();
		}
	}

	public void ResetInfo()
	{
		_curTameState = TameState.None;
		if ((bool)_monsterEntity && (bool)_playerEntity)
		{
			SetMonsterAiState(isOpen: true);
			_monsterEntity.monstermountCtrl.TameFailure();
		}
		UITameMonsterCtrl.Instance.SetTameState(_curTameState);
		DestoryinputCtrl();
		DestoryAnalogStruggle();
		_monsterEntity = null;
		_playerEntity = null;
	}

	public void LoadTameSucceed(PeEntity playerEntity, PeEntity monsterEntity, Transform ridePosTrans)
	{
		_curTameState = TameState.TameSucceed;
		_monsterEntity = monsterEntity;
		_playerEntity = playerEntity;
		_playerTrans = _playerEntity.peTrans;
		_ridePosTrans = ridePosTrans;
		SetMonsterAiState(isOpen: false);
	}

	public void LoadTameSucceed(PeEntity playerEntity, PeEntity monsterEntity, Transform ridePosTrans, bool isNew)
	{
		_curTameState = TameState.TameSucceed;
		_monsterEntity = monsterEntity;
		_playerEntity = playerEntity;
		_playerTrans = _playerEntity.peTrans;
		_ridePosTrans = ridePosTrans;
		SetMonsterAiState(isOpen: false);
		_monsterEntity.monstermountCtrl.InitTame(_playerEntity);
		_monsterEntity.monstermountCtrl.TameSucceed();
		if (isNew && (bool)_playerEntity.mountCmpt)
		{
			_playerEntity.mountCmpt.SetMount(_monsterEntity);
		}
	}
}
