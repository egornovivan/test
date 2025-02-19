using System;
using System.Collections.Generic;
using System.IO;
using Pathea;
using PETools;
using UnityEngine;

public class SceneMan : MonoBehaviour, Pathea.ISerializable
{
	private const string ArchiveKey = "ArchiveKeySceneManager";

	private const int ArchiveVer = 0;

	public const int InvalidID = 0;

	public static SceneMan self = null;

	public static readonly float MaxHitTstHeight = 999f;

	public static readonly LayerMask DependenceLayer = 71680;

	private IdGenerator _idGen = new IdGenerator(1, 1, int.MaxValue);

	private LODOctreeMan _lodMan;

	private List<ISceneObjActivationDependence> _lstActDependences = new List<ISceneObjActivationDependence>();

	private List<ISceneObjAgent> _lstIsActive = new List<ISceneObjAgent>();

	private List<ISceneObjAgent> _lstToActive = new List<ISceneObjAgent>();

	private List<ISceneObjAgent> _lstInactive = new List<ISceneObjAgent>();

	private List<ISceneObjAgent> _lstIdle = new List<ISceneObjAgent>();

	private List<ISceneObjAgent> _toConstruct = new List<ISceneObjAgent>();

	private List<ISceneObjAgent> _toDestruct = new List<ISceneObjAgent>();

	private List<ISceneObjAgent> _toActivate = new List<ISceneObjAgent>();

	private List<ISceneObjAgent> _toDeactivate = new List<ISceneObjAgent>();

	private Rect _curRectActive;

	private Rect _curRectInactive;

	private Rect _curRectInactiveE;

	private Bounds _curBoundsActive;

	private Bounds _curBoundsInactive;

	private Bounds _curBoundsInactiveE;

	private Vector3 _vExtForDelay = new Vector3(48f, 48f, 48f);

	private bool _bActivateFunctional;

	private bool _bMoved;

	private bool _dirtyLstIsActive;

	private bool _dirtyLstInactive;

	private bool _dirtyLstIdle;

	private Vector3 _lastRefreshPos = LODOctreeMan.InitPos;

	private int _sqrObjRefreshThreshold = 1;

	private byte[] _dataToImport;

	private List<long> _lstPosToSerialize;

	private List<string> _lstTypeToSerialize;

	private Transform _playerTrans;

	public static int MaxLod
	{
		get
		{
			return LODOctreeMan._maxLod;
		}
		set
		{
			LODOctreeMan._maxLod = value;
		}
	}

	public static Vector3 LastRefreshPos => (!(self != null) || self._lodMan == null) ? LODOctreeMan.InitPos : self._lodMan.LastRefreshPos;

	public Transform Observer
	{
		get
		{
			return (_lodMan == null) ? null : _lodMan.Observer;
		}
		set
		{
			if (_lodMan != null)
			{
				_lodMan.Observer = value;
			}
		}
	}

	public bool CenterMoved => _bMoved;

	public bool ActivationStarted => _bActivateFunctional;

	void Pathea.ISerializable.Serialize(PeRecordWriter w)
	{
		w.Write(Export());
	}

	public static RaycastHit[] DependenceHitTst(ISceneObjAgent agent)
	{
		float num = ((!agent.TstYOnActivate) ? MaxHitTstHeight : (agent.Pos.y + 1f));
		Vector3 origin = new Vector3(agent.Pos.x, num, agent.Pos.z);
		RaycastHit[] array = Physics.RaycastAll(origin, Vector3.down, num, DependenceLayer);
		return (array != null && array.Length != 0) ? array : null;
	}

	public void StartWork()
	{
		if (!_bActivateFunctional)
		{
			AddImportedObj();
			_bActivateFunctional = true;
		}
	}

	private void Awake()
	{
		self = this;
		Profiler.maxNumberOfSamplesPerFrame = -1;
		PeSingleton<ArchiveMgr>.Instance.Register("ArchiveKeySceneManager", this, yird: true);
	}

	private void Start()
	{
		List<ILODNodeDataMan> list = new List<ILODNodeDataMan>();
		if (VFVoxelTerrain.self != null)
		{
			list.Add(VFVoxelTerrain.self);
			list.Add(VFVoxelWater.self);
		}
		if (Block45Man.self != null)
		{
			list.Add(Block45Man.self);
		}
		_lstActDependences.Add(SceneChunkDependence.Instance);
		_lstActDependences.Add(new SceneStaticObjDependence(_lstInactive));
		LODOctreeMan.ResetRootChunkCount();
		int refreshThreshold = 1;
		if (MaxLod > 3)
		{
			Debug.Log("[LOD]Too many levels");
			MaxLod = 3;
		}
		_lodMan = new LODOctreeMan(list.ToArray(), MaxLod, refreshThreshold, PEUtil.MainCamTransform);
	}

	private void Update()
	{
		if (_playerTrans == null && PeSingleton<PeCreature>.Instance.mainPlayer != null && PeSingleton<PeCreature>.Instance.mainPlayer.peTrans != null)
		{
			_playerTrans = PeSingleton<PeCreature>.Instance.mainPlayer.peTrans.trans;
		}
		if (PeCamera.isFreeLook || _playerTrans == null || PlotLensAnimation.IsPlaying)
		{
			SetObserverTransform(PEUtil.MainCamTransform);
		}
		else
		{
			SetObserverTransform(_playerTrans);
		}
		if (!_lodMan.ReqRefresh())
		{
			return;
		}
		VFGoPool<VFVoxelChunkGo>.SwapReq();
		VFGoPool<Block45ChunkGo>.SwapReq();
		Vector3 lastRefreshPos = _lodMan.LastRefreshPos;
		_bMoved = (lastRefreshPos - _lastRefreshPos).sqrMagnitude > (float)_sqrObjRefreshThreshold;
		if (_bMoved)
		{
			_curBoundsInactive = LODOctreeMan.self._viewBounds;
			if (LODOctreeMan._maxLod == 0)
			{
				_curBoundsInactive.extents *= 1.5f;
			}
			Vector3 min = _curBoundsInactive.min;
			Vector3 size = _curBoundsInactive.size;
			_curRectInactive = new Rect(min.x, min.z, size.x, size.z);
			_curRectInactiveE = new Rect(min.x - _vExtForDelay.x, min.z - _vExtForDelay.z, size.x + _vExtForDelay.x, size.z + _vExtForDelay.z);
			_curBoundsInactiveE = new Bounds(_curBoundsInactive.center, (_curBoundsInactive.extents + _vExtForDelay) * 2f);
			_curBoundsActive = LODOctreeMan.self._Lod0ViewBounds;
			min = _curBoundsActive.min;
			size = _curBoundsActive.size;
			_curRectActive = new Rect(min.x, min.z, size.x, size.z);
			_lastRefreshPos = lastRefreshPos;
		}
		if (_bMoved || _dirtyLstIdle)
		{
			UpdateIdle();
		}
		if (_bMoved || _dirtyLstInactive)
		{
			UpdateInactive();
		}
		if (_bActivateFunctional)
		{
			_dirtyLstIsActive = true;
			UpdateToActive();
			UpdateIsActive();
		}
		VFGoPool<VFVoxelChunkGo>.ExecFreeGo();
		VFGoPool<Block45ChunkGo>.ExecFreeGo();
	}

	private void OnDestroy()
	{
		SceneChunkDependence.Instance.Reset();
		if (_lodMan != null)
		{
			_lodMan.ReqDestroy();
			_lodMan = null;
		}
		self = null;
	}

	public static void SetObserverTransform(Transform trans)
	{
		if (null != self)
		{
			self.Observer = trans;
		}
	}

	public static void AddSceneObj(ISceneObjAgent obj)
	{
		if (!(null == self))
		{
			if (obj.Id == 0)
			{
				obj.Id = self._idGen.Fetch();
			}
			self._lstIdle.Add(obj);
			self._dirtyLstIdle = true;
		}
	}

	public static void AddSceneObjs<T>(List<T> objs) where T : ISceneObjAgent
	{
		if (null == self)
		{
			return;
		}
		int count = objs.Count;
		for (int i = 0; i < count; i++)
		{
			ISceneObjAgent sceneObjAgent = objs[i];
			if (sceneObjAgent.Id == 0)
			{
				sceneObjAgent.Id = self._idGen.Fetch();
			}
			self._lstIdle.Add(sceneObjAgent);
		}
		self._dirtyLstIdle = true;
	}

	public static int RemoveSceneObj(ISceneObjAgent obj)
	{
		if (null == self)
		{
			return 0;
		}
		if (self._lstIsActive.Remove(obj))
		{
			return 4;
		}
		if (self._lstToActive.Remove(obj))
		{
			return 3;
		}
		if (self._lstInactive.Remove(obj))
		{
			return 2;
		}
		if (self._lstIdle.Remove(obj))
		{
			return 1;
		}
		return 0;
	}

	public static int RemoveSceneObjs<T>(List<T> objs) where T : ISceneObjAgent
	{
		if (null == self)
		{
			return 0;
		}
		int num = 0;
		int count = objs.Count;
		for (int i = 0; i < count; i++)
		{
			ISceneObjAgent item = objs[i];
			if (self._lstIsActive.Remove(item) || self._lstToActive.Remove(item) || self._lstInactive.Remove(item) || self._lstIdle.Remove(item))
			{
				num++;
			}
		}
		return num;
	}

	public static bool RemoveSceneObjByGo(GameObject go)
	{
		if (null == self || go == null)
		{
			return false;
		}
		return self._lstIsActive.RemoveAll((ISceneObjAgent it) => it.Go == go) > 0 || self._lstToActive.RemoveAll((ISceneObjAgent it) => it.Go == go) > 0 || self._lstInactive.RemoveAll((ISceneObjAgent it) => it.Go == go) > 0 || self._lstIdle.RemoveAll((ISceneObjAgent it) => it.Go == go) > 0;
	}

	public static ISceneObjAgent GetSceneObjById(int id)
	{
		if (null == self || id == 0)
		{
			return null;
		}
		ISceneObjAgent sceneObjAgent = null;
		object result;
		if ((sceneObjAgent = self._lstIsActive.Find((ISceneObjAgent it) => it.Id == id)) != null || (sceneObjAgent = self._lstToActive.Find((ISceneObjAgent it) => it.Id == id)) != null || (sceneObjAgent = self._lstInactive.Find((ISceneObjAgent it) => it.Id == id)) != null || (sceneObjAgent = self._lstIdle.Find((ISceneObjAgent it) => it.Id == id)) != null)
		{
			ISceneObjAgent sceneObjAgent2 = sceneObjAgent;
			result = sceneObjAgent2;
		}
		else
		{
			result = null;
		}
		return (ISceneObjAgent)result;
	}

	public static ISceneObjAgent GetSceneObjByGo(GameObject go)
	{
		if (null == self || go == null)
		{
			return null;
		}
		ISceneObjAgent sceneObjAgent = null;
		object result;
		if ((sceneObjAgent = self._lstIsActive.Find((ISceneObjAgent it) => it.Go == go)) != null || (sceneObjAgent = self._lstToActive.Find((ISceneObjAgent it) => it.Go == go)) != null || (sceneObjAgent = self._lstInactive.Find((ISceneObjAgent it) => it.Go == go)) != null || (sceneObjAgent = self._lstIdle.Find((ISceneObjAgent it) => it.Go == go)) != null)
		{
			ISceneObjAgent sceneObjAgent2 = sceneObjAgent;
			result = sceneObjAgent2;
		}
		else
		{
			result = null;
		}
		return (ISceneObjAgent)result;
	}

	public static List<ISceneObjAgent> GetSceneObjs<T>() where T : ISceneObjAgent
	{
		if (self == null)
		{
			return null;
		}
		List<ISceneObjAgent> list = new List<ISceneObjAgent>();
		List<ISceneObjAgent> list2 = null;
		list2 = self._lstIsActive.FindAll((ISceneObjAgent item0) => item0.GetType() == typeof(T) || item0.GetType().IsSubclassOf(typeof(T)));
		list.AddRange(list2);
		list2 = self._lstToActive.FindAll((ISceneObjAgent item0) => item0.GetType() == typeof(T) || item0.GetType().IsSubclassOf(typeof(T)));
		list.AddRange(list2);
		list2 = self._lstInactive.FindAll((ISceneObjAgent item0) => item0.GetType() == typeof(T) || item0.GetType().IsSubclassOf(typeof(T)));
		list.AddRange(list2);
		list2 = self._lstIdle.FindAll((ISceneObjAgent item0) => item0.GetType() == typeof(T) || item0.GetType().IsSubclassOf(typeof(T)));
		list.AddRange(list2);
		return list;
	}

	public static List<ISceneObjAgent> GetSceneObjs(Type type)
	{
		if (self == null && !type.IsSubclassOf(typeof(ISceneObjAgent)))
		{
			return null;
		}
		List<ISceneObjAgent> list = new List<ISceneObjAgent>();
		List<ISceneObjAgent> list2 = null;
		list2 = self._lstIsActive.FindAll((ISceneObjAgent item0) => item0.GetType() == type);
		list.AddRange(list2);
		list2 = self._lstToActive.FindAll((ISceneObjAgent item0) => item0.GetType() == type);
		list.AddRange(list2);
		list2 = self._lstInactive.FindAll((ISceneObjAgent item0) => item0.GetType() == type);
		list.AddRange(list2);
		list2 = self._lstIdle.FindAll((ISceneObjAgent item0) => item0.GetType() == type);
		list.AddRange(list2);
		return list;
	}

	public static List<ISceneObjAgent> GetActiveSceneObjs(Type type, bool includChild = false)
	{
		if (self == null && !type.IsSubclassOf(typeof(ISceneObjAgent)))
		{
			return null;
		}
		List<ISceneObjAgent> list = new List<ISceneObjAgent>();
		list.AddRange(self._lstInactive.FindAll((ISceneObjAgent item0) => (!includChild) ? (item0.GetType() == type) : type.IsAssignableFrom(item0.GetType())));
		list.AddRange(self._lstIsActive.FindAll((ISceneObjAgent item0) => (!includChild) ? (item0.GetType() == type) : type.IsAssignableFrom(item0.GetType())));
		return list;
	}

	public static void OnActivationDependenceDirty(bool bAdd)
	{
		if ((object)self != null)
		{
			self._dirtyLstIsActive = true;
		}
	}

	public static int SetDirty(ISceneObjAgent obj)
	{
		if (null == self)
		{
			return 0;
		}
		if (!self._dirtyLstInactive && (!self._curBoundsInactive.Contains(obj.Pos) || self._curBoundsActive.Contains(obj.Pos)) && self._lstInactive.Contains(obj))
		{
			self._dirtyLstInactive = true;
			return 2;
		}
		if (!self._dirtyLstIdle && self._curBoundsInactive.Contains(obj.Pos) && self._lstIdle.Contains(obj))
		{
			self._dirtyLstIdle = true;
			return 1;
		}
		return 5;
	}

	private bool IntoInactive(ISceneObjAgent obj)
	{
		if (obj.TstYOnActivate)
		{
			return _curBoundsInactive.Contains(obj.Pos);
		}
		return _curRectInactive.Contains(new Vector2(obj.Pos.x, obj.Pos.z));
	}

	private bool ExitInactive(ISceneObjAgent obj)
	{
		if (obj.TstYOnActivate)
		{
			return !_curBoundsInactiveE.Contains(obj.Pos);
		}
		return !_curRectInactiveE.Contains(new Vector2(obj.Pos.x, obj.Pos.z));
	}

	private bool IntoActive(ISceneObjAgent obj)
	{
		if (obj.TstYOnActivate)
		{
			return _curBoundsActive.Contains(obj.Pos);
		}
		return _curRectActive.Contains(new Vector2(obj.Pos.x, obj.Pos.z));
	}

	private bool ExitActive(ISceneObjAgent obj)
	{
		if (obj.TstYOnActivate)
		{
			return !_curBoundsActive.Contains(obj.Pos);
		}
		return !_curRectActive.Contains(new Vector2(obj.Pos.x, obj.Pos.z));
	}

	private void UpdateIdle()
	{
		int count = _lstIdle.Count;
		for (int num = count - 1; num >= 0; num--)
		{
			ISceneObjAgent sceneObjAgent = _lstIdle[num];
			if (IntoInactive(sceneObjAgent))
			{
				_lstIdle.RemoveAt(num);
				_toConstruct.Add(sceneObjAgent);
				_lstInactive.Add(sceneObjAgent);
				_dirtyLstInactive = true;
			}
		}
		count = _toConstruct.Count;
		if (count > 0)
		{
			for (int num2 = count - 1; num2 >= 0; num2--)
			{
				_toConstruct[num2].OnConstruct();
			}
			_toConstruct.Clear();
		}
		_dirtyLstIdle = false;
	}

	private void UpdateInactive()
	{
		int count = _lstInactive.Count;
		for (int num = count - 1; num >= 0; num--)
		{
			ISceneObjAgent sceneObjAgent = _lstInactive[num];
			if (ExitInactive(sceneObjAgent))
			{
				_lstInactive.RemoveAt(num);
				_toDestruct.Add(sceneObjAgent);
				_lstIdle.Add(sceneObjAgent);
				_dirtyLstIdle = true;
			}
			else if (sceneObjAgent.NeedToActivate && IntoActive(sceneObjAgent))
			{
				_lstInactive.RemoveAt(num);
				_lstToActive.Add(sceneObjAgent);
			}
		}
		count = _toDestruct.Count;
		if (count > 0)
		{
			for (int num2 = count - 1; num2 >= 0; num2--)
			{
				_toDestruct[num2].OnDestruct();
			}
			_toDestruct.Clear();
		}
		_dirtyLstInactive = false;
	}

	private void UpdateToActive()
	{
		int count = _lstToActive.Count;
		for (int num = count - 1; num >= 0; num--)
		{
			ISceneObjAgent sceneObjAgent = _lstToActive[num];
			if (!IntoActive(sceneObjAgent))
			{
				_lstToActive.RemoveAt(num);
				_lstInactive.Add(sceneObjAgent);
				_dirtyLstInactive = true;
			}
			else
			{
				bool flag = true;
				EDependChunkType type = EDependChunkType.ChunkNotAvailable;
				int count2 = _lstActDependences.Count;
				for (int i = 0; i < count2; i++)
				{
					ISceneObjActivationDependence sceneObjActivationDependence = _lstActDependences[i];
					if (!sceneObjActivationDependence.IsDependableForAgent(sceneObjAgent, ref type))
					{
						flag = false;
						break;
					}
				}
				if (flag)
				{
					_lstToActive.RemoveAt(num);
					_toActivate.Add(sceneObjAgent);
					_lstIsActive.Add(sceneObjAgent);
				}
			}
		}
		count = _toActivate.Count;
		if (count > 0)
		{
			for (int num2 = count - 1; num2 >= 0; num2--)
			{
				_toActivate[num2].OnActivate();
			}
			_toActivate.Clear();
		}
	}

	private void UpdateIsActive()
	{
		int count = _lstIsActive.Count;
		for (int num = count - 1; num >= 0; num--)
		{
			ISceneObjAgent sceneObjAgent = _lstIsActive[num];
			if (ExitActive(sceneObjAgent))
			{
				_lstIsActive.RemoveAt(num);
				_toDeactivate.Add(sceneObjAgent);
				_lstInactive.Add(sceneObjAgent);
				_dirtyLstInactive = true;
			}
			else if (_dirtyLstIsActive)
			{
				bool flag = true;
				EDependChunkType type = EDependChunkType.ChunkNotAvailable;
				int count2 = _lstActDependences.Count;
				for (int i = 0; i < count2; i++)
				{
					ISceneObjActivationDependence sceneObjActivationDependence = _lstActDependences[i];
					if (!sceneObjActivationDependence.IsDependableForAgent(sceneObjAgent, ref type))
					{
						flag = false;
						break;
					}
				}
				if (!flag)
				{
					_lstIsActive.RemoveAt(num);
					_toDeactivate.Add(sceneObjAgent);
					_lstInactive.Add(sceneObjAgent);
					_dirtyLstInactive = true;
				}
			}
		}
		count = _toDeactivate.Count;
		if (count > 0)
		{
			for (int num2 = count - 1; num2 >= 0; num2--)
			{
				_toDeactivate[num2].OnDeactivate();
			}
			_toDeactivate.Clear();
		}
		_dirtyLstIsActive = false;
	}

	private void LoadLodDesc()
	{
		try
		{
			TerrainLodDescPaser.LoadTerLodDesc("TerrainLodDesc");
		}
		catch
		{
			Debug.Log("TerrainLodDesc not found.");
		}
		if (TerrainLodDescPaser.terLodDesc != null)
		{
			LODOctreeMan.ResetRootChunkCount(TerrainLodDescPaser.terLodDesc.x, TerrainLodDescPaser.terLodDesc.y, TerrainLodDescPaser.terLodDesc.z);
			MaxLod = TerrainLodDescPaser.terLodDesc.lod;
		}
	}

	private byte[] Export()
	{
		MemoryStream memoryStream = new MemoryStream();
		BinaryWriter binaryWriter = new BinaryWriter(memoryStream);
		binaryWriter.Write(0);
		_idGen.Serialize(binaryWriter);
		List<ISceneSerializableObjAgent> lstObjToSerialize = new List<ISceneSerializableObjAgent>();
		_lstIsActive.ForEach(delegate(ISceneObjAgent it)
		{
			if (it is ISceneSerializableObjAgent item)
			{
				lstObjToSerialize.Add(item);
			}
		});
		_lstToActive.ForEach(delegate(ISceneObjAgent it)
		{
			if (it is ISceneSerializableObjAgent item2)
			{
				lstObjToSerialize.Add(item2);
			}
		});
		_lstInactive.ForEach(delegate(ISceneObjAgent it)
		{
			if (it is ISceneSerializableObjAgent item3)
			{
				lstObjToSerialize.Add(item3);
			}
		});
		_lstIdle.ForEach(delegate(ISceneObjAgent it)
		{
			if (it is ISceneSerializableObjAgent item4)
			{
				lstObjToSerialize.Add(item4);
			}
		});
		int count = lstObjToSerialize.Count;
		binaryWriter.Write(count);
		for (int i = 0; i < count; i++)
		{
			ISceneSerializableObjAgent sceneSerializableObjAgent = lstObjToSerialize[i];
			binaryWriter.Write(sceneSerializableObjAgent.GetType().Name);
			binaryWriter.Write(0);
			try
			{
				long position = binaryWriter.BaseStream.Position;
				sceneSerializableObjAgent.Serialize(binaryWriter);
				long position2 = binaryWriter.BaseStream.Position;
				binaryWriter.BaseStream.Seek(position - 4, SeekOrigin.Begin);
				binaryWriter.Write((int)(position2 - position));
				binaryWriter.BaseStream.Seek(position2, SeekOrigin.Begin);
			}
			catch
			{
				Debug.LogError(string.Concat("[SceneMan]Failed to export ", sceneSerializableObjAgent.GetType(), sceneSerializableObjAgent.Id));
			}
		}
		binaryWriter.Close();
		memoryStream.Close();
		return memoryStream.ToArray();
	}

	private void Import(byte[] buffer)
	{
		_dataToImport = buffer;
		if (buffer == null || buffer.Length == 0)
		{
			return;
		}
		MemoryStream input = new MemoryStream(buffer);
		BinaryReader binaryReader = new BinaryReader(input);
		int num = binaryReader.ReadInt32();
		if (num != 0)
		{
			throw new Exception("Wrong save data version for scene manager:" + num + "|" + 0);
		}
		_idGen.Deserialize(binaryReader);
		int num2 = binaryReader.ReadInt32();
		_lstPosToSerialize = new List<long>(num2);
		_lstTypeToSerialize = new List<string>(num2);
		for (int i = 0; i < num2; i++)
		{
			string item = binaryReader.ReadString();
			int num3 = binaryReader.ReadInt32();
			if (num3 > 0)
			{
				_lstTypeToSerialize.Add(item);
				_lstPosToSerialize.Add(binaryReader.BaseStream.Position);
				binaryReader.BaseStream.Seek(num3, SeekOrigin.Current);
			}
		}
	}

	public void AddImportedObj(string typeNameMask = null)
	{
		if (_dataToImport == null || _lstPosToSerialize == null || _lstTypeToSerialize == null)
		{
			return;
		}
		MemoryStream input = new MemoryStream(_dataToImport);
		BinaryReader binaryReader = new BinaryReader(input);
		int count = _lstTypeToSerialize.Count;
		List<ISceneSerializableObjAgent> list = new List<ISceneSerializableObjAgent>(count);
		for (int num = count - 1; num >= 0; num--)
		{
			string text = _lstTypeToSerialize[num];
			long offset = _lstPosToSerialize[num];
			if (typeNameMask == null || text.Equals(typeNameMask))
			{
				try
				{
					Type type = Type.GetType(text);
					ISceneSerializableObjAgent sceneSerializableObjAgent = Activator.CreateInstance(type) as ISceneSerializableObjAgent;
					binaryReader.BaseStream.Seek(offset, SeekOrigin.Begin);
					sceneSerializableObjAgent.Deserialize(binaryReader);
					list.Add(sceneSerializableObjAgent);
				}
				catch (Exception ex)
				{
					throw new Exception("[SceneMan]Wrong save data format: DataType " + text + "\nDetail:" + ex);
				}
				_lstTypeToSerialize.RemoveAt(num);
				_lstPosToSerialize.RemoveAt(num);
			}
		}
		AddSceneObjs(list);
	}

	public void New()
	{
	}

	public void Restore()
	{
		Import(PeSingleton<ArchiveMgr>.Instance.GetData("ArchiveKeySceneManager"));
	}

	public static void RemoveTerrainDependence()
	{
		if (self != null && self._lstActDependences != null)
		{
			self._lstActDependences.Remove(SceneChunkDependence.Instance);
		}
	}

	public static void AddTerrainDependence()
	{
		if (self != null && self._lstActDependences != null && !self._lstActDependences.Contains(SceneChunkDependence.Instance))
		{
			self._lstActDependences.Add(SceneChunkDependence.Instance);
		}
	}

	public static List<ISceneObjAgent> FindAllDragItemInDungeon()
	{
		List<ISceneObjAgent> list = new List<ISceneObjAgent>();
		if (self != null)
		{
			list.AddRange(self._lstIsActive.FindAll((ISceneObjAgent it) => it is DragItemAgent && it.Pos.y < 0f));
			list.AddRange(self._lstToActive.FindAll((ISceneObjAgent it) => it is DragItemAgent && it.Pos.y < 0f));
			list.AddRange(self._lstInactive.FindAll((ISceneObjAgent it) => it is DragItemAgent && it.Pos.y < 0f));
			list.AddRange(self._lstIdle.FindAll((ISceneObjAgent it) => it is DragItemAgent && it.Pos.y < 0f));
		}
		return list;
	}
}
