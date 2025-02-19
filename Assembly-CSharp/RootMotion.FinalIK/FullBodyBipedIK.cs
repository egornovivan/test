using UnityEngine;

namespace RootMotion.FinalIK;

[AddComponentMenu("Scripts/RootMotion.FinalIK/IK/Full Body Biped IK")]
public class FullBodyBipedIK : IK
{
	public BipedReferences references = new BipedReferences();

	public IKSolverFullBodyBiped solver = new IKSolverFullBodyBiped();

	public void SetReferences(BipedReferences references, Transform rootNode)
	{
		this.references = references;
		solver.SetToReferences(this.references, rootNode);
	}

	public override IKSolver GetIKSolver()
	{
		return solver;
	}

	[ContextMenu("User Manual")]
	protected override void OpenUserManual()
	{
		Application.OpenURL("http://www.root-motion.com/finalikdox/html/page6.html");
	}

	[ContextMenu("Scrpt Reference")]
	protected override void OpenScriptReference()
	{
		Application.OpenURL("http://www.root-motion.com/finalikdox/html/class_root_motion_1_1_final_i_k_1_1_full_body_biped_i_k.html");
	}

	[ContextMenu("Reinitiate")]
	private void Reinitiate()
	{
		SetReferences(references, solver.rootNode);
	}

	[ContextMenu("Auto-detect References")]
	private void AutoDetectReferences()
	{
		references = new BipedReferences();
		BipedReferences.AutoDetectReferences(ref references, base.transform, new BipedReferences.AutoDetectParams(legsParentInSpine: true, includeEyes: false));
		solver.rootNode = IKSolverFullBodyBiped.DetectRootNodeBone(references);
		solver.SetToReferences(references, solver.rootNode);
	}
}
