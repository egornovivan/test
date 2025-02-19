using System;
using Pathea;

[Serializable]
public class ActiveAttr
{
	public string[] m_Layers;

	public string m_PutOnBone = "mountMain";

	public string m_PutOffBone = "Long_Gun";

	public string m_PutOnAnim = "SwordPutOn";

	public string m_PutOffAnim = "SwordPutOff";

	public CameraModeData m_CamMode;

	public PEActionType m_ActiveActionType = PEActionType.GunHold;

	public PEActionType m_UnActiveActionType = PEActionType.GunPutOff;

	public PEActionMask m_HoldActionMask = PEActionMask.GunHold;

	public MoveStyle m_BaseMoveStyle;

	public MoveStyle m_MoveStyle;

	public float m_AimIKAngleRange = 40f;
}
