using System.Collections.Generic;
using System.IO;
using Pathea;
using UnityEngine;

public static class RelationshipDataMgr
{
	public const int VERSION0 = 0;

	public const int CUR_VERSION = 0;

	public static List<RelationshipData> mRelationship = new List<RelationshipData>();

	public static void AddRelationship(PeEntity player, PeEntity mounts)
	{
		if (mRelationship.Count < 1 && !(player == null) && !(mounts == null))
		{
			RelationshipData relationshipData = new RelationshipData();
			relationshipData._playerId = player.Id;
			relationshipData._mountsProtoId = mounts.ProtoID;
			relationshipData.AddData(mounts);
			mRelationship.Add(relationshipData);
		}
	}

	public static void RemoveRalationship(int playerId, int mountsprotoId)
	{
		int count = mRelationship.Count;
		for (int num = count - 1; num >= 0; num--)
		{
			if (mRelationship[num]._playerId == playerId && mRelationship[num]._mountsProtoId == mountsprotoId)
			{
				mRelationship.Remove(mRelationship[num]);
			}
		}
	}

	public static void RecoverRelationship(int playerId)
	{
		int count = mRelationship.Count;
		for (int num = count - 1; num >= 0; num--)
		{
			if (mRelationship[num]._playerId == playerId && !mRelationship[num].RecoverRelationship())
			{
				mRelationship.Remove(mRelationship[num]);
			}
		}
	}

	public static void Clear()
	{
		mRelationship.Clear();
	}

	public static void Import(byte[] buffer)
	{
		Clear();
		MemoryStream input = new MemoryStream(buffer);
		BinaryReader binaryReader = new BinaryReader(input);
		int num = binaryReader.ReadInt32();
		if (num != 0)
		{
			Debug.LogWarning("The version of ColonyrecordMgr is newer than the record.");
		}
		int num2 = binaryReader.ReadInt32();
		if (num >= 0)
		{
			for (int i = 0; i < num2; i++)
			{
				RelationshipData relationshipData = new RelationshipData();
				relationshipData.Import(binaryReader);
				mRelationship.Add(relationshipData);
			}
		}
	}

	public static void Export(BinaryWriter w)
	{
		w.Write(0);
		w.Write(mRelationship.Count);
		for (int i = 0; i < mRelationship.Count; i++)
		{
			mRelationship[i].Export(w);
		}
	}
}
