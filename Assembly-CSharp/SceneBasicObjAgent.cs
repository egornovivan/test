using UnityEngine;

public class SceneBasicObjAgent : ISceneObjAgent
{
	protected int _id;

	protected Vector3 _pos = Vector3.zero;

	protected Vector3 _scl = Vector3.one;

	protected Quaternion _rot = Quaternion.identity;

	protected string _pathPreAsset = string.Empty;

	protected string _pathMainAsset = string.Empty;

	protected GameObject _go;

	protected GameObject _mainGo;

	protected bool _bMainAssetLoading;

	public bool IsMainAssetLoading => _bMainAssetLoading;

	public virtual int Id
	{
		get
		{
			return _id;
		}
		set
		{
			_id = value;
		}
	}

	public int ScenarioId { get; set; }

	public GameObject Go => _go;

	public virtual Vector3 Pos => _pos;

	public virtual IBoundInScene Bound => null;

	public virtual bool NeedToActivate => true;

	public virtual bool TstYOnActivate => true;

	public SceneBasicObjAgent()
	{
	}

	public SceneBasicObjAgent(string pathPreAsset, string pathMainAsset, Vector3 pos, Quaternion rotation, Vector3 scale, int id = 0)
	{
		_id = id;
		_pos = pos;
		_scl = scale;
		_rot = rotation;
		if (pathPreAsset != null)
		{
			_pathPreAsset = pathPreAsset;
			TryLoadPreGo();
		}
		if (pathMainAsset != null)
		{
			_pathMainAsset = pathMainAsset;
		}
	}

	public virtual void OnConstruct()
	{
		TryLoadMainGo();
	}

	public virtual void OnDestruct()
	{
		if (!(_mainGo == null))
		{
			OnMainGoDestroy();
			Object.Destroy(_mainGo);
			_mainGo = null;
		}
	}

	public virtual void OnActivate()
	{
	}

	public virtual void OnDeactivate()
	{
	}

	public virtual void OnPreGoLoaded()
	{
		if (SceneMan.self != null)
		{
			_go.transform.parent = SceneMan.self.transform;
		}
	}

	public virtual void OnMainGoLoaded()
	{
		if (_mainGo == _go && SceneMan.self != null)
		{
			_go.transform.parent = SceneMan.self.transform;
		}
	}

	public virtual void OnMainGoDestroy()
	{
	}

	protected void TryLoadPreGo()
	{
		if (_pathPreAsset.Length > 0)
		{
			_go = Object.Instantiate(Resources.Load(_pathPreAsset), _pos, _rot) as GameObject;
			_go.transform.localScale = _scl;
			OnPreGoLoaded();
		}
	}

	protected void TryLoadMainGo()
	{
		if (_pathMainAsset.Length > 0)
		{
			_bMainAssetLoading = true;
			if (_pathMainAsset.Contains(".unity3d"))
			{
				AssetPRS assetPos = new AssetPRS(_pos, _rot, _scl);
				AssetReq assetReq = new AssetReq(_pathMainAsset, assetPos);
				AssetsLoader.Instance.AddReq(assetReq);
				assetReq.ReqFinishHandler += OnAssetLoaded;
			}
			else
			{
				GameObject gameObject = Object.Instantiate(Resources.Load(_pathMainAsset), _pos, _rot) as GameObject;
				gameObject.transform.localScale = _scl;
				OnAssetLoaded(gameObject);
			}
		}
	}

	protected void OnAssetLoaded(GameObject go)
	{
		_bMainAssetLoading = false;
		_mainGo = go;
		if (!(_mainGo == null))
		{
			if (_go != null)
			{
				_mainGo.transform.parent = _go.transform;
			}
			else
			{
				_go = _mainGo;
			}
			OnMainGoLoaded();
		}
	}
}
