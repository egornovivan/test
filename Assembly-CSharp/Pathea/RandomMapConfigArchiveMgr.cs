using System.IO;
using PETools;
using UnityEngine;

namespace Pathea;

public class RandomMapConfigArchiveMgr : ArchivableSingleton<RandomMapConfigArchiveMgr>
{
	private const int VERSION_0000 = 0;

	private const int VERSION_0001 = 1;

	private const int VERSION_0002 = 2;

	private const int VERSION_0003 = 3;

	private const int VERSION_0004 = 4;

	private const int VERSION_0005 = 5;

	private const int CURRENT_VERSION = 5;

	protected override void WriteData(BinaryWriter bw)
	{
		Export(bw);
	}

	protected override void SetData(byte[] data)
	{
		Import(data);
		RandomMapConfig.Instance.SetMapParam();
	}

	public override void New()
	{
		base.New();
		RandomMapConfig.Instance.SetMapParam();
	}

	private void Export(BinaryWriter w)
	{
		w.Write(5);
		w.Write(RandomMapConfig.RandSeed);
		w.Write(RandomMapConfig.SeedString);
		w.Write((int)RandomMapConfig.vegetationId);
		w.Write((int)RandomMapConfig.RandomMapID);
		w.Write((int)RandomMapConfig.ScenceClimate);
		w.Write(RandomMapConfig.mapSize);
		w.Write(RandomMapConfig.riverDensity);
		w.Write(RandomMapConfig.riverWidth);
		w.Write(RandomMapConfig.useSkillTree);
		w.Write(RandomMapConfig.TerrainHeight);
		w.Write(RandomMapConfig.plainHeight);
		w.Write(RandomMapConfig.flatness);
		w.Write(RandomMapConfig.bridgeMaxHeight);
		w.Write(RandomMapConfig.mirror);
		w.Write(RandomMapConfig.rotation);
		w.Write(RandomMapConfig.pickedLineIndex);
		w.Write(RandomMapConfig.pickedLevelIndex);
		w.Write(RandomMapConfig.allyCount);
	}

	private void Import(byte[] buffer)
	{
		Serialize.Import(buffer, delegate(BinaryReader r)
		{
			int num = r.ReadInt32();
			if (num > 5)
			{
				Debug.LogError("error version:" + num);
			}
			if (num >= 0)
			{
				RandomMapConfig.RandSeed = r.ReadInt32();
				RandomMapConfig.SeedString = r.ReadString();
				RandomMapConfig.vegetationId = (RandomMapType)r.ReadInt32();
				RandomMapConfig.RandomMapID = (RandomMapType)r.ReadInt32();
				RandomMapConfig.ScenceClimate = (ClimateType)r.ReadInt32();
				RandomMapConfig.mapSize = r.ReadInt32();
				RandomMapConfig.riverDensity = r.ReadInt32();
				RandomMapConfig.riverWidth = r.ReadInt32();
			}
			if (num >= 1)
			{
				RandomMapConfig.useSkillTree = r.ReadBoolean();
			}
			if (num >= 2)
			{
				RandomMapConfig.TerrainHeight = r.ReadInt32();
			}
			if (num >= 3)
			{
				RandomMapConfig.plainHeight = r.ReadInt32();
				RandomMapConfig.flatness = r.ReadInt32();
				RandomMapConfig.bridgeMaxHeight = r.ReadInt32();
			}
			if (num >= 4)
			{
				RandomMapConfig.mirror = r.ReadBoolean();
				RandomMapConfig.rotation = r.ReadInt32();
				RandomMapConfig.pickedLineIndex = r.ReadInt32();
				RandomMapConfig.pickedLevelIndex = r.ReadInt32();
			}
			if (num >= 5)
			{
				RandomMapConfig.allyCount = r.ReadInt32();
			}
		});
	}
}
