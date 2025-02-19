using System;
using System.Collections.Generic;
using Mono.Data.SqliteClient;
using Pathea;
using UnityEngine;

public class SceneAssetsMan : MonoLikeSingleton<SceneAssetsMan>
{
	private List<SceneAssetDesc> _assets = new List<SceneAssetDesc>();

	private GameObject _rootObj;

	public GameObject RootObj
	{
		get
		{
			if (_rootObj == null)
			{
				_rootObj = new GameObject("SceneStaticObjs");
			}
			if (_rootObj.transform.parent == null && SceneMan.self != null)
			{
				_rootObj.transform.parent = SceneMan.self.transform;
			}
			return _rootObj;
		}
	}

	public List<SceneAssetDesc> Assets => _assets;

	public void New()
	{
	}

	public void Restore()
	{
	}

	private void LoadStaticAssetsFromDB()
	{
		List<ISceneObjAgent> list = new List<ISceneObjAgent>();
		_assets.Clear();
		SqliteDataReader sqliteDataReader = LocalDatabase.Instance.ReadFullTable("sceneAssetList");
		while (sqliteDataReader.Read())
		{
			int id = Convert.ToInt32(sqliteDataReader.GetString(0));
			int num = Convert.ToInt32(sqliteDataReader.GetString(1));
			string text = sqliteDataReader.GetString(2);
			string @string = sqliteDataReader.GetString(3);
			string[] array = sqliteDataReader.GetString(4).Split(',');
			string[] array2 = sqliteDataReader.GetString(5).Split(',');
			string[] array3 = sqliteDataReader.GetString(6).Split(',');
			Vector3 pos = new Vector3(Convert.ToSingle(array[0]), Convert.ToSingle(array[1]), Convert.ToSingle(array[2]));
			Quaternion rotation = new Quaternion(Convert.ToSingle(array2[0]), Convert.ToSingle(array2[1]), Convert.ToSingle(array2[2]), Convert.ToSingle(array2[3]));
			if (rotation.w > 2f)
			{
				rotation.eulerAngles = new Vector3(rotation.x, rotation.y, rotation.z);
			}
			Vector3 scale = new Vector3(Convert.ToSingle(array3[0]), Convert.ToSingle(array3[1]), Convert.ToSingle(array3[2]));
			if (text != null && text.Length <= 1)
			{
				text = null;
			}
			switch (num)
			{
			case 0:
			{
				SceneAssetDesc sceneAssetDesc = new SceneAssetDesc();
				sceneAssetDesc._id = id;
				sceneAssetDesc._agent = new SceneStaticAssetAgent(text, @string, pos, rotation, scale);
				_assets.Add(sceneAssetDesc);
				list.Add(sceneAssetDesc._agent);
				break;
			}
			case 1:
			{
				OperatableItemAgent item = new OperatableItemAgent(id, pos, @string);
				list.Add(item);
				break;
			}
			default:
				Debug.LogError("[SceneAssets]:Unrecognizable asset type:" + num);
				break;
			}
		}
		SceneMan.AddSceneObjs(list);
	}

	public void Register(string pathMain, Vector3 position, Quaternion rotation, Vector3 scale, SceneAssetType type = SceneAssetType.StaticAsset)
	{
	}
}
