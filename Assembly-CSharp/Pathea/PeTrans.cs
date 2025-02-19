using System.IO;
using PETools;
using UnityEngine;

namespace Pathea;

public class PeTrans : PeCmpt, IPeMsg
{
	private const int VERSION_0 = 0;

	private const int VERSION_1 = 1;

	private const int CURRENT_VERSION = 1;

	private Transform mCamAnchor;

	private Transform mTrans;

	private Transform mModel;

	private Bounds mLocalBounds;

	private Bounds mLocalBoundsExtend;

	private bool mFastTravel;

	private Vector3 mFastTravelPos;

	private Vector3 mSpawnPosition;

	private Vector3 mSpawnForward;

	private float m_Radius;

	public Vector3 position
	{
		get
		{
			if (null == mTrans)
			{
				return Vector3.zero;
			}
			return mTrans.position;
		}
		set
		{
			if (value.x < -9999999f || value.x > 9999999f || value.y < -9999999f || value.y > 9999999f || value.z < -9999999f || value.z > 9999999f)
			{
				Debug.LogError("[ERROR]Try to set wrong pos[" + value.x + "," + value.y + "," + value.z + "] to entity " + base.name);
			}
			else
			{
				int num = 0;
				if (base.Entity.Id == 9016 || base.Entity.Id == 9017 || base.Entity.Id == 9205)
				{
					num++;
				}
				mTrans.position = value;
				if (mModel != null)
				{
					mModel.position = value;
				}
			}
		}
	}

	public Vector3 forwardBottom
	{
		get
		{
			Vector3 vector = new Vector3(mLocalBounds.center.x, 0f, mLocalBounds.center.z);
			vector += mLocalBounds.extents.z * Vector3.forward;
			return trans.TransformPoint(vector);
		}
	}

	public Vector3 forwardUp
	{
		get
		{
			Vector3 vector = new Vector3(mLocalBounds.center.x, 0f, mLocalBounds.center.z);
			vector += mLocalBounds.extents.z * Vector3.forward;
			vector += mLocalBounds.size.y * Vector3.up;
			return trans.TransformPoint(vector);
		}
	}

	public Vector3 forwardCenter
	{
		get
		{
			Vector3 vector = new Vector3(mLocalBounds.center.x, 0f, mLocalBounds.center.z);
			vector += mLocalBounds.extents.z * Vector3.forward;
			vector += mLocalBounds.extents.y * Vector3.up;
			return trans.TransformPoint(vector);
		}
	}

	public Vector3 centerUp
	{
		get
		{
			Vector3 vector = new Vector3(mLocalBounds.center.x, 0f, mLocalBounds.center.z);
			vector += mLocalBounds.size.y * Vector3.up;
			return trans.TransformPoint(vector);
		}
	}

	public Vector3 uiHeadTop
	{
		get
		{
			Vector3 vector = mLocalBounds.center;
			vector.y += mLocalBounds.size.y * 0.5f;
			return trans.TransformPoint(vector);
		}
	}

	public Vector3 center => mTrans.TransformPoint(mLocalBounds.center);

	public Vector3 scale
	{
		get
		{
			return existent.lossyScale;
		}
		set
		{
			existent.localScale = value;
		}
	}

	public Quaternion rotation
	{
		get
		{
			return existent.rotation;
		}
		set
		{
			existent.rotation = value;
		}
	}

	public Vector3 headTop
	{
		get
		{
			if (mModel == null)
			{
				return existent.position + existent.up * 1.8f;
			}
			return existent.position + existent.up * mLocalBounds.size.y + new Vector3(0f, 0.5f, 0f);
		}
	}

	public Vector3 forward => existent.forward;

	public Transform existent
	{
		get
		{
			if (mModel != null)
			{
				return mModel;
			}
			if (null != mTrans)
			{
				return mTrans;
			}
			return base.transform;
		}
	}

	public Transform realTrans => mModel;

	public Transform trans => mTrans;

	public Transform camAnchor
	{
		get
		{
			if (mCamAnchor == null)
			{
				mCamAnchor = new GameObject("CamAnchor").transform;
				mCamAnchor.parent = trans;
				PEUtil.ResetTransform(mCamAnchor);
				mCamAnchor.position = headTop;
			}
			return mCamAnchor;
		}
	}

	public Bounds bound
	{
		get
		{
			return mLocalBounds;
		}
		set
		{
			mLocalBounds = value;
		}
	}

	public Bounds boundExtend => mLocalBoundsExtend;

	public Vector3 spawnPosition => mSpawnPosition;

	public Vector3 spawnForward => mSpawnForward;

	public Vector3 fastTravelPos
	{
		get
		{
			return mFastTravelPos;
		}
		set
		{
			mFastTravel = true;
			mFastTravelPos = value;
		}
	}

	public bool fastTravel => mFastTravel;

	public float radius => m_Radius;

	void IPeMsg.OnMsg(EMsg msg, params object[] args)
	{
		switch (msg)
		{
		case EMsg.View_Model_Build:
		{
			GameObject gameObject = args[0] as GameObject;
			if (null != gameObject)
			{
				SetModel(gameObject.transform);
			}
			break;
		}
		case EMsg.View_Prefab_Destroy:
			ResetModel();
			break;
		}
	}

	public override void Awake()
	{
		base.Awake();
		if (Application.isPlaying)
		{
			Create();
		}
	}

	private void InitBound()
	{
		if (mModel != null)
		{
			mLocalBounds = PEUtil.GetLocalColliderBoundsInChildren(mModel.gameObject);
			mLocalBoundsExtend = new Bounds(mLocalBounds.center, mLocalBounds.size);
			mLocalBoundsExtend.Expand(3f);
			m_Radius = Mathf.Max(mLocalBounds.extents.x, mLocalBounds.extents.z);
		}
	}

	private void Create()
	{
		mTrans = new GameObject("DummyTransform").transform;
		mTrans.parent = base.transform;
		PEUtil.ResetTransform(mTrans);
	}

	public override void OnUpdate()
	{
		base.OnUpdate();
		if (null != mModel)
		{
			mTrans.position = mModel.position;
			mTrans.localScale = mModel.localScale;
			mTrans.rotation = mModel.rotation;
			if (mSpawnPosition == Vector3.zero)
			{
				mSpawnPosition = mTrans.position;
			}
			if (mSpawnForward == Vector3.zero)
			{
				mSpawnForward = mTrans.forward;
			}
		}
	}

	public bool InsideBody(Vector3 position)
	{
		Bounds bounds = default(Bounds);
		bounds.center = mLocalBounds.center;
		bounds.size = new Vector3(mLocalBounds.size.x * 0.8f, mLocalBounds.size.y, mLocalBounds.size.z * 0.8f);
		Vector3 point = trans.InverseTransformPoint(position);
		return bounds.Contains(point);
	}

	public override void Deserialize(BinaryReader r)
	{
		int num = r.ReadInt32();
		if (num > 1)
		{
			Debug.LogError("version error");
			return;
		}
		position = PETools.Serialize.ReadVector3(r);
		rotation = PETools.Serialize.ReadQuaternion(r);
		switch (num)
		{
		case 0:
			PETools.Serialize.ReadVector3(r);
			break;
		case 1:
			mFastTravel = r.ReadBoolean();
			break;
		}
		mFastTravelPos = PETools.Serialize.ReadVector3(r);
		if (mFastTravel)
		{
			position = mFastTravelPos;
			mFastTravel = false;
		}
	}

	public override void Serialize(BinaryWriter w)
	{
		w.Write(1);
		PETools.Serialize.WriteVector3(w, position);
		PETools.Serialize.WriteQuaternion(w, rotation);
		w.Write(mFastTravel);
		PETools.Serialize.WriteVector3(w, mFastTravelPos);
	}

	public float GetSqrDistanceXZ(Collider collider)
	{
		Vector3 vector = mTrans.TransformPoint(mLocalBounds.center);
		Vector3 vector2 = PEUtil.GetCenter(collider);
		if (Physics.Raycast(vector, vector2 - vector, out var hitInfo, Vector3.Distance(vector, vector2), 1 << collider.gameObject.layer))
		{
			Vector3 point = mTrans.InverseTransformPoint(hitInfo.point);
			return bound.SqrDistance(point);
		}
		return 0f;
	}

	public void ResetModel()
	{
		if (mModel != null)
		{
			mTrans.position = mModel.position;
			mTrans.rotation = mModel.rotation;
			mModel = null;
			base.Entity.SendMsg(EMsg.Trans_Simulator);
		}
	}

	public void SetModel(Transform model)
	{
		if (null != model)
		{
			mModel = model;
			InitBound();
			base.Entity.SendMsg(EMsg.Trans_Real);
		}
	}

	private void OnDrawGizmosSelected()
	{
		PEUtil.DrawBounds(mModel, mLocalBounds, Color.red);
	}
}
