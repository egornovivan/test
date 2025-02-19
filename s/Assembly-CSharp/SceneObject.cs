using uLink;
using UnityEngine;

public class SceneObject : ISceneObject
{
	protected int _id;

	protected int _protoId;

	protected int _worldId;

	protected int _scenarioId;

	protected ESceneObjType _type;

	protected Vector3 _pos;

	protected Vector3 _scale;

	protected Quaternion _rot;

	public int Id => _id;

	public int ProtoId => _protoId;

	public int WorldId => _worldId;

	public int ScenarioId => _scenarioId;

	public ESceneObjType Type => _type;

	public Vector3 Pos
	{
		get
		{
			return _pos;
		}
		set
		{
			_pos = value;
		}
	}

	public Vector3 Scale => _scale;

	public Quaternion Rot
	{
		get
		{
			return _rot;
		}
		set
		{
			_rot = value;
		}
	}

	public SceneObject()
	{
		_type = ESceneObjType.NONE;
	}

	public void Init(int id, int protoId, Vector3 pos, Vector3 scale, Quaternion rot, int worldId)
	{
		_id = id;
		_protoId = protoId;
		_pos = pos;
		_scale = scale;
		_rot = rot;
		_worldId = worldId;
		_scenarioId = -1;
	}

	public void SetType(ESceneObjType type)
	{
		_type = type;
	}

	public void SetScenarioId(int scenarioId)
	{
		_scenarioId = scenarioId;
	}

	public static object Deserialize(uLink.BitStream stream, params object[] codecOptions)
	{
		SceneObject sceneObject = new SceneObject();
		sceneObject._id = stream.Read<int>(new object[0]);
		sceneObject._protoId = stream.Read<int>(new object[0]);
		sceneObject._worldId = stream.Read<int>(new object[0]);
		sceneObject._type = stream.Read<ESceneObjType>(new object[0]);
		sceneObject._pos = stream.Read<Vector3>(new object[0]);
		sceneObject._scale = stream.Read<Vector3>(new object[0]);
		sceneObject._rot = stream.Read<Quaternion>(new object[0]);
		sceneObject._scenarioId = stream.Read<int>(new object[0]);
		return sceneObject;
	}

	public static void Serialize(uLink.BitStream stream, object value, params object[] codecOptions)
	{
		SceneObject sceneObject = value as SceneObject;
		stream.Write(sceneObject._id);
		stream.Write(sceneObject._protoId);
		stream.Write(sceneObject._worldId);
		stream.Write(sceneObject._type);
		stream.Write(sceneObject._pos);
		stream.Write(sceneObject._scale);
		stream.Write(sceneObject._rot);
		stream.Write(sceneObject._scenarioId);
	}
}
