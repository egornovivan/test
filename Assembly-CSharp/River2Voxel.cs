using System;
using System.Collections.Generic;
using System.IO;
using UnityEngine;

public class River2Voxel
{
	private const int VolPlus = 32;

	private VFDataReader terraReader;

	private VFDataReader waterReader;

	public static string outputDir = Application.dataPath + "/../Water";

	private string curOutputDir = string.Empty;

	public River2Voxel()
	{
		terraReader = new VFDataReader(VFVoxelTerrain.MapDataPath_Zip + "/map", OnChunkDataLoad);
		waterReader = new VFDataReader(VFVoxelTerrain.MapDataPath_Zip + "/water", OnChunkDataLoad);
	}

	public void GenRiverVoxels(AttachedRiverScript riverScript)
	{
		MeshCollider component = riverScript.GetComponent<MeshCollider>();
		if (component == null)
		{
			riverScript.CreateMesh(riverScript.riverSmooth);
			component = riverScript.GetComponent<MeshCollider>();
		}
		if (component == null || component.sharedMesh == null)
		{
			Debug.LogError("Can not find mesh collider");
			return;
		}
		VFVoxelWater.InitSufaceChunkData();
		Bounds bounds = component.sharedMesh.bounds;
		Vector3 boundMin = bounds.min;
		Vector3 boundMax = bounds.max;
		IntVector4 intVector = new IntVector4();
		if (!Directory.Exists(outputDir))
		{
			Directory.CreateDirectory(outputDir);
		}
		curOutputDir = outputDir + "/" + riverScript.name;
		if (!Directory.Exists(curOutputDir))
		{
			Directory.CreateDirectory(curOutputDir);
		}
		string filePrefix = curOutputDir + "/water";
		intVector.w = 0;
		while (intVector.w <= 4)
		{
			int num = 1 << intVector.w;
			int num2 = 1 << intVector.w;
			int num3 = 2 << intVector.w;
			int num4 = (int)boundMin.x - num2 >> 5 + intVector.w << intVector.w;
			int num5 = (int)boundMin.y - num2 >> 5 + intVector.w << intVector.w;
			int num6 = (int)boundMin.z - num2 >> 5 + intVector.w << intVector.w;
			int num7 = (int)boundMax.x + num3 >> 5 + intVector.w << intVector.w;
			int num8 = (int)boundMax.y + num3 >> 5 + intVector.w << intVector.w;
			int num9 = (int)boundMax.z + num3 >> 5 + intVector.w << intVector.w;
			if (num5 < 0)
			{
				num5 = 0;
			}
			int num10 = ((num7 - num4 >> intVector.w) + 1) * ((num8 - num5 >> intVector.w) + 1) * ((num9 - num6 >> intVector.w) + 1);
			float num11 = 1f / (float)num10;
			float num12 = 0f;
			intVector.x = num4;
			while (intVector.x <= num7)
			{
				intVector.z = num6;
				while (intVector.z <= num9)
				{
					intVector.y = num5;
					while (intVector.y <= num8)
					{
						GenARiverChunk(ref boundMin, ref boundMax, component, new IntVector4(intVector), filePrefix);
						intVector.y += num;
					}
					intVector.z += num;
				}
				intVector.x += num;
			}
			intVector.w++;
		}
	}

	private void GenARiverChunk(ref Vector3 boundMin, ref Vector3 boundMax, MeshCollider mc, IntVector4 chunkPos, string filePrefix)
	{
		int num = 1 << chunkPos.w;
		int num2 = 35 << chunkPos.w;
		int num3 = (chunkPos.x << 5) - num;
		int num4 = (chunkPos.y << 5) - num;
		int num5 = (chunkPos.z << 5) - num;
		int b = num3 + num2;
		int b2 = num4 + num2;
		int b3 = num5 + num2;
		int num6 = Mathf.Max((int)boundMin.x, num3);
		int num7 = Mathf.Max((int)boundMin.y, num4);
		int num8 = Mathf.Max((int)boundMin.z, num5);
		int num9 = Mathf.Min((int)boundMax.x, b);
		int num10 = Mathf.Min((int)boundMax.y, b2);
		int num11 = Mathf.Min((int)boundMax.z, b3);
		if (num6 >= num9 || num7 >= num10 || num8 >= num11)
		{
			return;
		}
		VFVoxelChunkData vFVoxelChunkData = terraReader.ReadChunkImm(chunkPos);
		byte[] dataVT = vFVoxelChunkData.DataVT;
		VFVoxelChunkData vFVoxelChunkData2 = waterReader.ReadChunkImm(chunkPos);
		byte[] dataVT2 = vFVoxelChunkData2.DataVT;
		Ray ray = default(Ray);
		ray.direction = Vector3.down;
		Vector3 origin = new Vector3(0f, boundMax.y + 0.5f, 0f);
		float maxDistance = boundMax.y - (float)num7 + 1f;
		int num12 = 1 << chunkPos.w;
		int num13 = -1 << chunkPos.w;
		num6 &= num13;
		num7 &= num13;
		num8 &= num13;
		int num14 = ((num4 < 0) ? 1 : 0);
		int num15 = VFVoxelChunkData.OneIndexNoPrefix(num6 - num3 >> chunkPos.w, 0, num8 - num5 >> chunkPos.w);
		bool flag = false;
		int num16 = num8;
		while (num16 < num11)
		{
			int num17 = num15;
			origin.z = num16;
			int num18 = num6;
			while (num18 < num9)
			{
				origin.x = num18;
				ray.origin = origin;
				if (mc.Raycast(ray, out var hitInfo, maxDistance) && hitInfo.point.y >= VFVoxelWater.c_fWaterLvl)
				{
					float y = hitInfo.point.y;
					int num19 = (int)(y + 0.5f * (float)num12);
					int num20 = num19 - num4 >> chunkPos.w;
					if (num20 >= 0)
					{
						bool flag2 = false;
						int num21;
						int num22;
						if (num20 >= 35)
						{
							num20 = 35;
							num21 = 34;
							num22 = (num17 << 1) + num21 * 70;
						}
						else
						{
							num22 = (num17 << 1) + num20 * 70;
							int num23 = 255 - dataVT[num22];
							byte b4 = 0;
							if (num23 > 0)
							{
								float num24 = y / (float)num12;
								float num25 = num24 - (float)(int)num24;
								b4 = ((!(num25 < 0.5f)) ? ((byte)(255.999f * (1f - 0.5f / num25))) : ((byte)(128f / (1f - num25))));
								if (b4 > dataVT2[num22])
								{
									dataVT2[num22] = b4;
									dataVT2[num22 + 1] = 128;
									flag2 = true;
								}
							}
							num22 -= 70;
							num21 = num20 - 1;
							if (flag2 && num21 >= 0)
							{
								if (b4 < 128)
								{
									dataVT2[num22] = byte.MaxValue;
									dataVT2[num22 + 1] = 128;
									num22 -= 70;
									num21--;
								}
								else
								{
									dataVT2[num22] = 128;
									dataVT2[num22 + 1] = 128;
								}
							}
						}
						bool flag3 = false;
						bool flag4 = false;
						while (num21 >= num14)
						{
							flag4 = false;
							int num23 = 255 - dataVT[num22];
							if (flag3)
							{
								num23 += 32;
							}
							if (num23 > dataVT2[num22])
							{
								if (!flag3)
								{
									num23 += 32;
								}
								if (num23 > 255)
								{
									if (num21 + 1 < num20)
									{
										int num26 = dataVT2[num22 + 70];
										if (num26 < 255)
										{
											num26 += 32;
											if (num26 > 255)
											{
												num26 = 255;
											}
											dataVT2[num22 + 70] = (byte)num26;
											dataVT2[num22 + 70 + 1] = 128;
										}
									}
									flag4 = true;
									num23 = 255;
								}
								dataVT2[num22] = (byte)num23;
								dataVT2[num22 + 1] = 128;
								flag2 = true;
							}
							flag3 = flag4;
							num21--;
							num22 -= 70;
						}
						if (flag2)
						{
							flag = true;
						}
					}
				}
				num18 += num12;
				num17++;
			}
			num16 += num12;
			num15 += 1225;
		}
		if (flag)
		{
			WriteChunkToFile(filePrefix, vFVoxelChunkData2);
		}
		vFVoxelChunkData2.ClearMem();
		vFVoxelChunkData.ClearMem();
	}

	private void WriteChunkToFile(string filePrefix, VFVoxelChunkData chunk)
	{
		byte[] dataVT = chunk.DataVT;
		int count = dataVT.Length;
		string path = filePrefix + "_x" + chunk.ChunkPosLod.x + "_y" + chunk.ChunkPosLod.y + "_z" + chunk.ChunkPosLod.z + "_" + chunk.ChunkPosLod.w + ".chnk";
		FileStream fileStream = File.Create(path);
		fileStream.Write(dataVT, 0, count);
		fileStream.Close();
	}

	private void OnChunkDataLoad(VFVoxelChunkData chunkData, byte[] chunkDataVT, bool bFromPool)
	{
		chunkData.SetDataVT(chunkDataVT, bFromPool);
		if (chunkData.IsHollow)
		{
			if (chunkData.DataVT[0] == 128)
			{
				VFVoxelWater.ExpandSurfaceChunkData(chunkData);
			}
			else
			{
				VFVoxelChunkData.ExpandHollowChunkData(chunkData);
			}
		}
	}

	public static void ReadRiverChunksList(ref Dictionary<IntVector4, List<string>> riverChunkFileList, string fileFilter = "water_x*.chnk", int fileX = -1, int fileZ = -1)
	{
		if (riverChunkFileList == null)
		{
			riverChunkFileList = new Dictionary<IntVector4, List<string>>();
		}
		string path = outputDir;
		if (!Directory.Exists(path))
		{
			return;
		}
		string[] files = Directory.GetFiles(path, fileFilter, SearchOption.AllDirectories);
		string[] separator = new string[5] { "_x", "_y", "_z", "_", ".chnk" };
		int num = ((fileX >= 0) ? (fileX * 192) : 0);
		int num2 = ((fileX >= 0) ? ((fileX + 1) * 192) : 576);
		int num3 = ((fileZ >= 0) ? (fileZ * 192) : 0);
		int num4 = ((fileZ >= 0) ? ((fileZ + 1) * 192) : 576);
		string[] array = files;
		foreach (string text in array)
		{
			string[] array2 = Path.GetFileName(text).Split(separator, StringSplitOptions.RemoveEmptyEntries);
			try
			{
				int num5 = Convert.ToInt32(array2[1]);
				int y_ = Convert.ToInt32(array2[2]);
				int num6 = Convert.ToInt32(array2[3]);
				int w_ = Convert.ToInt32(array2[4]);
				if (num5 >= num && num5 < num2 && num6 >= num3 && num6 < num4)
				{
					IntVector4 intVector = new IntVector4(num5, y_, num6, w_);
					if (riverChunkFileList.ContainsKey(intVector))
					{
						Debug.LogWarning("Warning in ReadChunkFileList: multiple same chunk at" + intVector);
						riverChunkFileList[intVector].Add(text);
					}
					else
					{
						riverChunkFileList[intVector] = new List<string>();
						riverChunkFileList[intVector].Add(text);
					}
				}
			}
			catch
			{
				Debug.LogWarning("Warning in reading river chunk files : Unresolved chunk file name" + text);
			}
		}
	}
}
