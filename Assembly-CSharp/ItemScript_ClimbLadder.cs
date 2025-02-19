using Pathea.Operate;
using UnityEngine;

public class ItemScript_ClimbLadder : ItemScript_Connection
{
	public enum OpSide
	{
		Both,
		Forward,
		Backward
	}

	public OpSide m_OpSide;

	[Tooltip("楼梯高度")]
	public float m_LadderHeight;

	[Tooltip("楼梯宽度")]
	public float m_LadderWith;

	private PEClimbLadder _opnd;

	public PEClimbLadder opClimb => _opnd;

	private void Awake()
	{
		_opnd = GetComponent<PEClimbLadder>();
		if (_opnd == null)
		{
			_opnd = base.gameObject.AddComponent<PEClimbLadder>();
		}
		_opnd.opSide = m_OpSide;
	}
}
