using System;
using System.Collections.Generic;
using System.IO;
using Pathea;
using PeEvent;
using PETools;
using uLink;
using UnityEngine;

namespace PeMap;

public class MaskTile
{
	public class Mgr : ArchivableSingleton<Mgr>
	{
		public class Args : EventArg
		{
			public bool add;

			public int index;
		}

		public const int mLenthPerArea = 128;

		private Dictionary<int, MaskTile> maskTiles = new Dictionary<int, MaskTile>();

		public Event<Args> eventor = new Event<Args>();

		public int mHalfLength => mLength / 2;

		public int mLength => Convert.ToInt32(Mathf.Max(RandomMapConfig.Instance.MapSize.x, RandomMapConfig.Instance.MapSize.y));

		public int mHalfPerSide
		{
			get
			{
				int num = mHalfLength / 128;
				return (mHalfLength % 128 != 0) ? (num + 1) : num;
			}
		}

		public int mNumPerSide => mHalfPerSide * 2;

		public int GetMapIndex(Vector3 pos)
		{
			int num = Mathf.FloorToInt(pos.x / 128f) + mHalfPerSide;
			int num2 = Mathf.FloorToInt(pos.z / 128f) + mHalfPerSide;
			return num + num2 * mNumPerSide;
		}

		public bool GetIsKnowByPos(Vector3 pos)
		{
			int mapIndex = GetMapIndex(pos);
			if (maskTiles != null && maskTiles.ContainsKey(mapIndex))
			{
				return true;
			}
			return false;
		}

		public Vector2 GetCenterPos(int index)
		{
			Vector2 zero = Vector2.zero;
			zero.x = ((float)(index % mNumPerSide - mHalfPerSide) + 0.5f) * 128f;
			zero.y = ((float)(index / mNumPerSide - mHalfPerSide) + 0.5f) * 128f;
			return zero;
		}

		public List<int> GetNeighborIndex(int centerIndex)
		{
			int num = centerIndex % mNumPerSide;
			int num2 = centerIndex / mNumPerSide;
			List<int> list = new List<int>();
			for (int i = num - 1; i <= num + 1; i++)
			{
				for (int j = num2 - 1; j <= num2 + 1; j++)
				{
					if (i >= 0 && i < mNumPerSide && j >= 0 && j < mNumPerSide)
					{
						int item = i + j * mNumPerSide;
						list.Add(item);
					}
				}
			}
			return list;
		}

		public MaskTile Get(int index)
		{
			if (maskTiles.ContainsKey(index))
			{
				return maskTiles[index];
			}
			return null;
		}

		public void Add(int index, MaskTile tile)
		{
			maskTiles[index] = tile;
			Args args = new Args();
			args.index = index;
			args.add = true;
			eventor.Dispatch(args, this);
		}

		public bool Remove(int index)
		{
			Args args = new Args();
			args.index = index;
			args.add = false;
			eventor.Dispatch(args, this);
			return maskTiles.Remove(index);
		}

		public void Index()
		{
		}

		public void Tick(Vector3 pos)
		{
			List<int> neighborIndex = GetNeighborIndex(GetMapIndex(pos));
			foreach (int item in neighborIndex)
			{
				MaskTile maskTile = Get(item);
				if (maskTile == null)
				{
					Vector2 centerPos = GetCenterPos(item);
					byte type = GetType((int)centerPos.x, (int)centerPos.y);
					if (!PeGameMgr.IsMulti)
					{
						maskTile = new MaskTile();
						maskTile.index = item;
						maskTile.type = type;
						Add(item, maskTile);
					}
				}
			}
		}

		public byte GetType(int pos_x, int pos_z)
		{
			if (VFDataRTGen.IsSea(pos_x, pos_z))
			{
				return 9;
			}
			return (byte)VFDataRTGen.GetXZMapType(pos_x, pos_z);
		}

		protected override void WriteData(BinaryWriter w)
		{
			w.Write(maskTiles.Count);
			foreach (KeyValuePair<int, MaskTile> maskTile in maskTiles)
			{
				PETools.Serialize.WriteBytes(maskTile.Value.Serialize(), w);
			}
		}

		protected override void SetData(byte[] data)
		{
			PETools.Serialize.Import(data, delegate(BinaryReader r)
			{
				int num = r.ReadInt32();
				for (int i = 0; i < num; i++)
				{
					byte[] array = PETools.Serialize.ReadBytes(r);
					if (array != null && array.Length > 0)
					{
						MaskTile maskTile = new MaskTile();
						maskTile.Deserialize(array);
						Add(maskTile.index, maskTile);
					}
				}
			});
		}

		protected override bool GetYird()
		{
			return false;
		}
	}

	public int index;

	public int forceGroup;

	public byte type;

	public MaskTile()
	{
		index = -1;
		forceGroup = -1;
		type = byte.MaxValue;
	}

	public static void SerializeTile(uLink.BitStream stream, object obj, params object[] codecOptions)
	{
		MaskTile maskTile = (MaskTile)obj;
		stream.Write(maskTile.index);
		stream.Write(maskTile.forceGroup);
		stream.Write(maskTile.type);
	}

	public static object DeserializeTile(uLink.BitStream stream, params object[] codecOptions)
	{
		MaskTile maskTile = new MaskTile();
		maskTile.index = stream.Read<int>(new object[0]);
		maskTile.forceGroup = stream.Read<int>(new object[0]);
		maskTile.type = stream.Read<byte>(new object[0]);
		return maskTile;
	}

	public byte[] Serialize()
	{
		return PETools.Serialize.Export(delegate(BinaryWriter w)
		{
			w.Write(index);
			w.Write(forceGroup);
			w.Write(type);
		});
	}

	public void Deserialize(byte[] data)
	{
		PETools.Serialize.Import(data, delegate(BinaryReader r)
		{
			index = r.ReadInt32();
			forceGroup = r.ReadInt32();
			type = r.ReadByte();
		});
	}
}
