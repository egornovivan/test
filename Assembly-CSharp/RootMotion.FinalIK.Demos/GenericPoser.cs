using System;
using UnityEngine;

namespace RootMotion.FinalIK.Demos;

public class GenericPoser : Poser
{
	[Serializable]
	public class Map
	{
		public Transform bone;

		public Transform target;

		public Map(Transform bone, Transform target)
		{
			this.bone = bone;
			this.target = target;
		}

		public void Update(float localRotationWeight, float localPositionWeight)
		{
			bone.localRotation = Quaternion.Lerp(bone.localRotation, target.localRotation, localRotationWeight);
			bone.localPosition = Vector3.Lerp(bone.localPosition, target.localPosition, localPositionWeight);
		}
	}

	public Map[] maps;

	[ContextMenu("Auto-Mapping")]
	public override void AutoMapping()
	{
		if (poseRoot == null)
		{
			maps = new Map[0];
			return;
		}
		maps = new Map[0];
		Transform[] componentsInChildren = base.transform.GetComponentsInChildren<Transform>();
		Transform[] componentsInChildren2 = poseRoot.GetComponentsInChildren<Transform>();
		for (int i = 1; i < componentsInChildren.Length; i++)
		{
			Transform targetNamed = GetTargetNamed(componentsInChildren[i].name, componentsInChildren2);
			if (targetNamed != null)
			{
				Array.Resize(ref maps, maps.Length + 1);
				maps[maps.Length - 1] = new Map(componentsInChildren[i], targetNamed);
			}
		}
	}

	private Transform GetTargetNamed(string tName, Transform[] array)
	{
		for (int i = 0; i < array.Length; i++)
		{
			if (array[i].name == tName)
			{
				return array[i];
			}
		}
		return null;
	}

	private void LateUpdate()
	{
		if (!(weight <= 0f) && (!(localPositionWeight <= 0f) || !(localRotationWeight <= 0f)))
		{
			float num = localRotationWeight * weight;
			float num2 = localPositionWeight * weight;
			for (int i = 0; i < maps.Length; i++)
			{
				maps[i].Update(num, num2);
			}
		}
	}
}
