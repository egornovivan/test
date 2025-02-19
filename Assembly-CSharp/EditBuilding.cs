using System.Collections.Generic;
using ItemAsset;
using UnityEngine;

public class EditBuilding : GLBehaviour
{
	public BlockBuilding mBlockBuilding;

	public bool mSelected;

	private bool mDrawGL;

	private Dictionary<IntVector3, B45Block> mBlocks;

	private Block45CurMan mB45Building;

	private Dictionary<AssetReq, CreatItemInfo> mReqList = new Dictionary<AssetReq, CreatItemInfo>();

	public bool DeletEnable => mReqList.Count == 0;

	private void Awake()
	{
		m_Material = new Material(Shader.Find("Lines/Colored Blended"));
		m_Material.hideFlags = HideFlags.HideAndDontSave;
		m_Material.shader.hideFlags = HideFlags.HideAndDontSave;
		GlobalGLs.AddGL(this);
	}

	public void Init(BlockBuilding building, Block45CurMan perfab)
	{
		mBlockBuilding = building;
		mBlockBuilding.GetBuildingInfo(out var size, out mBlocks, out var _, out var itemList, out var _);
		BoxCollider boxCollider = base.gameObject.AddComponent<BoxCollider>();
		boxCollider.center = 0.5f * size + 0.5f * Vector3.up;
		boxCollider.size = size;
		mB45Building = Object.Instantiate(perfab);
		mB45Building.transform.parent = base.transform;
		mB45Building.transform.localPosition = Vector3.zero;
		mB45Building.transform.localRotation = Quaternion.identity;
		mB45Building.transform.localScale = Vector3.one;
		Invoke("BuildBuilding", 0.5f);
		foreach (CreatItemInfo item in itemList)
		{
			ItemProto itemData = ItemProto.GetItemData(item.mItemId);
			GameObject gameObject = Object.Instantiate(Resources.Load(itemData.resourcePath)) as GameObject;
			gameObject.transform.parent = base.transform;
			gameObject.transform.transform.localPosition = item.mPos;
			gameObject.transform.transform.localRotation = item.mRotation;
			gameObject.transform.transform.localScale = Vector3.one;
		}
	}

	private void BuildBuilding()
	{
		foreach (IntVector3 key in mBlocks.Keys)
		{
			mB45Building.DataSource.SafeWrite(mBlocks[key], key.x, key.y, key.z);
		}
	}

	public void OnSpawned(GameObject go, AssetReq req)
	{
		if (mReqList.ContainsKey(req))
		{
			go.transform.parent = base.transform;
			go.transform.transform.localPosition = mReqList[req].mPos;
			go.transform.transform.localRotation = mReqList[req].mRotation;
			go.transform.transform.localScale = Vector3.one;
			mReqList.Remove(req);
		}
		else
		{
			Object.Destroy(go);
		}
	}

	private void OnMouseUpAsButton()
	{
		if (mReqList.Count == 0)
		{
			TownEditor.Instance.OnBuildingSelected(this);
		}
	}

	private void OnMouseDrag()
	{
		TownEditor.Instance.OnBuildingDrag(this);
	}

	private void OnMouseOver()
	{
		mDrawGL = true;
	}

	public override void OnGL()
	{
		if (null != GetComponent<Collider>() && (mDrawGL || mSelected))
		{
			mDrawGL = false;
			Vector3[] array = new Vector3[8]
			{
				new Vector3(GetComponent<Collider>().bounds.min.x, GetComponent<Collider>().bounds.min.y, GetComponent<Collider>().bounds.min.z),
				new Vector3(GetComponent<Collider>().bounds.max.x, GetComponent<Collider>().bounds.min.y, GetComponent<Collider>().bounds.min.z),
				new Vector3(GetComponent<Collider>().bounds.min.x, GetComponent<Collider>().bounds.max.y, GetComponent<Collider>().bounds.min.z),
				new Vector3(GetComponent<Collider>().bounds.max.x, GetComponent<Collider>().bounds.max.y, GetComponent<Collider>().bounds.min.z),
				new Vector3(GetComponent<Collider>().bounds.min.x, GetComponent<Collider>().bounds.min.y, GetComponent<Collider>().bounds.max.z),
				new Vector3(GetComponent<Collider>().bounds.max.x, GetComponent<Collider>().bounds.min.y, GetComponent<Collider>().bounds.max.z),
				new Vector3(GetComponent<Collider>().bounds.min.x, GetComponent<Collider>().bounds.max.y, GetComponent<Collider>().bounds.max.z),
				new Vector3(GetComponent<Collider>().bounds.max.x, GetComponent<Collider>().bounds.max.y, GetComponent<Collider>().bounds.max.z)
			};
			GL.PushMatrix();
			m_Material.SetPass(0);
			GL.Begin(1);
			GL.Color(Color.yellow);
			GL.Vertex3(array[0].x, array[0].y, array[0].z);
			GL.Vertex3(array[1].x, array[1].y, array[1].z);
			GL.Vertex3(array[2].x, array[2].y, array[2].z);
			GL.Vertex3(array[3].x, array[3].y, array[3].z);
			GL.Vertex3(array[4].x, array[4].y, array[4].z);
			GL.Vertex3(array[5].x, array[5].y, array[5].z);
			GL.Vertex3(array[6].x, array[6].y, array[6].z);
			GL.Vertex3(array[7].x, array[7].y, array[7].z);
			GL.Vertex3(array[0].x, array[0].y, array[0].z);
			GL.Vertex3(array[4].x, array[4].y, array[4].z);
			GL.Vertex3(array[1].x, array[1].y, array[1].z);
			GL.Vertex3(array[5].x, array[5].y, array[5].z);
			GL.Vertex3(array[2].x, array[2].y, array[2].z);
			GL.Vertex3(array[6].x, array[6].y, array[6].z);
			GL.Vertex3(array[3].x, array[3].y, array[3].z);
			GL.Vertex3(array[7].x, array[7].y, array[7].z);
			GL.Vertex3(array[0].x, array[0].y, array[0].z);
			GL.Vertex3(array[2].x, array[2].y, array[2].z);
			GL.Vertex3(array[1].x, array[1].y, array[1].z);
			GL.Vertex3(array[3].x, array[3].y, array[3].z);
			GL.Vertex3(array[4].x, array[4].y, array[4].z);
			GL.Vertex3(array[6].x, array[6].y, array[6].z);
			GL.Vertex3(array[5].x, array[5].y, array[5].z);
			GL.Vertex3(array[7].x, array[7].y, array[7].z);
			GL.End();
			GL.Begin(7);
			if (mSelected)
			{
				GL.Color(new Color(0f, 0f, 0.2f, 0.5f));
			}
			else
			{
				GL.Color(new Color(0f, 0.2f, 0f, 0.5f));
			}
			GL.Vertex3(array[0].x, array[0].y, array[0].z);
			GL.Vertex3(array[1].x, array[1].y, array[1].z);
			GL.Vertex3(array[3].x, array[3].y, array[3].z);
			GL.Vertex3(array[2].x, array[2].y, array[2].z);
			GL.Vertex3(array[4].x, array[4].y, array[4].z);
			GL.Vertex3(array[5].x, array[5].y, array[5].z);
			GL.Vertex3(array[7].x, array[7].y, array[7].z);
			GL.Vertex3(array[6].x, array[6].y, array[6].z);
			GL.Vertex3(array[3].x, array[3].y, array[3].z);
			GL.Vertex3(array[2].x, array[2].y, array[2].z);
			GL.Vertex3(array[6].x, array[6].y, array[6].z);
			GL.Vertex3(array[7].x, array[7].y, array[7].z);
			GL.Vertex3(array[0].x, array[0].y, array[0].z);
			GL.Vertex3(array[1].x, array[1].y, array[1].z);
			GL.Vertex3(array[5].x, array[5].y, array[5].z);
			GL.Vertex3(array[4].x, array[4].y, array[4].z);
			GL.Vertex3(array[1].x, array[1].y, array[1].z);
			GL.Vertex3(array[5].x, array[5].y, array[5].z);
			GL.Vertex3(array[7].x, array[7].y, array[7].z);
			GL.Vertex3(array[3].x, array[3].y, array[3].z);
			GL.Vertex3(array[0].x, array[0].y, array[0].z);
			GL.Vertex3(array[4].x, array[4].y, array[4].z);
			GL.Vertex3(array[6].x, array[6].y, array[6].z);
			GL.Vertex3(array[2].x, array[2].y, array[2].z);
			GL.End();
			GL.PopMatrix();
		}
	}
}
