using System.Collections.Generic;
using UnityEngine;

public abstract class BSBrush : GLBehaviour
{
	public IBSDataSource dataSource;

	public BSPattern pattern;

	public int minvol = 128;

	public EBSBrushMode mode;

	public byte materialType;

	public virtual Bounds brushBound => default(Bounds);

	protected abstract void Do();

	public virtual void Cancel()
	{
	}

	public static T Create<T>(string res_path, Transform parent) where T : BSBrush
	{
		GameObject gameObject = Resources.Load(res_path) as GameObject;
		if (gameObject == null)
		{
			return (T)null;
		}
		T component = gameObject.GetComponent<T>();
		if (component == null)
		{
			return (T)null;
		}
		T val = (T)Object.Instantiate(component);
		if (val == null)
		{
			return (T)null;
		}
		val.gameObject.name = gameObject.name;
		val.transform.parent = parent;
		val.transform.localPosition = Vector3.zero;
		val.transform.localRotation = Quaternion.identity;
		val.transform.localScale = Vector3.one;
		return val;
	}

	public static T Create<T>(GameObject prefab, Transform parent) where T : BSBrush
	{
		if (prefab == null)
		{
			return (T)null;
		}
		T component = prefab.GetComponent<T>();
		if (component == null)
		{
			return (T)null;
		}
		T val = (T)Object.Instantiate(component);
		if (val == null)
		{
			return (T)null;
		}
		val.gameObject.name = prefab.name;
		val.transform.parent = parent;
		val.transform.localPosition = Vector3.zero;
		val.transform.localRotation = Quaternion.identity;
		val.transform.localScale = Vector3.one;
		return val;
	}

	public override void OnGL()
	{
	}

	protected static Vector3 CalcCursor(BSMath.DrawTarget target, IBSDataSource ds, int size)
	{
		Vector3 zero = Vector3.zero;
		IntVector3 intVector = new IntVector3(Mathf.FloorToInt(target.cursor.x * (float)ds.ScaleInverted), Mathf.FloorToInt(target.cursor.y * (float)ds.ScaleInverted), Mathf.FloorToInt(target.cursor.z * (float)ds.ScaleInverted));
		float num = (float)Mathf.FloorToInt((float)size * 0.5f) * ds.Scale;
		zero = intVector.ToVector3() * ds.Scale - new Vector3(num, num, num);
		int num2 = ((size % 2 == 0) ? 1 : 0);
		if (num != 0f)
		{
			Vector3 zero2 = Vector3.zero;
			if (target.rch.normal.x > 0f)
			{
				zero2.x += num;
			}
			else if (target.rch.normal.x < 0f)
			{
				zero2.x -= num - ds.Scale * (float)num2;
			}
			else
			{
				zero2.x = 0f;
			}
			if (target.rch.normal.y > 0f)
			{
				zero2.y += num;
			}
			else if (target.rch.normal.y < 0f)
			{
				zero2.y -= num - ds.Scale * (float)num2;
			}
			else
			{
				zero2.y = 0f;
			}
			if (target.rch.normal.z > 0f)
			{
				zero2.z += num;
			}
			else if (target.rch.normal.z < 0f)
			{
				zero2.z -= num - ds.Scale * (float)num2;
			}
			else
			{
				zero2.z = 0f;
			}
			zero += zero2;
		}
		return zero;
	}

	protected static Vector3 CalcSnapto(BSMath.DrawTarget target, IBSDataSource ds, BSPattern pt)
	{
		Vector3 zero = Vector3.zero;
		IntVector3 intVector = new IntVector3(Mathf.FloorToInt(target.snapto.x * (float)ds.ScaleInverted), Mathf.FloorToInt(target.snapto.y * (float)ds.ScaleInverted), Mathf.FloorToInt(target.snapto.z * (float)ds.ScaleInverted));
		float num = (float)Mathf.FloorToInt((float)pt.size * 0.5f) * ds.Scale;
		zero = intVector.ToVector3() * ds.Scale - new Vector3(num, num, num);
		int num2 = ((pt.size % 2 == 0) ? 1 : 0);
		if (num != 0f)
		{
			Vector3 zero2 = Vector3.zero;
			if (target.rch.normal.x > 0f)
			{
				zero2.x -= num - ds.Scale * (float)num2;
			}
			else if (target.rch.normal.x < 0f)
			{
				zero2.x += num;
			}
			else
			{
				zero2.x = 0f;
			}
			if (target.rch.normal.y > 0f)
			{
				zero2.y -= num - ds.Scale * (float)num2;
			}
			else if (target.rch.normal.y < 0f)
			{
				zero2.y += num;
			}
			else
			{
				zero2.y = 0f;
			}
			if (target.rch.normal.z > 0f)
			{
				zero2.z -= num - ds.Scale * (float)num2;
			}
			else if (target.rch.normal.z < 0f)
			{
				zero2.z += num;
			}
			else
			{
				zero2.z = 0f;
			}
			zero += zero2;
		}
		return zero;
	}

	protected static void FindExtraExtendableVoxels(IBSDataSource ds, List<BSVoxel> new_voxels, List<BSVoxel> old_voxels, List<IntVector3> indexes, Dictionary<IntVector3, int> refMap)
	{
		List<BSVoxel> list = new List<BSVoxel>();
		List<IntVector3> list2 = new List<IntVector3>();
		List<BSVoxel> list3 = new List<BSVoxel>();
		for (int i = 0; i < indexes.Count; i++)
		{
			List<IntVector4> posList = null;
			List<BSVoxel> voxels = null;
			if (!ds.ReadExtendableBlock(new IntVector4(indexes[i], 0), out posList, out voxels))
			{
				continue;
			}
			for (int j = 0; j < voxels.Count; j++)
			{
				IntVector3 intVector = new IntVector3(posList[j].x, posList[j].y, posList[j].z);
				if (!refMap.ContainsKey(intVector))
				{
					BSVoxel item = ds.Read(intVector.x, intVector.y, intVector.z);
					list3.Add(item);
					list2.Add(intVector);
					list.Add(default(BSVoxel));
				}
			}
		}
		indexes.AddRange(list2);
		new_voxels.AddRange(list);
		old_voxels.AddRange(list3);
	}
}
