using Pathea;
using UnityEngine;

public class TameMonsterInputCtrl : MonoBehaviour
{
	private Quaternion _curRotate;

	private PeTrans _targetTrans;

	private float _tempX;

	private float _tempZ;

	private Vector3 _offsetRotate;

	public Quaternion InitRotate { get; private set; }

	public Vector3 OffsetRotate => _offsetRotate;

	private void Update()
	{
		if ((bool)_targetTrans)
		{
			_tempZ = PeInput.GetAxisH();
			_tempX = PeInput.GetAxisV();
			if (Mathf.Abs(_tempX) > float.Epsilon)
			{
				_offsetRotate.x = _tempX * Time.deltaTime * TameMonsterConfig.instance.CtrlRotateSpeed;
			}
			else
			{
				_offsetRotate.x = 0f;
			}
			if (Mathf.Abs(_tempZ) > float.Epsilon)
			{
				_offsetRotate.z = (0f - _tempZ * Time.deltaTime) * TameMonsterConfig.instance.CtrlRotateSpeed;
			}
			else
			{
				_offsetRotate.z = 0f;
			}
			_offsetRotate.y = 0f;
		}
	}

	public void SetTrans(PeTrans trans)
	{
		if ((bool)trans)
		{
			_targetTrans = trans;
			InitRotate = _targetTrans.transform.rotation;
		}
	}
}
