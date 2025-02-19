using System;
using System.Collections.Generic;
using Mono.Data.SqliteClient;
using UnityEngine;

public class CloudManager : MonoBehaviour
{
	private const float CheckDt = 5f;

	private const float ShowCloudDis = 2000f;

	public static Dictionary<int, Cloud3D> s_tblCloudList = new Dictionary<int, Cloud3D>();

	public Light mSun;

	private List<int> mUnInstantiateList = new List<int>();

	private List<CloudController> mClouds = new List<CloudController>();

	private void Awake()
	{
		mUnInstantiateList.Clear();
		foreach (int key in s_tblCloudList.Keys)
		{
			mUnInstantiateList.Add(key);
		}
		base.gameObject.SetActive(value: false);
	}

	private void Update()
	{
	}

	private void CreateCloud(Cloud3D cloud)
	{
		UnityEngine.Object original = Resources.Load("Prefab/Cloud/" + cloud.mPerfabName);
		GameObject gameObject = UnityEngine.Object.Instantiate(original) as GameObject;
		CloudController component = gameObject.GetComponent<CloudController>();
		component.transform.parent = base.transform;
		component.InitCloud(mSun, cloud);
		mClouds.Add(component);
	}

	public static void LoadData()
	{
		s_tblCloudList.Clear();
		SqliteDataReader sqliteDataReader = LocalDatabase.Instance.ReadFullTable("Cloud3D");
		while (sqliteDataReader.Read())
		{
			Cloud3D cloud3D = new Cloud3D();
			cloud3D.mCloudType = Convert.ToInt32(sqliteDataReader.GetString(sqliteDataReader.GetOrdinal("Type")));
			string[] array = sqliteDataReader.GetString(sqliteDataReader.GetOrdinal("BaseColor")).Split(',');
			cloud3D.mBaseColor = new Color(Convert.ToSingle(array[0]) / 255f, Convert.ToSingle(array[1]) / 255f, Convert.ToSingle(array[2]) / 255f, Convert.ToSingle(array[3]) / 255f);
			array = sqliteDataReader.GetString(sqliteDataReader.GetOrdinal("Position")).Split(',');
			cloud3D.mPosition = new Vector3(Convert.ToSingle(array[0]), Convert.ToSingle(array[1]), Convert.ToSingle(array[2]));
			cloud3D.mPerfabName = sqliteDataReader.GetString(sqliteDataReader.GetOrdinal("PerfabName"));
			s_tblCloudList[Convert.ToInt32(sqliteDataReader.GetString(sqliteDataReader.GetOrdinal("Id")))] = cloud3D;
		}
	}
}
