using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

namespace SkillSystem;

public class SkColliderTracker
{
	public class Positions
	{
		private const int _N = 2;

		private const float _SqrMinDist = 0.0625f;

		private Vector3[] _lstPos;

		public int _cur;

		public Vector3 this[int idx] => _lstPos[idx];

		public Positions(Vector3 pos)
		{
			_cur = 0;
			_lstPos = new Vector3[2];
			_lstPos[_cur] = pos;
		}

		public void Add(Vector3 curPos)
		{
			if (Vector3.SqrMagnitude(curPos - _lstPos[_cur]) >= 0.0625f)
			{
				_cur++;
				if (_cur >= 2)
				{
					_cur = 0;
				}
				_lstPos[_cur] = curPos;
			}
		}

		public Vector3 GetMoveVec(Vector3 curPos)
		{
			Vector3 vector = curPos - _lstPos[_cur];
			if (Vector3.SqrMagnitude(vector) >= 0.0625f)
			{
				return vector;
			}
			return curPos - _lstPos[(_cur == 0) ? 1 : _cur];
		}
	}

	private Dictionary<Collider, Positions> _colPositions = new Dictionary<Collider, Positions>();

	private SkInst _inst;

	public SkColliderTracker(SkInst inst)
	{
		_inst = inst;
		_inst._caster.StartCoroutine(Exec());
	}

	public void Add(SkCond colCond)
	{
		SkFuncInOutPara skFuncInOutPara = new SkFuncInOutPara(_inst, colCond._para);
		SkCondColDesc colDesc = skFuncInOutPara.GetColDesc();
		if (colDesc.colTypes[0] == SkCondColDesc.ColType.ColName)
		{
			string[] array = colDesc.colStrings[0];
			foreach (string name in array)
			{
				Collider collider = _inst._caster.GetCollider(name);
				if (collider != null && !_colPositions.ContainsKey(collider))
				{
					_colPositions[collider] = new Positions(collider.Pos());
				}
			}
		}
		else
		{
			Debug.LogError("Unimplemented code for getting collider from collayer cond");
		}
	}

	public bool GetMoveVec(Collider col, out Vector3 moveVec)
	{
		if (_colPositions.TryGetValue(col, out var value))
		{
			moveVec = value.GetMoveVec(col.Pos());
			return true;
		}
		moveVec = Vector3.zero;
		return false;
	}

	private IEnumerator Exec()
	{
		while (_inst.IsActive)
		{
			yield return new WaitForFixedUpdate();
			if (_colPositions.Count <= 0)
			{
				continue;
			}
			List<Collider> cols = _colPositions.Keys.Cast<Collider>().ToList();
			foreach (Collider col in cols)
			{
				if (col != null)
				{
					_colPositions[col].Add(col.Pos());
				}
			}
		}
	}
}
