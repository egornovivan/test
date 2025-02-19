using UnityEngine;

public interface IRagdollHandler
{
	void OnRagdollBuild(GameObject obj);

	void OnDetachJoint(GameObject boneRagdoll);

	void OnReattachJoint(GameObject boneRagdoll);

	void OnAttachJoint(GameObject boneRagdoll);

	void OnFallBegin(GameObject ragdoll);

	void OnFallFinished(GameObject ragdoll);

	void OnGetupBegin(GameObject ragdoll);

	void OnGetupFinished(GameObject ragdoll);

	void ActivateRagdollRenderer(bool isActive);
}
