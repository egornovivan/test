using PETools;
using UnityEngine;

public class AssemblyHumanMesh : MonoBehaviour
{
	[SerializeField]
	private Transform _targeModelRoot;

	[SerializeField]
	private Transform[] _MeshModelRoots;

	public void ExecAssemblyMesh()
	{
		if (!_targeModelRoot || _MeshModelRoots == null || _MeshModelRoots.Length <= 0)
		{
			return;
		}
		for (int i = 0; i < _MeshModelRoots.Length; i++)
		{
			SkinnedMeshRenderer[] componentsInChildren = _MeshModelRoots[i].GetComponentsInChildren<SkinnedMeshRenderer>(includeInactive: true);
			if (componentsInChildren == null || componentsInChildren.Length <= 0)
			{
				continue;
			}
			for (int j = 0; j < componentsInChildren.Length; j++)
			{
				SkinnedMeshRenderer skinnedMeshRenderer = componentsInChildren[j];
				GameObject gameObject = Object.Instantiate(skinnedMeshRenderer.gameObject);
				gameObject.name = skinnedMeshRenderer.gameObject.name;
				gameObject.transform.parent = _targeModelRoot.transform;
				gameObject.transform.localPosition = componentsInChildren[j].gameObject.transform.localPosition;
				gameObject.transform.localRotation = componentsInChildren[j].gameObject.transform.localRotation;
				gameObject.transform.localScale = componentsInChildren[j].gameObject.transform.localScale;
				SkinnedMeshRenderer component = gameObject.GetComponent<SkinnedMeshRenderer>();
				component.rootBone = PEUtil.GetChild(_targeModelRoot, skinnedMeshRenderer.rootBone.name, lowerCompare: true);
				if (skinnedMeshRenderer.bones != null && skinnedMeshRenderer.bones.Length > 0)
				{
					Transform[] array = new Transform[skinnedMeshRenderer.bones.Length];
					for (int k = 0; k < skinnedMeshRenderer.bones.Length; k++)
					{
						array[k] = PEUtil.GetChild(_targeModelRoot, skinnedMeshRenderer.bones[k].name, lowerCompare: true);
					}
					component.bones = array;
				}
			}
		}
	}
}
