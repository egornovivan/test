using System;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using UnityEngine;

public class AssetsLoader : MonoBehaviour
{
	private const int MinValidPathLen = 2;

	public const string InvalidAssetPath = "0";

	public const string PrefabExtension = ".prefab";

	public const string AssetBundleExtension = ".unity3d";

	private static AssetsLoader self;

	private Stack<AssetReq> assetStack_Request = new Stack<AssetReq>();

	public static AssetsLoader Instance
	{
		get
		{
			if (self == null)
			{
				GameObject gameObject = new GameObject("AssetsLoader");
				self = gameObject.AddComponent<AssetsLoader>();
				Debug.Log("AssetsLoader Awake end");
			}
			return self;
		}
	}

	private void Start()
	{
		AssetsPool.PreLoad();
		StartCoroutine(ProcessReqs(assetStack_Request, GameConfig.AssetBundlePath));
	}

	private void OnDisable()
	{
		StopAllCoroutines();
		assetStack_Request.Clear();
		AssetsPool.Clear();
	}

	private IEnumerator ProcessReqs(Stack<AssetReq> assetReqList, string assetBundlePath)
	{
		while (true)
		{
			if (assetReqList.Count > 0)
			{
				AssetReq req = assetReqList.Pop();
				if (req.ProcLevel <= AssetReq.ProcLvl.DoNothing)
				{
					continue;
				}
				UnityEngine.Object asset = null;
				string assetPathName = req.PathName;
				string assetPathNameExt = Path.GetExtension(req.PathName);
				bool bAssetBundle = assetPathNameExt.Equals(".unity3d");
				if (!bAssetBundle && !string.IsNullOrEmpty(assetPathNameExt))
				{
					assetPathName = Path.GetDirectoryName(req.PathName) + "/" + Path.GetFileNameWithoutExtension(req.PathName);
				}
				if (!AssetsPool.TryGetAsset(assetPathName, out asset))
				{
					if (bAssetBundle)
					{
						WWW www = WWW.LoadFromCacheOrDownload("file://" + assetBundlePath + assetPathName, AssetsPool.s_Version);
						yield return www;
						if (www.error != null)
						{
							Debug.LogError(www.error);
						}
						else
						{
							AssetBundle assetBundle = www.assetBundle;
							AssetBundleRequest request = assetBundle.LoadAssetAsync(Path.GetFileNameWithoutExtension(assetPathName), typeof(GameObject));
							yield return request;
							if (request != null)
							{
								asset = request.asset;
							}
							request = null;
							assetBundle.Unload(unloadAllLoadedObjects: false);
						}
						if (www != null)
						{
							www.Dispose();
							www = null;
						}
					}
					else
					{
						ResourceRequest resReq = Resources.LoadAsync(assetPathName);
						while (!resReq.isDone)
						{
							yield return null;
						}
						asset = resReq.asset;
					}
					if (asset != null && req.NeedCaching)
					{
						AssetsPool.RegisterAsset(assetPathName, asset);
					}
				}
				if (asset != null && req.ProcLevel >= AssetReq.ProcLvl.DoInstantiation)
				{
					GameObject go = UnityEngine.Object.Instantiate(asset, req.Prs.Position(), req.Prs.Rotation()) as GameObject;
					if (go != null)
					{
						go.transform.localScale = req.Prs.Scale();
						req.OnFinish(go);
					}
				}
			}
			yield return null;
		}
	}

	public void AddReq(AssetReq request)
	{
		if (request.PathName.Length >= 2)
		{
			assetStack_Request.Push(request);
		}
	}

	public AssetReq AddReq(string pathName, Vector3 position, Quaternion rotation, Vector3 scale)
	{
		AssetReq assetReq = new AssetReq(pathName, new AssetPRS(position, rotation, scale));
		AddReq(assetReq);
		return assetReq;
	}

	public UnityEngine.Object LoadPrefabImm(string prefabPathName, bool bIntoCache = false)
	{
		UnityEngine.Object asset = null;
		if (prefabPathName.Length < 2)
		{
			return asset;
		}
		string text = prefabPathName;
		string extension = Path.GetExtension(prefabPathName);
		if (!string.IsNullOrEmpty(extension))
		{
			text = Path.GetDirectoryName(prefabPathName) + "/" + Path.GetFileNameWithoutExtension(prefabPathName);
		}
		if (!AssetsPool.TryGetAsset(text, out asset))
		{
			try
			{
				asset = Resources.Load(text);
			}
			catch (Exception ex)
			{
				Debug.LogError("[AssetsLoader]Failed to load " + text + " with error: " + ex.ToString());
			}
			if (bIntoCache && asset != null)
			{
				AssetsPool.RegisterAsset(text, asset);
			}
		}
		return asset;
	}

	public GameObject InstantiateAssetImm(string pathName, Vector3 position, Quaternion rotation, Vector3 scale, bool bIntoCache = false)
	{
		GameObject gameObject = null;
		if (pathName.Length < 2)
		{
			return gameObject;
		}
		try
		{
			UnityEngine.Object asset = null;
			string text = pathName;
			string extension = Path.GetExtension(pathName);
			bool flag = extension.Equals(".unity3d");
			if (!flag && !string.IsNullOrEmpty(extension))
			{
				text = Path.GetDirectoryName(pathName) + "/" + Path.GetFileNameWithoutExtension(pathName);
			}
			if (!AssetsPool.TryGetAsset(text, out asset))
			{
				if (flag)
				{
					FileStream fileStream = new FileStream(GameConfig.AssetBundlePath + text, FileMode.Open, FileAccess.Read, FileShare.Read);
					BinaryReader binaryReader = new BinaryReader(fileStream);
					byte[] binary = binaryReader.ReadBytes((int)fileStream.Length);
					binaryReader.Close();
					fileStream.Close();
					AssetBundle assetBundle = AssetBundle.CreateFromMemoryImmediate(binary);
					asset = assetBundle.LoadAsset<GameObject>(Path.GetFileNameWithoutExtension(text));
					assetBundle.Unload(unloadAllLoadedObjects: false);
				}
				else
				{
					asset = Resources.Load(text);
				}
				if (asset != null && bIntoCache)
				{
					AssetsPool.RegisterAsset(text, asset);
				}
			}
			if (asset != null)
			{
				gameObject = UnityEngine.Object.Instantiate(asset, position, rotation) as GameObject;
				gameObject.transform.localScale = scale;
			}
		}
		catch (Exception)
		{
			Debug.Log("Error: Faile to LoadAssetImm " + pathName);
		}
		return gameObject;
	}
}
