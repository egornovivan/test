using System;
using System.Collections.Generic;
using UnityEngine;

public class MCOutputData
{
	private static GenericPool<MCOutputData> s_dataPool = new GenericPool<MCOutputData>(672);

	private Vector3[] _pos;

	private Vector2[] _norm01;

	private Vector2[] _norm2t;

	private int[] _indice;

	public MCOutputData()
	{
		_pos = new Vector3[3999];
		_norm01 = new Vector2[3999];
		_norm2t = new Vector2[3999];
		_indice = null;
	}

	public MCOutputData(Vector3[] verts, Vector2[] norm01, Vector2[] norm2t, int[] indice)
	{
		_pos = verts;
		_norm01 = norm01;
		_norm2t = norm2t;
		_indice = indice;
	}

	public void Clone(MCOutputData data)
	{
		_pos = data._pos;
		_norm01 = data._norm01;
		_norm2t = data._norm2t;
		_indice = data._indice;
	}

	public void SetToMesh(Mesh mesh)
	{
		mesh.MarkDynamic();
		mesh.vertices = _pos;
		mesh.uv = _norm01;
		mesh.uv2 = _norm2t;
		mesh.triangles = _indice;
	}

	public static MCOutputData Get(Vector3[] srcPosArray, Vector2[] srcNorm01Array, Vector2[] srcNorm2tArray, int srcIdx, int len)
	{
		if (len > 3999)
		{
			Vector3[] array = new Vector3[len];
			Vector2[] array2 = new Vector2[len];
			Vector2[] array3 = new Vector2[len];
			int[] array4 = new int[len];
			Array.Copy(srcPosArray, srcIdx, array, 0, len);
			Array.Copy(srcNorm01Array, srcIdx, array2, 0, len);
			Array.Copy(srcNorm2tArray, srcIdx, array3, 0, len);
			Array.Copy(SurfExtractorsMan.s_indiceMax, array4, len);
			return new MCOutputData(array, array2, array3, array4);
		}
		MCOutputData mCOutputData = s_dataPool.Get();
		Array.Copy(srcPosArray, srcIdx, mCOutputData._pos, 0, len);
		Array.Copy(srcNorm01Array, srcIdx, mCOutputData._norm01, 0, len);
		Array.Copy(srcNorm2tArray, srcIdx, mCOutputData._norm2t, 0, len);
		if (len == 3999)
		{
			mCOutputData._indice = SurfExtractorsMan.s_indice4Pool;
		}
		else if (mCOutputData._indice == null || len != mCOutputData._indice.Length)
		{
			mCOutputData._indice = new int[len];
			Array.Copy(SurfExtractorsMan.s_indiceMax, mCOutputData._indice, len);
		}
		return mCOutputData;
	}

	public static MCOutputData Get(List<Vector3> srcPosLst, List<Vector2> srcNorm01Lst, List<Vector2> srcNorm2tLst, int srcIdx, int len)
	{
		if (len > 3999)
		{
			Vector3[] array = new Vector3[len];
			Vector2[] array2 = new Vector2[len];
			Vector2[] array3 = new Vector2[len];
			int[] array4 = new int[len];
			srcPosLst.CopyTo(srcIdx, array, 0, len);
			srcNorm01Lst.CopyTo(srcIdx, array2, 0, len);
			srcNorm2tLst.CopyTo(srcIdx, array3, 0, len);
			Array.Copy(SurfExtractorsMan.s_indiceMax, array4, len);
			return new MCOutputData(array, array2, array3, array4);
		}
		MCOutputData mCOutputData = s_dataPool.Get();
		srcPosLst.CopyTo(srcIdx, mCOutputData._pos, 0, len);
		srcNorm01Lst.CopyTo(srcIdx, mCOutputData._norm01, 0, len);
		srcNorm2tLst.CopyTo(srcIdx, mCOutputData._norm2t, 0, len);
		if (len == 3999)
		{
			mCOutputData._indice = SurfExtractorsMan.s_indice4Pool;
		}
		else if (mCOutputData._indice == null || len != mCOutputData._indice.Length)
		{
			mCOutputData._indice = new int[len];
			Array.Copy(SurfExtractorsMan.s_indiceMax, mCOutputData._indice, len);
		}
		return mCOutputData;
	}

	public static void Free(MCOutputData data)
	{
		if (data._pos.Length == 3999)
		{
			s_dataPool.Free(data);
			data._indice = null;
			return;
		}
		data._pos = null;
		data._norm01 = null;
		data._norm2t = null;
		data._indice = null;
	}
}
