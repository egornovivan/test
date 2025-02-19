using System;
using System.Collections.Generic;
using System.IO;
using UnityEngine;

public class BuildingEditOps : MonoBehaviour
{
	public struct OpInfo
	{
		public byte x;

		public byte y;

		public byte z;

		public byte voxelByte0;

		public byte voxelByte1;

		public bool Equals(OpInfo val)
		{
			return x == val.x && y == val.y && z == val.z && voxelByte0 == val.voxelByte0 && voxelByte1 == val.voxelByte1;
		}
	}

	private GameObject cursorCubeGo;

	private GameObject RefPlaneManGo;

	private Block45CurMan b45Building;

	private Block45OctDataSource _voxels;

	private int currentShape;

	private int currentRotation;

	private int currentMat;

	private IntVector3 buildCursorPos;

	private int lastInvocation = -1;

	private int opFrequency = 50;

	private List<IntVector3> spots;

	private bool randomized;

	private int numClicks = 100;

	private int curClick = -1;

	private List<OpInfo> opList;

	private void Awake()
	{
	}

	private void Start()
	{
	}

	public void setShape(int val)
	{
		currentShape = val;
	}

	public void setMat(int val)
	{
		currentMat = val;
	}

	public void setRotation(int val)
	{
		currentRotation = val;
	}

	private void Update()
	{
		if (_voxels == null && b45Building != null)
		{
			_voxels = b45Building.DataSource;
		}
		Ray ray = Camera.main.ScreenPointToRay(Input.mousePosition);
		if (Physics.Raycast(ray, out var hitInfo, 100f) && cursorCubeGo != null)
		{
			cursorCubeGo.GetComponent<MeshRenderer>().enabled = true;
			Vector3 point = hitInfo.point;
			point *= 2f;
			point.x = Mathf.FloorToInt(point.x);
			point.y = Mathf.FloorToInt(point.y);
			point.z = Mathf.FloorToInt(point.z);
			point.x += 0.25f;
			point.y += 0.25f;
			point.z += 0.25f;
			buildCursorPos = new IntVector3(point);
			point /= 2f;
			cursorCubeGo.transform.position = point;
			if (point.x >= 0f && point.y >= 0f && point.z >= 0f)
			{
				buildCursorPos.x = Mathf.FloorToInt(buildCursorPos.x);
				buildCursorPos.y = Mathf.FloorToInt(buildCursorPos.y);
				buildCursorPos.z = Mathf.FloorToInt(buildCursorPos.z);
			}
			else
			{
				cursorCubeGo.GetComponent<MeshRenderer>().enabled = false;
				buildCursorPos = null;
			}
		}
		if (Input.GetMouseButtonUp(0) && Input.GetKey(KeyCode.LeftAlt) && buildCursorPos != null)
		{
			B45Block blk = default(B45Block);
			blk.blockType = B45Block.MakeBlockType(currentShape, currentRotation);
			blk.materialType = (byte)currentMat;
			b45Building.AlterBlockInBuild(buildCursorPos.x, buildCursorPos.y, buildCursorPos.z, blk);
		}
		testRoutine();
	}

	private void testRoutine()
	{
		if (Environment.TickCount - lastInvocation <= opFrequency || curClick == -1)
		{
			return;
		}
		B45Block blk = default(B45Block);
		blk.blockType = B45Block.MakeBlockType(1, 0);
		blk.materialType = (byte)(Mathf.RoundToInt(UnityEngine.Random.value * 65535f) % 16);
		int num;
		int num2;
		int num3;
		if (randomized)
		{
			num = Mathf.RoundToInt(UnityEngine.Random.value * 65535f) % 512;
			num2 = Mathf.RoundToInt(UnityEngine.Random.value * 65535f) % 127 + 1;
			num3 = Mathf.RoundToInt(UnityEngine.Random.value * 65535f) % 512;
		}
		else
		{
			num = spots[curClick].x;
			num2 = spots[curClick].y;
			num3 = spots[curClick].z;
		}
		b45Building.AlterBlockInBuild(num, num2, num3, blk);
		OpInfo item = default(OpInfo);
		item.x = (byte)num;
		item.y = (byte)num2;
		item.z = (byte)num3;
		item.voxelByte0 = blk.blockType;
		item.voxelByte1 = blk.materialType;
		opList.Add(item);
		curClick++;
		if (curClick >= numClicks)
		{
			MonoBehaviour.print("simulated test finished.");
			FileStream fileStream = new FileStream("test_simulation.bin", FileMode.Append, FileAccess.Write, FileShare.Read);
			byte[] array = new byte[5];
			for (int i = 0; i < opList.Count; i++)
			{
				array[0] = opList[i].x;
				array[1] = opList[i].y;
				array[2] = opList[i].z;
				array[3] = opList[i].voxelByte0;
				array[4] = opList[i].voxelByte1;
				fileStream.Write(array, 0, 5);
			}
			fileStream.Close();
			opList.Clear();
			curClick = -1;
		}
		lastInvocation = Environment.TickCount;
	}

	private void OnGUI()
	{
		if (GUI.Button(new Rect(10f, 10f, 140f, 45f), "randomized plots"))
		{
			randomized = true;
			if (opList == null)
			{
				opList = new List<OpInfo>();
			}
			opList.Clear();
			curClick = 0;
			UnityEngine.Random.seed = Environment.TickCount % 65536;
		}
		if (GUI.Button(new Rect(10f, 70f, 140f, 45f), "sequential plots"))
		{
			randomized = false;
			spots = new List<IntVector3>();
			for (int i = 0; i < 10; i++)
			{
				spots.Add(new IntVector3(1, 1 + i, 1));
			}
			numClicks = spots.Count;
			if (opList == null)
			{
				opList = new List<OpInfo>();
			}
			opList.Clear();
			curClick = 0;
		}
		if (GUI.Button(new Rect(10f, 130f, 80f, 45f), "verify"))
		{
			if (opList == null)
			{
				opList = new List<OpInfo>();
			}
			opList.Clear();
			loadPreviousSimulatedTestData();
			verify();
		}
		if (!GUI.Button(new Rect(10f, 190f, 80f, 45f), "merge"))
		{
		}
	}

	private void loadPreviousSimulatedTestData()
	{
		FileStream fileStream = new FileStream("test_simulation.bin", FileMode.Open, FileAccess.Read, FileShare.Read);
		byte[] array = new byte[5];
		OpInfo item = default(OpInfo);
		for (int i = 0; i < fileStream.Length / 5; i++)
		{
			fileStream.Read(array, 0, 5);
			item.x = array[0];
			item.y = array[1];
			item.z = array[2];
			item.voxelByte0 = array[3];
			item.voxelByte1 = array[4];
			opList.Add(item);
		}
		fileStream.Close();
	}

	private void verify()
	{
		B45ChunkGo[] componentsInChildren = GameObject.Find("Block45").GetComponentsInChildren<B45ChunkGo>();
		foreach (B45ChunkGo b45ChunkGo in componentsInChildren)
		{
			if (b45ChunkGo._data == null)
			{
				continue;
			}
			List<OpInfo> list = b45ChunkGo._data.OccupiedVecs();
			for (int num = list.Count - 1; num >= 0; num--)
			{
				for (int num2 = opList.Count - 1; num2 >= 0; num2--)
				{
					if (list[num].Equals(opList[num2]))
					{
						opList.RemoveAt(num2);
						break;
					}
				}
			}
		}
		MonoBehaviour.print("leftovers: " + opList.Count);
		for (int j = 0; j < opList.Count; j++)
		{
			MonoBehaviour.print(":(" + opList[j].x + "," + opList[j].y + "," + opList[j].z + ") " + opList[j].voxelByte0 + "-" + opList[j].voxelByte1 + ";");
		}
	}
}
