using System;
using System.Collections.Generic;
using UnityEngine;

namespace WhiteCat;

public sealed class CreationController : MonoBehaviour, ICloneModelHelper
{
	[SerializeField]
	private Transform _partRoot;

	[SerializeField]
	private Transform _meshRoot;

	[SerializeField]
	private Transform _decalRoot;

	[SerializeField]
	private Transform _effectRoot;

	[SerializeField]
	private Transform _centerObject;

	[SerializeField]
	private bool _visible = true;

	[SerializeField]
	private bool _collidable = true;

	[SerializeField]
	private int _creationID = -1;

	private CreationData _creationData;

	[SerializeField]
	private EVCCategory _category;

	[SerializeField]
	private Bounds _bounds;

	[SerializeField]
	private float _robotRadius;

	[SerializeField]
	private float _buildTime = -1f;

	[SerializeField]
	private bool _buildFinished;

	private Action _onBuildFinish;

	public int armorBoneIndex => creationData.m_IsoData.m_Components[0].m_ExtendData;

	public Transform partRoot => _partRoot;

	public Transform meshRoot => _meshRoot;

	public Transform decalRoot => _decalRoot;

	public Transform effectRoot => _effectRoot;

	public Transform centerObject => _centerObject;

	public bool visible
	{
		get
		{
			return _visible;
		}
		set
		{
			if (_visible == value)
			{
				return;
			}
			_visible = value;
			if (value)
			{
				for (int i = 0; i < _partRoot.childCount; i++)
				{
					VCPart component = _partRoot.GetChild(i).GetComponent<VCPart>();
					if (!component)
					{
						continue;
					}
					Renderer[] componentsInChildren = component.GetComponentsInChildren<Renderer>(includeInactive: true);
					Renderer[] array = componentsInChildren;
					foreach (Renderer renderer in array)
					{
						if (renderer is TrailRenderer || renderer is ParticleRenderer || renderer is ParticleSystemRenderer || renderer is LineRenderer || renderer is SpriteRenderer)
						{
							renderer.enabled = true;
						}
						else
						{
							renderer.enabled = !component.hiddenModel;
						}
					}
				}
			}
			else
			{
				Renderer[] componentsInChildren2 = _partRoot.GetComponentsInChildren<Renderer>();
				for (int k = 0; k < componentsInChildren2.Length; k++)
				{
					componentsInChildren2[k].enabled = false;
				}
			}
			for (int l = 0; l < _meshRoot.childCount; l++)
			{
				_meshRoot.GetChild(l).GetComponent<MeshRenderer>().enabled = value;
			}
			_decalRoot.gameObject.SetActive(value);
			_effectRoot.gameObject.SetActive(value);
		}
	}

	public bool collidable
	{
		get
		{
			return _collidable;
		}
		set
		{
			if (_collidable != value)
			{
				_collidable = value;
				Collider[] componentsInChildren = GetComponentsInChildren<Collider>(includeInactive: true);
				for (int i = 0; i < componentsInChildren.Length; i++)
				{
					componentsInChildren[i].enabled = value;
				}
			}
		}
	}

	public CreationData creationData
	{
		get
		{
			if (_creationData == null && _creationID >= 0)
			{
				_creationData = CreationMgr.GetCreation(_creationID);
			}
			return _creationData;
		}
	}

	public EVCCategory category => _category;

	public Bounds bounds => _bounds;

	public float BoundsRadius => _bounds.extents.magnitude;

	public float robotRadius => _robotRadius;

	public Vector3 boundsCenterInWorld => base.transform.TransformPoint(_bounds.center);

	public bool isBuildFinished => _buildFinished;

	public event Action onUpdate;

	void ICloneModelHelper.ResetView()
	{
		visible = true;
	}

	public void Init(Transform partRoot, Transform meshRoot, Transform decalRoot, Transform effectRoot, CreationData creationData)
	{
		_partRoot = partRoot;
		_meshRoot = meshRoot;
		_decalRoot = decalRoot;
		_effectRoot = effectRoot;
		_creationData = creationData;
		_creationID = ((_creationData != null) ? _creationData.m_ObjectID : (-1));
		_category = creationData.m_IsoData.m_HeadInfo.Category;
		CalcVoxelsBounds();
		ExtendPartsBounds();
		_bounds.Expand(0.05f);
		if (_category == EVCCategory.cgRobot)
		{
			Transform child = base.transform.GetChild(0);
			child.SetParent(null, worldPositionStays: true);
			base.transform.position = boundsCenterInWorld;
			child.SetParent(base.transform, worldPositionStays: true);
			_bounds.center = Vector3.zero;
			float num = _bounds.size.x;
			if (_bounds.size.y > num)
			{
				num = _bounds.size.y;
			}
			if (_bounds.size.z > num)
			{
				num = _bounds.size.z;
			}
			_robotRadius = Mathf.Clamp(num * 0.5f - 0.08f, 0.1f, 0.3f);
			base.gameObject.AddComponent<SphereCollider>().radius = _robotRadius;
		}
		_centerObject = new GameObject("Center Object").transform;
		_centerObject.SetParent(base.transform, worldPositionStays: false);
		_centerObject.localPosition = _bounds.center;
		_centerObject.gameObject.layer = VCConfig.s_ProductLayer;
	}

	public Vector3 ClosestWorldPoint(Vector3 point)
	{
		return base.transform.TransformPoint(_bounds.ClosestPoint(base.transform.InverseTransformPoint(point)));
	}

	public void AddBuildFinishedListener(Action action)
	{
		if (!_buildFinished)
		{
			_onBuildFinish = (Action)Delegate.Combine(_onBuildFinish, action);
		}
		else
		{
			action?.Invoke();
		}
	}

	public void RemoveBuildFinishedListener(Action action)
	{
		if (_onBuildFinish != null)
		{
			_onBuildFinish = (Action)Delegate.Remove(_onBuildFinish, action);
		}
	}

	private void CalcVoxelsBounds()
	{
		IntV3 intV = new IntV3(int.MaxValue, int.MaxValue, int.MaxValue);
		IntV3 intV2 = new IntV3(int.MinValue, int.MinValue, int.MinValue);
		IntV3 intV3 = default(IntV3);
		foreach (KeyValuePair<int, VCVoxel> voxel in creationData.m_IsoData.m_Voxels)
		{
			if (voxel.Value.Volume > 0)
			{
				intV3.x = voxel.Key & 0x3FF;
				intV3.y = voxel.Key >> 20;
				intV3.z = (voxel.Key >> 10) & 0x3FF;
				intV.x = Mathf.Min(intV.x, intV3.x);
				intV.y = Mathf.Min(intV.y, intV3.y);
				intV.z = Mathf.Min(intV.z, intV3.z);
				intV2.x = Mathf.Max(intV2.x, intV3.x);
				intV2.y = Mathf.Max(intV2.y, intV3.y);
				intV2.z = Mathf.Max(intV2.z, intV3.z);
			}
		}
		float voxelSize = creationData.m_IsoData.m_HeadInfo.FindSceneSetting().m_VoxelSize;
		Vector3 vector = new Vector3((float)intV.x * voxelSize, (float)intV.y * voxelSize, (float)intV.z * voxelSize);
		Vector3 vector2 = new Vector3((float)intV2.x * voxelSize, (float)intV2.y * voxelSize, (float)intV2.z * voxelSize);
		Vector3 localPosition = base.transform.GetChild(0).localPosition;
		_bounds.SetMinMax(vector + localPosition, vector2 + localPosition);
	}

	private void ExtendPartsBounds()
	{
		MeshRenderer[] componentsInChildren = GetComponentsInChildren<MeshRenderer>(includeInactive: true);
		if (componentsInChildren != null && componentsInChildren.Length > 0)
		{
			Quaternion localRotation = base.transform.localRotation;
			Vector3 position = new Vector3(float.MaxValue, float.MaxValue, float.MaxValue);
			Vector3 position2 = new Vector3(float.MinValue, float.MinValue, float.MinValue);
			Vector3 min;
			Vector3 max;
			foreach (MeshRenderer meshRenderer in componentsInChildren)
			{
				min = meshRenderer.bounds.min;
				max = meshRenderer.bounds.max;
				position.x = Mathf.Min(min.x, position.x);
				position.y = Mathf.Min(min.y, position.y);
				position.z = Mathf.Min(min.z, position.z);
				position2.x = Mathf.Max(max.x, position2.x);
				position2.y = Mathf.Max(max.y, position2.y);
				position2.z = Mathf.Max(max.z, position2.z);
			}
			position = base.transform.InverseTransformPoint(position);
			position2 = base.transform.InverseTransformPoint(position2);
			min = _bounds.min;
			max = _bounds.max;
			position.x = Mathf.Min(min.x, position.x);
			position.y = Mathf.Min(min.y, position.y);
			position.z = Mathf.Min(min.z, position.z);
			position2.x = Mathf.Max(max.x, position2.x);
			position2.y = Mathf.Max(max.y, position2.y);
			position2.z = Mathf.Max(max.z, position2.z);
			_bounds.SetMinMax(position, position2);
			base.transform.localRotation = localRotation;
		}
	}

	public void OnNewMeshBuild(MeshFilter mf)
	{
		_buildTime = -1f;
		mf.GetComponent<MeshRenderer>().enabled = _visible;
		Collider component = mf.GetComponent<Collider>();
		if ((bool)component)
		{
			component.enabled = _collidable;
		}
	}

	private void UpdateBuild()
	{
		if (_buildTime < -4.5f)
		{
			_buildTime = 0.6f;
			return;
		}
		if (_buildTime < 0f)
		{
			_buildTime -= 1f;
			return;
		}
		_buildTime -= Time.unscaledDeltaTime;
		if (!(_buildTime < 0f))
		{
			return;
		}
		_buildFinished = true;
		if (_onBuildFinish != null)
		{
			try
			{
				_onBuildFinish();
			}
			catch
			{
			}
			_onBuildFinish = null;
		}
	}

	private void Update()
	{
		if (!_buildFinished)
		{
			UpdateBuild();
		}
		if (this.onUpdate != null)
		{
			try
			{
				this.onUpdate();
			}
			catch
			{
			}
		}
	}
}
