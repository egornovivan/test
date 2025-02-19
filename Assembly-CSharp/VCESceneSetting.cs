using System;
using UnityEngine;

[Serializable]
public class VCESceneSetting
{
	public int m_Id;

	public int m_ParentId;

	public string m_Name = string.Empty;

	public EVCCategory m_Category;

	public IntVector3 m_EditorSize;

	public float m_VoxelSize;

	public int m_MajorInterval;

	public int m_MinorInterval;

	public float m_BlockUnit;

	public float m_DyeUnit;

	public Vector3 EditorWorldSize => m_EditorSize.ToVector3() * m_VoxelSize;
}
