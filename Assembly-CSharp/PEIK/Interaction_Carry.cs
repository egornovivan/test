using RootMotion.FinalIK;

namespace PEIK;

public class Interaction_Carry : Interaction
{
	private static readonly string CasterObjName = "Carry";

	private static readonly string TargetObjName = "BeCarry";

	private static readonly FullBodyBipedEffector[] CarrierEffector = new FullBodyBipedEffector[2]
	{
		FullBodyBipedEffector.LeftHand,
		FullBodyBipedEffector.RightHand
	};

	private static readonly FullBodyBipedEffector[] BeCarrierEffector = new FullBodyBipedEffector[3]
	{
		FullBodyBipedEffector.LeftHand,
		FullBodyBipedEffector.RightHand,
		FullBodyBipedEffector.Body
	};

	protected override string casterObjName => CasterObjName;

	protected override string targetObjName => TargetObjName;

	protected override FullBodyBipedEffector[] casterEffectors => CarrierEffector;

	protected override FullBodyBipedEffector[] targetEffectors => BeCarrierEffector;
}
