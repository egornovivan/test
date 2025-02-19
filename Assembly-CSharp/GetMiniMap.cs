using System;
using System.Collections;
using System.IO;
using UnityEngine;

public class GetMiniMap : MonoBehaviour
{
	public bool start;

	public float weightTime = 10f;

	public LayerMask mGroundLayer = LayerMask.GetMask("Water", "VFVoxelTerrain");

	private float worldSize = 18432f;

	private int texSize = 512;

	private int mNumOneside;

	private Texture2D mMinimap;

	private GameObject mCameraObj;

	private Camera mCamera;

	private GameObject mFloowObj;

	private VFVoxelTerrain mTerrain;

	private VoxelEditor mEditor;

	public int mIndex;

	public int mIndexTo = 1297;

	private float mtime;

	private void Start()
	{
		mNumOneside = (int)(worldSize / (float)texSize);
		mMinimap = new Texture2D(texSize, texSize, TextureFormat.RGB24, mipmap: false);
		mTerrain = VFVoxelTerrain.self;
		mEditor = GameObject.Find("Voxel Terrain").GetComponent<VoxelEditor>();
	}

	private IEnumerator waitTime(float time)
	{
		yield return new WaitForSeconds(1f);
	}

	private void Update()
	{
		if (Input.GetKeyDown(KeyCode.G) && !start)
		{
			start = true;
			GameObject gameObject = GameObject.Find("Player");
			if (gameObject != null)
			{
				gameObject.SetActive(value: false);
			}
			mTerrain.saveTerrain = true;
			mFloowObj = new GameObject("mapcenterObj");
			SceneMan.SetObserverTransform(mFloowObj.transform);
			mCamera = Camera.main;
			mCamera.clearFlags = CameraClearFlags.Skybox;
			mCamera.farClipPlane = 2500f;
			mCamera.nearClipPlane = 1f;
			mCamera.orthographic = true;
			mCamera.orthographicSize = 256f;
			mCamera.cullingMask = mGroundLayer.value;
			mCamera.transform.position = new Vector3(texSize / 2 + mIndex % mNumOneside * texSize, 1500f, texSize / 2 + mIndex / mNumOneside * texSize);
			mCamera.transform.rotation = new Quaternion(0.7f, 0f, 0f, 0.7f);
			mFloowObj.transform.position = mCamera.transform.position + 600f * Vector3.down;
			mtime = Time.time;
			if (!Directory.Exists(Application.dataPath + "../../MiniMaps"))
			{
				Directory.CreateDirectory(Application.dataPath + "../../MiniMaps");
			}
		}
	}

	private void OnPostRender()
	{
		if (!start || !(Time.time - mtime > 12f))
		{
			return;
		}
		mtime = Time.time;
		mMinimap.ReadPixels(new Rect((Screen.width - texSize) / 2, 0f, texSize, texSize), 0, 0);
		mMinimap.Apply();
		byte[] bytes = mMinimap.EncodeToPNG();
		File.WriteAllBytes(Application.dataPath + "../../MiniMaps/MiniMap" + mIndex + ".png", bytes);
		if (mIndex < mIndexTo)
		{
			mIndex++;
			mCamera.transform.position = new Vector3(texSize / 2 + mIndex % mNumOneside * texSize, 1000f, texSize / 2 + mIndex / mNumOneside * texSize);
			mFloowObj.transform.position = mCamera.transform.position + 500f * Vector3.down;
			if (mIndex > mIndexTo)
			{
				start = false;
			}
		}
		else
		{
			start = false;
		}
		GC.Collect();
	}
}
