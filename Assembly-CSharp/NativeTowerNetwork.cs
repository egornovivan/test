using uLink;
using UnityEngine;

public class NativeTowerNetwork : AiNetwork
{
	private int mTownId;

	private int mCampId;

	private int mDamageId;

	private Vector3 mScale;

	protected override void OnPEInstantiate(uLink.NetworkMessageInfo info)
	{
		_id = info.networkView.initialData.Read<int>(new object[0]);
		_externId = info.networkView.initialData.Read<int>(new object[0]);
		_worldId = info.networkView.initialData.Read<int>(new object[0]);
		mTownId = info.networkView.initialData.Read<int>(new object[0]);
		mCampId = info.networkView.initialData.Read<int>(new object[0]);
		mDamageId = info.networkView.initialData.Read<int>(new object[0]);
		mScale = info.networkView.initialData.Read<Vector3>(new object[0]);
	}

	protected override void RPC_S2C_InitData(uLink.BitStream stream, uLink.NetworkMessageInfo info)
	{
		base.transform.position = stream.Read<Vector3>(new object[0]);
		base.transform.rotation = stream.Read<Quaternion>(new object[0]);
		base.authId = stream.Read<int>(new object[0]);
		DoodadEntityCreator.CreateNetRandTerDoodad(base.Id, base.ExternId, base.transform.position, mScale, base.transform.rotation, mTownId, mCampId, mDamageId);
	}
}
