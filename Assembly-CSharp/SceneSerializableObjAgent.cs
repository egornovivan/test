using System.IO;
using UnityEngine;

public class SceneSerializableObjAgent : SceneBasicObjAgent, ISerializable, ISceneObjAgent, ISceneSerializableObjAgent
{
	protected SceneObjAdditionalSaveData _additionalData = new SceneObjAdditionalSaveData();

	public SceneSerializableObjAgent()
	{
	}

	public SceneSerializableObjAgent(string pathPreAsset, string pathMainAsset, Vector3 pos, Quaternion rotation, Vector3 scale, int id = 0)
		: base(pathPreAsset, pathMainAsset, pos, rotation, scale, id)
	{
	}

	public virtual void Serialize(BinaryWriter bw)
	{
		bw.Write(_id);
		bw.Write(_pos.x);
		bw.Write(_pos.y);
		bw.Write(_pos.z);
		bw.Write(_scl.x);
		bw.Write(_scl.y);
		bw.Write(_scl.z);
		bw.Write(_rot.x);
		bw.Write(_rot.y);
		bw.Write(_rot.z);
		bw.Write(_rot.w);
		bw.Write(_pathPreAsset);
		bw.Write(_pathMainAsset);
		_additionalData.CollectData(_go);
		_additionalData.Serialize(bw);
	}

	public virtual void Deserialize(BinaryReader br)
	{
		_id = br.ReadInt32();
		_pos.x = br.ReadSingle();
		_pos.y = br.ReadSingle();
		_pos.z = br.ReadSingle();
		_scl.x = br.ReadSingle();
		_scl.y = br.ReadSingle();
		_scl.z = br.ReadSingle();
		_rot.x = br.ReadSingle();
		_rot.y = br.ReadSingle();
		_rot.z = br.ReadSingle();
		_rot.w = br.ReadSingle();
		_pathPreAsset = br.ReadString();
		_pathMainAsset = br.ReadString();
		_additionalData.Deserialize(br);
		if (_go == null)
		{
			TryLoadPreGo();
		}
		_additionalData.DispatchData(_go);
	}

	public override void OnMainGoLoaded()
	{
		base.OnMainGoLoaded();
		_additionalData.DispatchData(_mainGo);
	}

	public override void OnMainGoDestroy()
	{
		base.OnMainGoDestroy();
		_additionalData.CollectData(_mainGo);
	}
}
