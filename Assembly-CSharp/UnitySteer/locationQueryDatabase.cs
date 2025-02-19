using System.Collections;
using UnityEngine;

namespace UnitySteer;

internal class locationQueryDatabase
{
	public float originx;

	public float originy;

	public float originz;

	public float sizex;

	public float sizey;

	public float sizez;

	public int divx;

	public int divy;

	public int divz;

	public lqBin[] bins;

	public lqBin other;

	private int bincount;

	public locationQueryDatabase(float _originx, float _originy, float _originz, float _sizex, float _sizey, float _sizez, int _divx, int _divy, int _divz)
	{
		originx = _originx;
		originy = _originy;
		originz = _originz;
		sizex = _sizex;
		sizey = _sizey;
		sizez = _sizez;
		divx = _divx;
		divy = _divy;
		divz = _divz;
		bincount = divx * divy * divz;
		bins = new lqBin[bincount];
		for (int i = 0; i < divx; i++)
		{
			for (int j = 0; j < divy; j++)
			{
				for (int k = 0; k < divz; k++)
				{
					int num = i * divy * divz + j * divz + k;
					float x = originx + (float)i * (sizex / (float)divx);
					float y = originy + (float)j * (sizey / (float)divy);
					float z = originz + (float)k * (sizez / (float)divz);
					Vector3 binCenter = new Vector3(x, y, z);
					bins[num] = new lqBin(binCenter);
				}
			}
		}
		other = new lqBin(Vector3.zero);
	}

	public int lqBinCoordsToBinIndex(float ix, float iy, float iz)
	{
		return (int)(ix * (float)divy * (float)divz + iy * (float)divz + iz);
	}

	public lqBin lqBinForLocation(float x, float y, float z)
	{
		if (x < originx)
		{
			return other;
		}
		if (y < originy)
		{
			return other;
		}
		if (z < originz)
		{
			return other;
		}
		if (x >= originx + sizex)
		{
			return other;
		}
		if (y >= originy + sizey)
		{
			return other;
		}
		if (z >= originz + sizez)
		{
			return other;
		}
		int num = (int)((x - originx) / sizex * (float)divx);
		int num2 = (int)((y - originy) / sizey * (float)divy);
		int num3 = (int)((z - originz) / sizez * (float)divz);
		int num4 = lqBinCoordsToBinIndex(num, num2, num3);
		if (num4 < 0 || num4 >= bincount)
		{
			return other;
		}
		return bins[num4];
	}

	public void lqAddToBin(lqClientProxy clientObject, lqBin bin)
	{
		bin.clientList.Add(clientObject);
		clientObject.bin = bin;
	}

	public void lqRemoveFromBin(lqClientProxy clientObject)
	{
		if (clientObject.bin != null)
		{
			clientObject.bin.clientList.Remove(clientObject);
		}
	}

	public void lqUpdateForNewLocation(lqClientProxy clientObject, float x, float y, float z)
	{
		lqBin lqBin2 = lqBinForLocation(x, y, z);
		clientObject.x = x;
		clientObject.y = y;
		clientObject.z = z;
		if (lqBin2 != clientObject.bin)
		{
			lqRemoveFromBin(clientObject);
			lqAddToBin(clientObject, lqBin2);
		}
	}

	public ArrayList getBinClientObjectList(lqBin bin, float x, float y, float z, float radiusSquared)
	{
		ArrayList arrayList = new ArrayList();
		for (int i = 0; i < bin.clientList.Count; i++)
		{
			lqClientProxy lqClientProxy2 = (lqClientProxy)bin.clientList[i];
			float num = x - lqClientProxy2.x;
			float num2 = y - lqClientProxy2.y;
			float num3 = z - lqClientProxy2.z;
			float num4 = num * num + num2 * num2 + num3 * num3;
			if (num4 < radiusSquared)
			{
				arrayList.Add(lqClientProxy2);
			}
		}
		return arrayList;
	}

	public ArrayList getAllClientObjectsInLocalityClipped(float x, float y, float z, float radius, int minBinX, int minBinY, int minBinZ, int maxBinX, int maxBinY, int maxBinZ)
	{
		int num = divy * divz;
		int num2 = divz;
		int num3 = minBinX * num;
		int num4 = minBinY * num2;
		float radiusSquared = radius * radius;
		ArrayList arrayList = new ArrayList();
		int num5 = num3;
		for (int i = minBinX; i <= maxBinX; i++)
		{
			int num6 = num4;
			for (int j = minBinY; j <= maxBinY; j++)
			{
				int num7 = minBinZ;
				for (int k = minBinZ; k <= maxBinZ; k++)
				{
					lqBin bin = bins[num5 + num6 + num7];
					ArrayList binClientObjectList = getBinClientObjectList(bin, x, y, z, radiusSquared);
					arrayList.AddRange(binClientObjectList);
					num7++;
				}
				num6 += num2;
			}
			num5 += num;
		}
		return arrayList;
	}

	public ArrayList getAllOutsideObjects(float x, float y, float z, float radius)
	{
		float radiusSquared = radius * radius;
		return getBinClientObjectList(other, x, y, z, radiusSquared);
	}

	public ArrayList getAllObjectsInLocality(float x, float y, float z, float radius)
	{
		bool flag = false;
		if (x + radius < originx || y + radius < originy || z + radius < originz || x - radius >= originx + sizex || y - radius >= originy + sizey || z - radius >= originz + sizez)
		{
			return getAllOutsideObjects(x, y, z, radius);
		}
		ArrayList arrayList = new ArrayList();
		int num = (int)((x - radius - originx) / sizex * (float)divx);
		int num2 = (int)((y - radius - originy) / sizey * (float)divy);
		int num3 = (int)((z - radius - originz) / sizez * (float)divz);
		int num4 = (int)((x + radius - originx) / sizex * (float)divx);
		int num5 = (int)((y + radius - originy) / sizey * (float)divy);
		int num6 = (int)((z + radius - originz) / sizez * (float)divz);
		if (num < 0)
		{
			flag = true;
			num = 0;
		}
		if (num2 < 0)
		{
			flag = true;
			num2 = 0;
		}
		if (num3 < 0)
		{
			flag = true;
			num3 = 0;
		}
		if (num4 >= divx)
		{
			flag = true;
			num4 = divx - 1;
		}
		if (num5 >= divy)
		{
			flag = true;
			num5 = divy - 1;
		}
		if (num6 >= divz)
		{
			flag = true;
			num6 = divz - 1;
		}
		if (flag)
		{
			arrayList.AddRange(getAllOutsideObjects(x, y, z, radius));
		}
		arrayList.AddRange(getAllClientObjectsInLocalityClipped(x, y, z, radius, num, num2, num3, num4, num5, num6));
		return arrayList;
	}

	public lqClientProxy lqFindNearestNeighborWithinRadius(float x, float y, float z, float radius, object ignoreObject)
	{
		float num = float.MaxValue;
		ArrayList allObjectsInLocality = getAllObjectsInLocality(x, y, z, radius);
		lqClientProxy result = null;
		for (int i = 0; i < allObjectsInLocality.Count; i++)
		{
			lqClientProxy lqClientProxy2 = (lqClientProxy)allObjectsInLocality[i];
			if (lqClientProxy2 != ignoreObject)
			{
				float num2 = lqClientProxy2.x - x;
				float num3 = lqClientProxy2.y - y;
				float num4 = lqClientProxy2.z - z;
				float num5 = num2 * num2 + num3 * num3 + num4 * num4;
				if (num5 < num)
				{
					result = lqClientProxy2;
					num = num5;
				}
			}
		}
		return result;
	}

	public ArrayList getAllObjects()
	{
		int num = divx * divy * divz;
		ArrayList arrayList = new ArrayList();
		for (int i = 0; i < num; i++)
		{
			arrayList.AddRange(bins[i].clientList);
		}
		arrayList.AddRange(other.clientList);
		return arrayList;
	}

	public void lqRemoveAllObjectsInBin(lqBin bin)
	{
		bin.clientList.Clear();
	}

	public void lqRemoveAllObjects()
	{
		int num = divx * divy * divz;
		for (int i = 0; i < num; i++)
		{
			lqRemoveAllObjectsInBin(bins[i]);
		}
		lqRemoveAllObjectsInBin(other);
	}

	public Vector3 getMostPopulatedBinCenter()
	{
		return getMostPopulatedBin()?.center ?? Vector3.zero;
	}

	public lqBin getMostPopulatedBin()
	{
		int num = divx * divy * divz;
		int num2 = 0;
		lqBin result = null;
		for (int i = 0; i < num; i++)
		{
			if (bins[i].clientList.Count > num2)
			{
				num2 = bins[i].clientList.Count;
				result = bins[i];
			}
		}
		return result;
	}
}
