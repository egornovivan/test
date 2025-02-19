using System.Collections.Generic;
using System.Runtime.InteropServices;
using Pathea;
using PETools;
using UnityEngine;

public class AnalogMonsterStruggle : MonoBehaviour
{
	public enum MonsterAction
	{
		None,
		Forward,
		Back,
		Left,
		Right,
		Jump,
		MoveDirtion
	}

	[StructLayout(LayoutKind.Sequential, Size = 1)]
	public struct SingleAction
	{
		public MonsterAction ActionType { get; private set; }

		public float ExecTime { get; private set; }

		public SpeedState Speed { get; private set; }

		public Vector3 Dirtion { get; private set; }

		public SingleAction(MonsterAction actionType, float execTime, SpeedState speed, Vector3 dirtion)
		{
			ActionType = actionType;
			ExecTime = execTime;
			Speed = speed;
			Dirtion = dirtion;
		}
	}

	public class SingleActionSeed
	{
		private TameMonsterConfig.SingleActionSeedInfo _info;

		public MonsterAction Action => _info.Action;

		public float RandomTime => (_info.TimeRandomRange.Min != _info.TimeRandomRange.Max) ? Random.Range(_info.TimeRandomRange.Min, _info.TimeRandomRange.Max) : _info.TimeRandomRange.Min;

		public float ActionPercent { get; private set; }

		public SpeedState RandomSpeed
		{
			get
			{
				switch (Action)
				{
				case MonsterAction.Forward:
				case MonsterAction.Back:
				case MonsterAction.Left:
				case MonsterAction.Right:
				case MonsterAction.MoveDirtion:
				{
					float num = Random.Range(0f, _info.WalkRandomPercent + _info.RunRandomPercent + _info.SprintRandomPercent - float.Epsilon);
					if (num <= _info.WalkRandomPercent)
					{
						return SpeedState.Walk;
					}
					num += _info.WalkRandomPercent;
					if (num <= _info.RunRandomPercent)
					{
						return SpeedState.Run;
					}
					return SpeedState.Sprint;
				}
				case MonsterAction.None:
				case MonsterAction.Jump:
					return SpeedState.None;
				default:
					return SpeedState.None;
				}
			}
		}

		public SingleActionSeed(TameMonsterConfig.SingleActionSeedInfo info, float actionPercent)
		{
			_info = info;
			ActionPercent = actionPercent;
		}
	}

	private Transform _ridePosTrans;

	private PeEntity _monsterEntity;

	private MonstermountCtrl _mMCtrl;

	private Vector3 _offsetRotate;

	private Vector3 _lastPosition;

	private Vector3 _lastVelocity;

	private Vector3 _lastUp;

	private Quaternion _lastAngularVelocity;

	private Vector3 _backupOffsetRotate;

	private Dictionary<MonsterAction, SingleActionSeed> _actionSeedDic;

	private float _percentTotal;

	private Queue<SingleAction> _struggleActions;

	private SingleAction _curActionInfo;

	public Vector3 OffsetRotate => _offsetRotate;

	public bool IsTameSucceed { get; private set; }

	public int CurTameProgress => (_struggleActions != null) ? (StruggleActionTotal - _struggleActions.Count) : 0;

	public int StruggleActionTotal { get; private set; }

	private void Update()
	{
		if ((bool)_ridePosTrans)
		{
			Vector3 position = _ridePosTrans.position;
			Vector3 vector = (position - _lastPosition) / Time.deltaTime;
			_lastPosition = position;
			Vector3 direction = vector - _lastVelocity;
			_lastVelocity = vector;
			_offsetRotate = _ridePosTrans.InverseTransformDirection(direction).normalized * direction.magnitude * TameMonsterConfig.instance.MonsterMoveFactor;
			Vector3 up = _ridePosTrans.up;
			Quaternion.FromToRotation(_lastUp, up).ToAngleAxis(out var angle, out var axis);
			angle /= Time.deltaTime;
			Quaternion quaternion = Quaternion.AngleAxis(angle, axis);
			_lastUp = up;
			_offsetRotate += _ridePosTrans.InverseTransformDirection(quaternion * _ridePosTrans.up) * TameMonsterConfig.instance.MonsterRotateFactor;
			_offsetRotate.y = _offsetRotate.z;
			_offsetRotate.z = 0f - _offsetRotate.x;
			_offsetRotate.x = _offsetRotate.y;
			_offsetRotate.y = 0f;
		}
	}

	private float VectorToAngle(Vector3 v1, Vector3 v2)
	{
		return Mathf.Asin(Vector3.Distance(Vector3.zero, Vector3.Cross(v1.normalized, v2.normalized))) * 57.29578f;
	}

	private float VectorToAngleZ(Vector3 v1, Vector3 v2)
	{
		v1.z = 0f;
		v2.z = 0f;
		return (!(Vector3.Cross(v1, v2).z > 0f)) ? VectorToAngle(v1, v2) : (0f - VectorToAngle(v1, v2));
	}

	private float VectorToAngleX(Vector3 v1, Vector3 v2)
	{
		v1.x = 0f;
		v2.x = 0f;
		return (!(Vector3.Cross(v1, v2).x > 0f)) ? (0f - VectorToAngle(v1, v2)) : VectorToAngle(v1, v2);
	}

	private void BindAction()
	{
		if (null == _mMCtrl || TameMonsterConfig.instance.ActionList.Count <= 0)
		{
			return;
		}
		_actionSeedDic = new Dictionary<MonsterAction, SingleActionSeed>();
		_percentTotal = 0f;
		for (int i = 0; i < TameMonsterConfig.instance.ActionList.Count; i++)
		{
			TameMonsterConfig.SingleActionSeedInfo info = TameMonsterConfig.instance.ActionList[i];
			if (info.Action != MonsterAction.Jump || (info.Action == MonsterAction.Jump && _mMCtrl.HasJump()))
			{
				_percentTotal += info.RandomPercent;
				_actionSeedDic[info.Action] = new SingleActionSeed(info, _percentTotal);
			}
		}
	}

	private void GenerateStruggleActions()
	{
		if (_actionSeedDic == null)
		{
			return;
		}
		Bounds monsterBounds = GetMonsterBounds();
		float value = monsterBounds.size.x * monsterBounds.size.y * monsterBounds.size.z;
		value = Mathf.Clamp(value, TameMonsterConfig.instance.MonsterBulkRange.Min, TameMonsterConfig.instance.MonsterBulkRange.Max);
		float num = (value - TameMonsterConfig.instance.MonsterBulkRange.Min) / (TameMonsterConfig.instance.MonsterBulkRange.Max - TameMonsterConfig.instance.MonsterBulkRange.Min);
		float value2 = num * TameMonsterConfig.instance.MonsterStruggleActionSizeRange.Max;
		StruggleActionTotal = (int)Mathf.Clamp(value2, TameMonsterConfig.instance.MonsterStruggleActionSizeRange.Min, TameMonsterConfig.instance.MonsterStruggleActionSizeRange.Max);
		_struggleActions = new Queue<SingleAction>();
		Vector3 dirtion = Vector3.one;
		for (int i = 0; i < StruggleActionTotal; i++)
		{
			MonsterAction randomAction = GetRandomAction();
			if (_actionSeedDic.ContainsKey(randomAction))
			{
				if (randomAction == MonsterAction.MoveDirtion)
				{
					dirtion = GetRandomDirtion();
				}
				SingleAction item = new SingleAction(randomAction, _actionSeedDic[randomAction].RandomTime, _actionSeedDic[randomAction].RandomSpeed, dirtion);
				_struggleActions.Enqueue(item);
			}
		}
	}

	private MonsterAction GetRandomAction()
	{
		if (_actionSeedDic == null)
		{
			return MonsterAction.None;
		}
		float num = Random.Range(0f, _percentTotal - float.Epsilon);
		foreach (KeyValuePair<MonsterAction, SingleActionSeed> item in _actionSeedDic)
		{
			if (num <= item.Value.ActionPercent)
			{
				return item.Key;
			}
		}
		return MonsterAction.None;
	}

	private Bounds GetMonsterBounds()
	{
		Bounds result = default(Bounds);
		if ((bool)_monsterEntity && (bool)_monsterEntity.peTrans)
		{
			SkinnedMeshRenderer componentInChildren = _monsterEntity.peTrans.realTrans.GetComponentInChildren<SkinnedMeshRenderer>();
			if (componentInChildren == null)
			{
				return result;
			}
			componentInChildren.updateWhenOffscreen = true;
			return componentInChildren.bounds;
		}
		return result;
	}

	private Vector3 GetRandomDirtion()
	{
		Vector3 result = Vector3.one;
		if ((bool)_monsterEntity && (bool)_monsterEntity.peTrans)
		{
			Vector3 position = Vector3.one;
			if (!PEUtil.GetNearbySafetyPos(_monsterEntity.peTrans, PEUtil.GetOffRideMask, PEUtil.Standlayer, ref position))
			{
				float num = Mathf.Max(_monsterEntity.peTrans.bound.size.x, _monsterEntity.peTrans.bound.size.z);
				position = PEUtil.GetEmptyPositionOnGround(_monsterEntity.peTrans.position + _monsterEntity.peTrans.bound.center, num, num + 3f);
			}
			result = _monsterEntity.peTrans.position - position;
		}
		return result;
	}

	private void ExecAction(SingleAction actionInfo)
	{
		if ((bool)_mMCtrl && actionInfo.ActionType != 0)
		{
			switch (actionInfo.ActionType)
			{
			case MonsterAction.Forward:
				_mMCtrl.Forward(ActionCallBack, actionInfo.ExecTime, actionInfo.Speed);
				break;
			case MonsterAction.Back:
				_mMCtrl.Back(ActionCallBack, actionInfo.ExecTime, actionInfo.Speed);
				break;
			case MonsterAction.Left:
				_mMCtrl.Left(ActionCallBack, actionInfo.ExecTime, actionInfo.Speed);
				break;
			case MonsterAction.Right:
				_mMCtrl.Right(ActionCallBack, actionInfo.ExecTime, actionInfo.Speed);
				break;
			case MonsterAction.Jump:
				_mMCtrl.Jump(ActionCallBack);
				break;
			case MonsterAction.MoveDirtion:
				_mMCtrl.MoveDirtion(actionInfo.Dirtion, ActionCallBack, actionInfo.ExecTime, actionInfo.Speed);
				break;
			}
		}
	}

	private void ActionCallBack()
	{
		StartMonsterStruggle();
	}

	private void StartMonsterStruggle()
	{
		if (_struggleActions == null)
		{
			Debug.Log("AnalogMonsterStruggle: _struggleActions is null!");
		}
		else if (_struggleActions.Count > 0)
		{
			_curActionInfo = _struggleActions.Dequeue();
			ExecAction(_curActionInfo);
			IsTameSucceed = false;
		}
		else
		{
			IsTameSucceed = true;
		}
	}

	public void SetInfo(PeEntity monsterEntity, Transform ridePosTrans)
	{
		if ((bool)monsterEntity && (bool)ridePosTrans)
		{
			_monsterEntity = monsterEntity;
			_mMCtrl = _monsterEntity.monstermountCtrl;
			_ridePosTrans = ridePosTrans;
			_lastPosition = _ridePosTrans.position;
			_lastUp = _ridePosTrans.up;
			_lastAngularVelocity = Quaternion.identity;
			IsTameSucceed = false;
			BindAction();
			GenerateStruggleActions();
			StartMonsterStruggle();
		}
	}
}
