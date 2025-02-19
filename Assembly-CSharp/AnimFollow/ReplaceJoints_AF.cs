using UnityEngine;

namespace AnimFollow;

[ExecuteInEditMode]
public class ReplaceJoints_AF : MonoBehaviour
{
	private void Start()
	{
		CharacterJoint[] componentsInChildren = GetComponentsInChildren<CharacterJoint>();
		int num = 0;
		CharacterJoint[] array = componentsInChildren;
		foreach (CharacterJoint characterJoint in array)
		{
			if (!characterJoint.transform.GetComponent<ConfigurableJoint>())
			{
				num++;
				ConfigurableJoint configurableJoint = characterJoint.gameObject.AddComponent<ConfigurableJoint>();
				configurableJoint.connectedBody = characterJoint.connectedBody;
				configurableJoint.anchor = characterJoint.anchor;
				configurableJoint.axis = characterJoint.axis;
				configurableJoint.secondaryAxis = characterJoint.swingAxis;
				configurableJoint.xMotion = ConfigurableJointMotion.Locked;
				configurableJoint.yMotion = ConfigurableJointMotion.Locked;
				configurableJoint.zMotion = ConfigurableJointMotion.Locked;
				configurableJoint.angularXMotion = ConfigurableJointMotion.Limited;
				configurableJoint.angularYMotion = ConfigurableJointMotion.Limited;
				configurableJoint.angularZMotion = ConfigurableJointMotion.Limited;
				configurableJoint.lowAngularXLimit = characterJoint.lowTwistLimit;
				configurableJoint.highAngularXLimit = characterJoint.highTwistLimit;
				configurableJoint.angularYLimit = characterJoint.swing1Limit;
				configurableJoint.angularZLimit = characterJoint.swing2Limit;
				configurableJoint.rotationDriveMode = RotationDriveMode.Slerp;
			}
			Object.DestroyImmediate(characterJoint);
		}
		Debug.Log("Replaced " + num + " CharacterJoints with ConfigurableJoints on " + base.name);
		Object.DestroyImmediate(this);
	}
}
