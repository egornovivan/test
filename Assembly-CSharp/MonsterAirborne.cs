using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class MonsterAirborne : MonoBehaviour
{
	public enum Type
	{
		Puja,
		Paja
	}

	private enum Step
	{
		Null,
		FadeIn,
		Running,
		FadeOut
	}

	private const float _initHeight = 150f;

	public float _endHeight = 20f;

	public float _endWaveAmp = 1f;

	public float _coefWave = 0.01f;

	public float _spdDown = 1f;

	public float _itvCreate = 3f;

	private float _spdWave = 0.02f;

	private Type _type = Type.Paja;

	private Step _step;

	private float _lastTime;

	private Vector3 _dstPos = Vector3.zero;

	private Transform _tAirborne;

	private List<SceneEntityPosAgent> _agents = new List<SceneEntityPosAgent>();

	private void Start()
	{
		string pathName = ((_type != 0) ? "Item/scene_paja_aircraft.unity3d" : "Item/scene_puja_aircraft.unity3d");
		AssetReq assetReq = AssetsLoader.Instance.AddReq(pathName, Vector3.zero, Quaternion.identity, Vector3.one);
		assetReq.ReqFinishHandler += StartAirborne;
	}

	private void StartAirborne(GameObject airborneGo)
	{
		_tAirborne = airborneGo.transform;
		_tAirborne.parent = base.transform;
		ReqFadeIn();
		StartCoroutine(Exec());
	}

	private IEnumerator Exec()
	{
		while (true)
		{
			switch (_step)
			{
			case Step.FadeIn:
				if (_tAirborne.position.y > _dstPos.y)
				{
					_tAirborne.position -= _spdDown * Vector3.up;
					break;
				}
				_lastTime = -1f;
				_spdWave = 0f;
				_step = Step.Running;
				break;
			case Step.Running:
			{
				float a = _coefWave * (base.transform.position.y + _endHeight - _tAirborne.position.y);
				_spdWave += a;
				_tAirborne.position += _spdWave * Vector3.up;
				if (_agents.Count > 0 && Time.realtimeSinceStartup > _lastTime + _itvCreate)
				{
					Transform tSpawn = _tAirborne.Find("CreatMonster");
					_agents[0].Pos = ((!(tSpawn != null)) ? _tAirborne.position : tSpawn.position);
					_agents[0].protoId &= -201326593;
					MonsterEntityCreator.CreateMonster(_agents[0]);
					_agents.RemoveAt(0);
					_lastTime = Time.realtimeSinceStartup;
				}
				break;
			}
			case Step.FadeOut:
				if (_tAirborne.position.y < _dstPos.y)
				{
					_tAirborne.position += _spdDown * Vector3.up;
				}
				else
				{
					Object.Destroy(base.gameObject);
				}
				break;
			}
			yield return 0;
		}
	}

	private void ReqFadeIn()
	{
		_tAirborne.position = base.transform.position + 150f * Vector3.up;
		_dstPos = base.transform.position + (_endHeight - _endWaveAmp) * Vector3.up;
		_step = Step.FadeIn;
	}

	private void ReqFadeOut(bool bImm)
	{
		_dstPos = ((!bImm) ? (base.transform.position + 150f * Vector3.up) : _tAirborne.position);
		_step = Step.FadeOut;
	}

	public static MonsterAirborne CreateAirborne(Vector3 landPos, Type type)
	{
		GameObject gameObject = new GameObject("Airborne_" + type);
		MonsterAirborne monsterAirborne = gameObject.AddComponent<MonsterAirborne>();
		monsterAirborne._type = type;
		monsterAirborne.transform.position = landPos;
		return monsterAirborne;
	}

	public static void DestroyAirborne(MonsterAirborne mab, bool bImm = false)
	{
		mab.ReqFadeOut(bImm);
	}

	public void AddAirborneReq(SceneEntityPosAgent agent)
	{
		_agents.Add(agent);
	}
}
