using UnityEngine;
using WhiteCat.UnityExtension;

namespace WhiteCat;

public class ArmorBones : MonoBehaviour
{
	[SerializeField]
	private BoneNode[] _nodes;

	private static int[] groupFirst = new int[5] { 0, 1, 4, 12, 0 };

	public static string[][] boneNames = new string[4][]
	{
		new string[1] { "Bip01 Head" },
		new string[3] { "Bip01 Spine3", "Bip01 Spine2", "Bip01 Spine1" },
		new string[8] { "Bip01 L UpperArm", "Bip01 R UpperArm", "Bip01 L Forearm", "Bip01 R Forearm", "Bip01 R Thigh", "Bip01 L Thigh", "Bip01 L Calf", "Bip01 R Calf" },
		new string[4] { "Bip01 L Hand", "Bip01 R Hand", "Bip01 L Foot", "Bip01 R Foot" }
	};

	public BoneNode nodes(int boneGroup, int boneIndex)
	{
		return _nodes[groupFirst[boneGroup] + boneIndex];
	}

	public BoneNode nodes(int index)
	{
		return _nodes[index];
	}

	public Transform GetArmorPart(int index, bool isDecoration)
	{
		return (!isDecoration) ? _nodes[index].normal : _nodes[index].decoration;
	}

	private Transform FindChild(string name)
	{
		return base.transform.TraverseHierarchy((Transform t, int i) => (t.gameObject.name == name) ? t : null) as Transform;
	}

	private void Reset()
	{
		_nodes = new BoneNode[16];
		int num = 0;
		for (int i = 0; i < boneNames.Length; i++)
		{
			for (int j = 0; j < boneNames[i].Length; j++)
			{
				_nodes[num] = new BoneNode();
				_nodes[num].bone = FindChild(boneNames[i][j]);
				num++;
			}
		}
	}
}
