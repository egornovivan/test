using System.IO;

public class BSSaveIsoBrush : BSSelectBrush
{
	public string IsoName = "No Name";

	public static string s_IsoPath = "/PlanetExplorers/BuildingIso/";

	public static string s_Ext = ".biso";

	protected override bool AfterSelectionUpdate()
	{
		return true;
	}

	private void SaveFile(string file_path, BSIsoData iso)
	{
		if (!Directory.Exists(file_path))
		{
			Directory.CreateDirectory(file_path);
		}
		file_path = file_path + iso.m_HeadInfo.Name + s_Ext;
		using FileStream output = new FileStream(file_path, FileMode.Create, FileAccess.Write);
		BinaryWriter binaryWriter = new BinaryWriter(output);
		byte[] buffer = iso.Export();
		binaryWriter.Write(buffer);
		binaryWriter.Close();
	}
}
