using System;
using System.Collections.Generic;
using Mono.Data.SqliteClient;

public class CameraRepository
{
	public static Dictionary<int, CameraPlot> m_CameraMap = new Dictionary<int, CameraPlot>();

	public static CameraPlot GetCameraPlotData(int id)
	{
		if (!m_CameraMap.ContainsKey(id))
		{
			return null;
		}
		return m_CameraMap[id];
	}

	public static void LoadCameraPlot()
	{
		SqliteDataReader sqliteDataReader = LocalDatabase.Instance.ReadFullTable("Camera");
		sqliteDataReader.Read();
		while (sqliteDataReader.Read())
		{
			CameraPlot cameraPlot = new CameraPlot();
			cameraPlot.m_ID = Convert.ToInt32(sqliteDataReader.GetString(sqliteDataReader.GetOrdinal("ID")));
			cameraPlot.m_ControlType = Convert.ToInt32(sqliteDataReader.GetString(sqliteDataReader.GetOrdinal("Type")));
			string @string = sqliteDataReader.GetString(sqliteDataReader.GetOrdinal("CamPos"));
			string[] array = @string.Split('_');
			if (array.Length == 3)
			{
				cameraPlot.m_CamMove.type = Convert.ToInt32(array[0]);
				string[] array2 = array[1].Split(',');
				if (array2.Length == 3)
				{
					cameraPlot.m_CamMove.pos.x = Convert.ToSingle(array2[0]);
					cameraPlot.m_CamMove.pos.y = Convert.ToSingle(array2[1]);
					cameraPlot.m_CamMove.pos.z = Convert.ToSingle(array2[2]);
				}
				else
				{
					cameraPlot.m_CamMove.npcid = Convert.ToInt32(array[1]);
				}
				cameraPlot.m_CamMove.dirType = Convert.ToInt32(array[2]);
			}
			@string = sqliteDataReader.GetString(sqliteDataReader.GetOrdinal("CamRotation"));
			array = @string.Split('_');
			if (array.Length == 4)
			{
				cameraPlot.m_CamRot.type = Convert.ToInt32(array[0]);
				cameraPlot.m_CamRot.type1 = Convert.ToInt32(array[1]);
				cameraPlot.m_CamRot.angleY = Convert.ToSingle(array[2]);
				cameraPlot.m_CamRot.dirType = Convert.ToInt32(array[3]);
			}
			else if (array.Length == 5)
			{
				cameraPlot.m_CamRot.type = Convert.ToInt32(array[0]);
				cameraPlot.m_CamRot.type1 = Convert.ToInt32(array[1]);
				cameraPlot.m_CamRot.angleY = Convert.ToSingle(array[2]);
				cameraPlot.m_CamRot.angleX = Convert.ToSingle(array[3]);
				cameraPlot.m_CamRot.dirType = Convert.ToInt32(array[4]);
			}
			@string = sqliteDataReader.GetString(sqliteDataReader.GetOrdinal("CamFollow"));
			array = @string.Split('_');
			if (array.Length == 2)
			{
				cameraPlot.m_CamTrack.npcid = Convert.ToInt32(array[0]);
				cameraPlot.m_CamTrack.type = Convert.ToInt32(array[1]);
			}
			cameraPlot.m_CamToPlayer = Convert.ToInt32(sqliteDataReader.GetString(sqliteDataReader.GetOrdinal("CamtoPlayer"))) > 0;
			cameraPlot.m_Delay = Convert.ToInt32(sqliteDataReader.GetString(sqliteDataReader.GetOrdinal("Delay")));
			m_CameraMap.Add(cameraPlot.m_ID, cameraPlot);
		}
	}
}
