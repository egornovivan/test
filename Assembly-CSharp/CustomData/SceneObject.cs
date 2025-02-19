using uLink;
using UnityEngine;

namespace CustomData;

public class SceneObject
{
	protected int _objId;

	protected int _protoId;

	protected int _worldId;

	protected int _scenarioId;

	protected ESceneObjType _type;

	protected Vector3 _pos;

	protected Vector3 _scale;

	protected Quaternion _rot;

	public int Id => _objId;

	public int ProtoId => _protoId;

	public int WorldId => _worldId;

	public int ScenarioId => _scenarioId;

	public ESceneObjType Type => _type;

	public Vector3 Pos => _pos;

	public Vector3 Scale => _scale;

	public Quaternion Rot => _rot;

	public SceneObject()
	{
		_scenarioId = -1;
	}

	public static object Deserialize(uLink.BitStream stream, params object[] codecOptions)
	{
		SceneObject sceneObject = new SceneObject();
		sceneObject._objId = stream.Read<int>(new object[0]);
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
		stream.Write(sceneObject._objId);
		stream.Write(sceneObject._protoId);
		stream.Write(sceneObject._worldId);
		stream.Write(sceneObject._type);
		stream.Write(sceneObject._pos);
		stream.Write(sceneObject._scale);
		stream.Write(sceneObject._rot);
		stream.Write(sceneObject._scenarioId);
	}
}
