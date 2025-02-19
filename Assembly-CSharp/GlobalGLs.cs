using System.Collections.Generic;
using UnityEngine;

public class GlobalGLs : MonoBehaviour
{
	private static List<GLBehaviour> s_listGLs = new List<GLBehaviour>();

	public static void AddGL(GLBehaviour gl)
	{
		if (s_listGLs != null)
		{
			s_listGLs.Add(gl);
		}
		else
		{
			Debug.LogError("Add GL Failed");
		}
	}

	public static void RemoveGL(GLBehaviour gl)
	{
		if (s_listGLs != null)
		{
			s_listGLs.Remove(gl);
		}
	}

	private void OnPostRender()
	{
		if (LSubTerrainMgr.Instance != null)
		{
			LSubTerrainMgr.Instance.m_Editor.DoGL();
		}
		foreach (GLBehaviour s_listGL in s_listGLs)
		{
			if (s_listGL != null)
			{
				s_listGL.OnGL();
			}
		}
	}
}
