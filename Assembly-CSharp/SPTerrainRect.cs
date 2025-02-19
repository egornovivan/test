using System.Collections.Generic;
using uLink;
using UnityEngine;
using WhiteCat;

public class SPTerrainRect : UnityEngine.MonoBehaviour
{
	private int mCount;

	private IntVector4 mPosition;

	private List<SPPoint> mPoints;

	private List<IntVector2> mMeshNodes;

	private List<IntVector2> mCaveNodes;

	private List<Vector3> mEventPosition;

	private SimplexNoise mNoise;

	public IntVector4 position => mPosition;

	public List<SPPoint> points => mPoints;

	public List<IntVector2> meshNodes => mMeshNodes;

	public IntVector4 nextIndex => new IntVector4(mPosition.x, mPosition.z, mCount++, 0);

	public IntVector4 currentIndex => new IntVector4(mPosition.x, mPosition.z, mCount + 1, 0);

	public static SPTerrainRect InstantiateSPTerrainRect(IntVector4 pos, int minCount, int maxCount, Transform parent, SimplexNoise noise)
	{
		GameObject gameObject = new GameObject(pos.x + " , " + pos.z);
		gameObject.transform.parent = parent;
		gameObject.transform.position = pos.ToVector3();
		SPTerrainRect sPTerrainRect = gameObject.AddComponent<SPTerrainRect>();
		sPTerrainRect.Init(pos, minCount, maxCount, noise);
		return sPTerrainRect;
	}

	public void Init(IntVector4 pos, int minCount, int maxCount, SimplexNoise noise)
	{
		mCount = 0;
		mNoise = noise;
		mPosition = pos;
		mMeshNodes = new List<IntVector2>();
		mCaveNodes = new List<IntVector2>();
		mPoints = new List<SPPoint>();
		Spawn(pos, minCount, maxCount);
	}

	public void Destroy()
	{
		Cleanup();
		Object.Destroy(base.gameObject);
	}

	public void Cleanup()
	{
		foreach (SPPoint mPoint in mPoints)
		{
			if (mPoint != null)
			{
				mPoint.Activate(value: false);
			}
		}
		mPoints.Clear();
	}

	public bool IsMarkExistCave(IntVector2 mark)
	{
		return mCaveNodes.Contains(mark);
	}

	public void RegisterCavMark(IntVector2 mark)
	{
		if (!mCaveNodes.Contains(mark))
		{
			mCaveNodes.Add(mark);
		}
	}

	public void RegisterSPPoint(SPPoint point)
	{
		if (!(point == null) && !mPoints.Contains(point))
		{
			mPoints.Add(point);
		}
	}

	public void RegisterMeshNode(IntVector4 node)
	{
		IntVector2 item = AiUtil.ConvertToIntVector2FormLodLevel(node, LODOctreeMan._maxLod);
		if (!mMeshNodes.Contains(item))
		{
			mMeshNodes.Add(item);
		}
	}

	public void RemoveMeshNode(IntVector4 node)
	{
		IntVector2 item = AiUtil.ConvertToIntVector2FormLodLevel(node, LODOctreeMan._maxLod);
		if (mMeshNodes.Contains(item))
		{
			mMeshNodes.Remove(item);
		}
	}

	private bool Match(SPPoint point, IntVector4 node)
	{
		if (point == null)
		{
			return false;
		}
		Rect rect = new Rect(node.x, node.z, 32 << node.w, 32 << node.w);
		Vector2 point2 = new Vector2(point.position.x, point.position.z);
		return rect.Contains(point2);
	}

	private void LoadNoisePoints(IntVector4 node, int minCount, int maxCount)
	{
		int num = node.x >> 5 >> node.w;
		int num2 = node.z >> 5 >> node.w;
		float num3 = (float)mNoise.Noise(num, num2, num + num2);
		int num4 = Mathf.FloorToInt((float)(maxCount - minCount) * num3);
		int num5 = Mathf.Clamp(minCount + num4, minCount, maxCount);
		int num6 = 32 << node.w;
		for (int i = 0; i < num5; i++)
		{
			float num7 = (float)mNoise.Noise(num, (num + num2) * i) * 0.5f + 0.5f;
			float num8 = (float)mNoise.Noise(num2, (num - num2) * i) * 0.5f + 0.5f;
			Vector3 vector = new Vector3((float)node.x + num7 * (float)num6, node.y, (float)node.z + num8 * (float)num6);
			Quaternion rotation = Quaternion.Euler(0f, Random.Range(0, 360), 0f);
			SPPoint sPPoint = SPPoint.InstantiateSPPoint<SPPoint>(vector, rotation, nextIndex, base.transform, 0, 0, isActive: true, revisePos: true, isBoss: false, erode: true, delete: true, mNoise);
			sPPoint.name = "Noise : " + sPPoint.name;
			RegisterSPPoint(sPPoint);
		}
	}

	private void LoadNoiseBossPoints(IntVector4 node)
	{
		int num = node.x >> 5 >> node.w;
		int num2 = node.z >> 5 >> node.w;
		float num3 = (float)mNoise.Noise(num, num2);
		if (num3 < -0.5f)
		{
			int num4 = 32 << node.w;
			float num5 = (float)mNoise.Noise(num, num2, num + num2) * 0.5f + 0.5f;
			float num6 = (float)mNoise.Noise(num2, num, num - num2) * 0.5f + 0.5f;
			Vector3 vector = new Vector3((float)node.x + num5 * (float)num4, node.y, (float)node.z + num6 * (float)num4);
			if (AIErodeMap.IsInErodeArea(vector) == null)
			{
				Quaternion rotation = Quaternion.Euler(0f, Random.Range(0, 360), 0f);
				SPPoint sPPoint = SPPoint.InstantiateSPPoint<SPPointBoss>(vector, rotation, nextIndex, base.transform, 0, 0, isActive: true, revisePos: true, isBoss: true, erode: true, delete: true, mNoise);
				sPPoint.name = "Noise boss : " + sPPoint.name;
				RegisterSPPoint(sPPoint);
			}
		}
	}

	private void LoadDynamicPoints(IntVector4 node, int min, int max)
	{
		int num = Random.Range(min, max);
		for (int i = 0; i < num; i++)
		{
			Vector3 vector = node.ToVector3();
			vector += new Vector3(Random.Range(0f, 32 << node.w), 0f, Random.Range(0f, 32 << node.w));
			Quaternion rotation = Quaternion.Euler(0f, Random.Range(0, 360), 0f);
			SPPoint sPPoint = SPPoint.InstantiateSPPoint<SPPoint>(vector, rotation, nextIndex, base.transform);
			sPPoint.name = "Dynamic : " + sPPoint.name;
			RegisterSPPoint(sPPoint);
		}
	}

	private void LoadUpperAirPoints(IntVector4 node, int min, int max)
	{
		GameObject gameObject = null;
		if (!(gameObject == null))
		{
			HelicopterController component = gameObject.GetComponent<HelicopterController>();
			if (!(component == null))
			{
				Vector3 vector = node.ToVector3();
				vector += new Vector3(Random.Range(0f, 32 << node.w), 0f, Random.Range(0f, 32 << node.w));
				Quaternion rotation = Quaternion.Euler(0f, Random.Range(0, 360), 0f);
				SPPoint sPPoint = SPPoint.InstantiateSPPoint<SPPoint>(vector, rotation, nextIndex, base.transform, 0, 63);
				sPPoint.name = "Upper Air : " + sPPoint.name;
				RegisterSPPoint(sPPoint);
			}
		}
	}

	private void LoadStaticPoints(IntVector4 node)
	{
		List<SPPoint> list = AISpawnPoint.GetPoints(node);
		foreach (SPPoint item in list)
		{
			item.index = nextIndex;
			RegisterSPPoint(item);
		}
	}

	private void Spawn(IntVector4 node, int min, int max)
	{
	}

	public static void WriteSPTerrainRect(uLink.BitStream stream, object obj, params object[] codecOptions)
	{
		SPTerrainRect sPTerrainRect = obj as SPTerrainRect;
		stream.Write(sPTerrainRect.mCount);
		stream.Write(sPTerrainRect.mPosition);
		stream.Write(sPTerrainRect.mPoints.ToArray());
	}

	public static object ReadSPTerrainRect(uLink.BitStream stream, params object[] codecOptions)
	{
		SPTerrainRect sPTerrainRect = new SPTerrainRect();
		sPTerrainRect.mCount = stream.Read<int>(new object[0]);
		sPTerrainRect.mPosition = stream.Read<IntVector4>(new object[0]);
		sPTerrainRect.mPoints = new List<SPPoint>();
		SPPoint[] collection = stream.Read<SPPoint[]>(new object[0]);
		sPTerrainRect.mPoints.AddRange(collection);
		return sPTerrainRect;
	}
}
