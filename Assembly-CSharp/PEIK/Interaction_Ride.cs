using RootMotion.FinalIK;

namespace PEIK;

public class Interaction_Ride : Interaction
{
	private static readonly FullBodyBipedEffector[] HelperEffector = new FullBodyBipedEffector[6]
	{
		FullBodyBipedEffector.RightHand,
		FullBodyBipedEffector.LeftHand,
		FullBodyBipedEffector.LeftFoot,
		FullBodyBipedEffector.RightFoot,
		FullBodyBipedEffector.LeftThigh,
		FullBodyBipedEffector.RightThigh
	};

	private static readonly FullBodyBipedEffector[] BeHelperEffector = new FullBodyBipedEffector[0];

	protected override string casterObjName => string.Empty;

	protected override string targetObjName => "BeRide";

	protected override FullBodyBipedEffector[] casterEffectors => HelperEffector;

	protected override FullBodyBipedEffector[] targetEffectors => BeHelperEffector;
}
