using System.IO;
using UnityEngine;

namespace CustomData;

public class BuildCube
{
	private Vector3 _startPos;

	private Vector3 _endPos;

	private bool _straightMode;

	private bool _dragHeight;

	private bool _deleteMode;

	private byte _rotation;

	private int _heightLength;

	private int _id;

	private int _matIndex;

	private IntVector3 _pivot;

	public Vector3 StartPos => _startPos;

	public Vector3 EndPos => _endPos;

	public bool StraightMode => _straightMode;

	public bool DragHeight => _dragHeight;

	public bool DeleteMode => _deleteMode;

	public byte Rotation => _rotation;

	public int HeightLength => _heightLength;

	public int ID => _id;

	public int MatIndex => _matIndex;

	public IntVector3 Pivot => _pivot;

	public virtual bool Equals(BuildCube bc)
	{
		if (bc._startPos.Equals(_startPos) && bc._endPos.Equals(_endPos) && bc._straightMode == _straightMode && bc._dragHeight == _dragHeight && bc._deleteMode == _deleteMode && bc._rotation == _rotation && bc._heightLength == _heightLength && bc._id == _id && bc._matIndex == _matIndex && bc._pivot.Equals(_pivot))
		{
			return true;
		}
		return false;
	}

	public void Init(Vector3 startPos, Vector3 endPos, bool straightMode, bool dragHeight, bool deleteMode, byte rotation, int heightLength, int id, int matIndex, IntVector3 pivot)
	{
		_startPos = startPos;
		_endPos = endPos;
		_straightMode = straightMode;
		_dragHeight = dragHeight;
		_deleteMode = deleteMode;
		_rotation = rotation;
		_heightLength = heightLength;
		_id = id;
		_matIndex = matIndex;
		_pivot = pivot;
	}

	public void Init(BuildCube bc)
	{
		_startPos = bc._startPos;
		_endPos = bc._endPos;
		_straightMode = bc._straightMode;
		_dragHeight = bc._dragHeight;
		_deleteMode = bc._deleteMode;
		_rotation = bc._rotation;
		_heightLength = bc._heightLength;
		_id = bc._id;
		_matIndex = bc._matIndex;
		_pivot = bc._pivot;
	}

	public byte[] ToBuffer()
	{
		using BinaryWriter binaryWriter = new BinaryWriter(new MemoryStream(64));
		binaryWriter.Write(_startPos.x);
		binaryWriter.Write(_startPos.y);
		binaryWriter.Write(_startPos.z);
		binaryWriter.Write(_endPos.x);
		binaryWriter.Write(_endPos.y);
		binaryWriter.Write(_endPos.z);
		binaryWriter.Write(_straightMode);
		binaryWriter.Write(_dragHeight);
		binaryWriter.Write(_deleteMode);
		binaryWriter.Write(_rotation);
		binaryWriter.Write(_heightLength);
		binaryWriter.Write(_id);
		binaryWriter.Write(_matIndex);
		binaryWriter.Write(_pivot.x);
		binaryWriter.Write(_pivot.y);
		binaryWriter.Write(_pivot.z);
		binaryWriter.Flush();
		MemoryStream memoryStream = binaryWriter.BaseStream as MemoryStream;
		return memoryStream.ToArray();
	}

	public void FromBuffer(byte[] buffer)
	{
		using BinaryReader binaryReader = new BinaryReader(new MemoryStream(buffer));
		_startPos.x = binaryReader.ReadSingle();
		_startPos.y = binaryReader.ReadSingle();
		_startPos.z = binaryReader.ReadSingle();
		_endPos.x = binaryReader.ReadSingle();
		_endPos.y = binaryReader.ReadSingle();
		_endPos.z = binaryReader.ReadSingle();
		_straightMode = binaryReader.ReadBoolean();
		_dragHeight = binaryReader.ReadBoolean();
		_deleteMode = binaryReader.ReadBoolean();
		_rotation = binaryReader.ReadByte();
		_heightLength = binaryReader.ReadInt32();
		_id = binaryReader.ReadInt32();
		_matIndex = binaryReader.ReadInt32();
		int x_ = binaryReader.ReadInt32();
		int y_ = binaryReader.ReadInt32();
		int z_ = binaryReader.ReadInt32();
		_pivot = new IntVector3(x_, y_, z_);
	}
}
