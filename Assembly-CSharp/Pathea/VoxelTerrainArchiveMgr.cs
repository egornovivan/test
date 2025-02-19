namespace Pathea;

public class VoxelTerrainArchiveMgr : MonoLikeSingleton<VoxelTerrainArchiveMgr>, ISerializable
{
	private const string VoxelArchiveFileName = "voxel";

	private const string WaterArchiveFileName = "water";

	private const string BlockArchiveFileName = "block45";

	public const string Bloc45kArchiveKey = "ArchiveKeyBlock45";

	void ISerializable.Serialize(PeRecordWriter w)
	{
		if (w.key == "ArchiveKeyVoxelTerrain")
		{
			VFVoxelTerrain.self.SaveLoad.Export(w);
		}
		else if (w.key == "ArchiveKeyVoxelWater")
		{
			VFVoxelWater.self.SaveLoad.Export(w);
		}
		else if (w.key == "ArchiveKeyBlock45")
		{
			Block45Man.self.Export(w);
		}
	}

	protected override void OnInit()
	{
		base.OnInit();
		PeSingleton<ArchiveMgr>.Instance.Register("ArchiveKeyVoxelTerrain", this, yird: true, "voxel", saveFlagResetValue: false);
		PeSingleton<ArchiveMgr>.Instance.Register("ArchiveKeyVoxelWater", this, yird: true, "water", saveFlagResetValue: false);
		PeSingleton<ArchiveMgr>.Instance.Register("ArchiveKeyBlock45", this, yird: true, "block45");
	}

	public void Restore()
	{
		PeRecordReader reader = PeSingleton<ArchiveMgr>.Instance.GetReader("ArchiveKeyVoxelTerrain");
		VFVoxelTerrain.self.Import(reader);
		reader = PeSingleton<ArchiveMgr>.Instance.GetReader("ArchiveKeyVoxelWater");
		VFVoxelWater.self.Import(reader);
		reader = PeSingleton<ArchiveMgr>.Instance.GetReader("ArchiveKeyBlock45");
		Block45Man.self.Import(reader);
	}

	public void New()
	{
		VFVoxelTerrain.self.Import(null);
		VFVoxelWater.self.Import(null);
		Block45Man.self.Import(null);
	}
}
