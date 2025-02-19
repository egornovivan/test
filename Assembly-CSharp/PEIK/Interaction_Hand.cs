using RootMotion.FinalIK;

namespace PEIK;

public class Interaction_Hand : Interaction
{
	private static readonly string CasterObjName = "Hand";

	private static readonly string TargetObjName = "BeHand";

	private static readonly FullBodyBipedEffector[] HelperEffector = new FullBodyBipedEffector[1] { FullBodyBipedEffector.RightHand };

	private static readonly FullBodyBipedEffector[] BeHelperEffector = new FullBodyBipedEffector[0];

	protected override string casterObjName => CasterObjName;

	protected override string targetObjName => TargetObjName;

	protected override FullBodyBipedEffector[] casterEffectors => HelperEffector;

	protected override FullBodyBipedEffector[] targetEffectors => BeHelperEffector;
}
