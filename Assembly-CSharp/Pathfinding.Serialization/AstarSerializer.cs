using System;
using System.IO;
using System.Text;
using Pathfinding.Ionic.Zip;
using Pathfinding.Serialization.JsonFx;
using Pathfinding.Util;
using UnityEngine;

namespace Pathfinding.Serialization;

public class AstarSerializer
{
	private const string binaryExt = ".binary";

	private const string jsonExt = ".json";

	private AstarData data;

	public JsonWriterSettings writerSettings;

	public JsonReaderSettings readerSettings;

	private ZipFile zip;

	private MemoryStream str;

	private GraphMeta meta;

	private SerializeSettings settings;

	private NavGraph[] graphs;

	private uint checksum = uint.MaxValue;

	private UTF8Encoding encoding = new UTF8Encoding();

	private static StringBuilder _stringBuilder = new StringBuilder();

	public AstarSerializer(AstarData data)
	{
		this.data = data;
		settings = SerializeSettings.Settings;
	}

	public AstarSerializer(AstarData data, SerializeSettings settings)
	{
		this.data = data;
		this.settings = settings;
	}

	private static StringBuilder GetStringBuilder()
	{
		_stringBuilder.Length = 0;
		return _stringBuilder;
	}

	public void AddChecksum(byte[] bytes)
	{
		checksum = Checksum.GetChecksum(bytes, checksum);
	}

	public uint GetChecksum()
	{
		return checksum;
	}

	public void OpenSerialize()
	{
		zip = new ZipFile();
		zip.AlternateEncoding = Encoding.UTF8;
		zip.AlternateEncodingUsage = ZipOption.Always;
		writerSettings = new JsonWriterSettings();
		writerSettings.AddTypeConverter(new VectorConverter());
		writerSettings.AddTypeConverter(new BoundsConverter());
		writerSettings.AddTypeConverter(new LayerMaskConverter());
		writerSettings.AddTypeConverter(new MatrixConverter());
		writerSettings.AddTypeConverter(new GuidConverter());
		writerSettings.AddTypeConverter(new UnityObjectConverter());
		writerSettings.PrettyPrint = settings.prettyPrint;
		meta = new GraphMeta();
	}

	public byte[] CloseSerialize()
	{
		byte[] array = SerializeMeta();
		AddChecksum(array);
		zip.AddEntry("meta.json", array);
		MemoryStream memoryStream = new MemoryStream();
		zip.Save(memoryStream);
		array = memoryStream.ToArray();
		memoryStream.Dispose();
		zip.Dispose();
		zip = null;
		return array;
	}

	public void SerializeGraphs(NavGraph[] _graphs)
	{
		if (graphs != null)
		{
			throw new InvalidOperationException("Cannot serialize graphs multiple times.");
		}
		graphs = _graphs;
		if (zip == null)
		{
			throw new NullReferenceException("You must not call CloseSerialize before a call to this function");
		}
		if (graphs == null)
		{
			graphs = new NavGraph[0];
		}
		for (int i = 0; i < graphs.Length; i++)
		{
			if (graphs[i] != null)
			{
				byte[] array = Serialize(graphs[i]);
				AddChecksum(array);
				zip.AddEntry("graph" + i + ".json", array);
			}
		}
	}

	public void SerializeUserConnections(UserConnection[] conns)
	{
		if (conns == null)
		{
			conns = new UserConnection[0];
		}
		StringBuilder stringBuilder = GetStringBuilder();
		JsonWriter jsonWriter = new JsonWriter(stringBuilder, writerSettings);
		jsonWriter.Write(conns);
		byte[] bytes = encoding.GetBytes(stringBuilder.ToString());
		stringBuilder = null;
		if (bytes.Length > 2)
		{
			AddChecksum(bytes);
			zip.AddEntry("connections.json", bytes);
		}
	}

	private byte[] SerializeMeta()
	{
		meta.version = AstarPath.Version;
		meta.graphs = data.graphs.Length;
		meta.guids = new string[data.graphs.Length];
		meta.typeNames = new string[data.graphs.Length];
		meta.nodeCounts = new int[data.graphs.Length];
		for (int i = 0; i < data.graphs.Length; i++)
		{
			if (data.graphs[i] != null)
			{
				meta.guids[i] = data.graphs[i].guid.ToString();
				meta.typeNames[i] = data.graphs[i].GetType().FullName;
			}
		}
		StringBuilder stringBuilder = GetStringBuilder();
		JsonWriter jsonWriter = new JsonWriter(stringBuilder, writerSettings);
		jsonWriter.Write(meta);
		return encoding.GetBytes(stringBuilder.ToString());
	}

	public byte[] Serialize(NavGraph graph)
	{
		StringBuilder stringBuilder = GetStringBuilder();
		JsonWriter jsonWriter = new JsonWriter(stringBuilder, writerSettings);
		jsonWriter.Write(graph);
		return encoding.GetBytes(stringBuilder.ToString());
	}

	public void SerializeNodes()
	{
		if (settings.nodes)
		{
			if (graphs == null)
			{
				throw new InvalidOperationException("Cannot serialize nodes with no serialized graphs (call SerializeGraphs first)");
			}
			for (int i = 0; i < graphs.Length; i++)
			{
				byte[] array = SerializeNodes(i);
				AddChecksum(array);
				zip.AddEntry("graph" + i + "_nodes.binary", array);
			}
			for (int j = 0; j < graphs.Length; j++)
			{
				byte[] array2 = SerializeNodeConnections(j);
				AddChecksum(array2);
				zip.AddEntry("graph" + j + "_conns.binary", array2);
			}
		}
	}

	private byte[] SerializeNodes(int index)
	{
		return new byte[0];
	}

	public void SerializeExtraInfo()
	{
		if (!settings.nodes)
		{
			return;
		}
		int totCount = 0;
		for (int i = 0; i < graphs.Length; i++)
		{
			if (graphs[i] == null)
			{
				continue;
			}
			graphs[i].GetNodes(delegate(GraphNode node)
			{
				totCount = Math.Max(node.NodeIndex, totCount);
				if (node.NodeIndex == -1)
				{
					Debug.LogError("Graph contains destroyed nodes. This is a bug.");
				}
				return true;
			});
		}
		MemoryStream memoryStream = new MemoryStream();
		BinaryWriter wr = new BinaryWriter(memoryStream);
		wr.Write(totCount);
		int c = 0;
		for (int j = 0; j < graphs.Length; j++)
		{
			if (graphs[j] != null)
			{
				graphs[j].GetNodes(delegate(GraphNode node)
				{
					c = Math.Max(node.NodeIndex, c);
					wr.Write(node.NodeIndex);
					return true;
				});
			}
		}
		if (c != totCount)
		{
			throw new Exception("Some graphs are not consistent in their GetNodes calls, sequential calls give different results.");
		}
		byte[] array = memoryStream.ToArray();
		wr.Close();
		AddChecksum(array);
		zip.AddEntry("graph_references.binary", array);
		for (int k = 0; k < graphs.Length; k++)
		{
			if (graphs[k] != null)
			{
				MemoryStream memoryStream2 = new MemoryStream();
				BinaryWriter binaryWriter = new BinaryWriter(memoryStream2);
				GraphSerializationContext ctx = new GraphSerializationContext(binaryWriter);
				graphs[k].SerializeExtraInfo(ctx);
				byte[] array2 = memoryStream2.ToArray();
				binaryWriter.Close();
				AddChecksum(array2);
				zip.AddEntry("graph" + k + "_extra.binary", array2);
				memoryStream2 = new MemoryStream();
				binaryWriter = new BinaryWriter(memoryStream2);
				ctx = new GraphSerializationContext(binaryWriter);
				graphs[k].GetNodes(delegate(GraphNode node)
				{
					node.SerializeReferences(ctx);
					return true;
				});
				binaryWriter.Close();
				array2 = memoryStream2.ToArray();
				AddChecksum(array2);
				zip.AddEntry("graph" + k + "_references.binary", array2);
			}
		}
	}

	private byte[] SerializeNodeConnections(int index)
	{
		return new byte[0];
	}

	public void SerializeEditorSettings(GraphEditorBase[] editors)
	{
		if (editors == null || !settings.editorSettings)
		{
			return;
		}
		for (int i = 0; i < editors.Length && editors[i] != null; i++)
		{
			StringBuilder stringBuilder = GetStringBuilder();
			JsonWriter jsonWriter = new JsonWriter(stringBuilder, writerSettings);
			jsonWriter.Write(editors[i]);
			byte[] bytes = encoding.GetBytes(stringBuilder.ToString());
			if (bytes.Length > 2)
			{
				AddChecksum(bytes);
				zip.AddEntry("graph" + i + "_editor.json", bytes);
			}
		}
	}

	public bool OpenDeserialize(byte[] bytes)
	{
		readerSettings = new JsonReaderSettings();
		readerSettings.AddTypeConverter(new VectorConverter());
		readerSettings.AddTypeConverter(new BoundsConverter());
		readerSettings.AddTypeConverter(new LayerMaskConverter());
		readerSettings.AddTypeConverter(new MatrixConverter());
		readerSettings.AddTypeConverter(new GuidConverter());
		readerSettings.AddTypeConverter(new UnityObjectConverter());
		str = new MemoryStream();
		str.Write(bytes, 0, bytes.Length);
		str.Position = 0L;
		try
		{
			zip = ZipFile.Read(str);
		}
		catch (Exception ex)
		{
			Debug.LogWarning("Caught exception when loading from zip\n" + ex);
			str.Dispose();
			return false;
		}
		meta = DeserializeMeta(zip["meta.json"]);
		if (meta.version > AstarPath.Version)
		{
			Debug.LogWarning(string.Concat("Trying to load data from a newer version of the A* Pathfinding Project\nCurrent version: ", AstarPath.Version, " Data version: ", meta.version));
		}
		else if (meta.version < AstarPath.Version)
		{
			Debug.LogWarning(string.Concat("Trying to load data from an older version of the A* Pathfinding Project\nCurrent version: ", AstarPath.Version, " Data version: ", meta.version, "\nThis is usually fine, it just means you have upgraded to a new version.\nHowever node data (not settings) can get corrupted between versions, so it is recommendedto recalculate any caches (those for faster startup) and resave any files. Even if it seems to load fine, it might cause subtle bugs.\n"));
		}
		return true;
	}

	public void CloseDeserialize()
	{
		str.Dispose();
		zip.Dispose();
		zip = null;
		str = null;
	}

	public NavGraph[] DeserializeGraphs()
	{
		graphs = new NavGraph[meta.graphs];
		int num = 0;
		for (int i = 0; i < meta.graphs; i++)
		{
			Type graphType = meta.GetGraphType(i);
			if (!object.Equals(graphType, null))
			{
				num++;
				ZipEntry zipEntry = zip["graph" + i + ".json"];
				if (zipEntry == null)
				{
					throw new FileNotFoundException("Could not find data for graph " + i + " in zip. Entry 'graph+" + i + ".json' does not exist");
				}
				NavGraph obj = data.CreateGraph(graphType);
				string @string = GetString(zipEntry);
				JsonReader jsonReader = new JsonReader(@string, readerSettings);
				jsonReader.PopulateObject(ref obj);
				graphs[i] = obj;
				if (graphs[i].guid.ToString() != meta.guids[i])
				{
					throw new Exception("Guid in graph file not equal to guid defined in meta file. Have you edited the data manually?\n" + graphs[i].guid.ToString() + " != " + meta.guids[i]);
				}
			}
		}
		NavGraph[] array = new NavGraph[num];
		num = 0;
		for (int j = 0; j < graphs.Length; j++)
		{
			if (graphs[j] != null)
			{
				array[num] = graphs[j];
				num++;
			}
		}
		graphs = array;
		return graphs;
	}

	public UserConnection[] DeserializeUserConnections()
	{
		ZipEntry zipEntry = zip["connections.json"];
		if (zipEntry == null)
		{
			return new UserConnection[0];
		}
		string @string = GetString(zipEntry);
		JsonReader jsonReader = new JsonReader(@string, readerSettings);
		return (UserConnection[])jsonReader.Deserialize(typeof(UserConnection[]));
	}

	public void DeserializeNodes()
	{
		for (int i = 0; i < graphs.Length; i++)
		{
			if (graphs[i] == null || zip.ContainsEntry("graph" + i + "_nodes.binary"))
			{
			}
		}
		for (int j = 0; j < graphs.Length; j++)
		{
			if (graphs[j] != null)
			{
				ZipEntry zipEntry = zip["graph" + j + "_nodes.binary"];
				if (zipEntry != null)
				{
					MemoryStream memoryStream = new MemoryStream();
					zipEntry.Extract(memoryStream);
					memoryStream.Position = 0L;
					BinaryReader reader = new BinaryReader(memoryStream);
					DeserializeNodes(j, reader);
				}
			}
		}
		for (int k = 0; k < graphs.Length; k++)
		{
			if (graphs[k] != null)
			{
				ZipEntry zipEntry2 = zip["graph" + k + "_conns.binary"];
				if (zipEntry2 != null)
				{
					MemoryStream memoryStream2 = new MemoryStream();
					zipEntry2.Extract(memoryStream2);
					memoryStream2.Position = 0L;
					BinaryReader reader2 = new BinaryReader(memoryStream2);
					DeserializeNodeConnections(k, reader2);
				}
			}
		}
	}

	public void DeserializeExtraInfo()
	{
		bool flag = false;
		for (int i = 0; i < graphs.Length; i++)
		{
			ZipEntry zipEntry = zip["graph" + i + "_extra.binary"];
			if (zipEntry != null)
			{
				flag = true;
				MemoryStream memoryStream = new MemoryStream();
				zipEntry.Extract(memoryStream);
				memoryStream.Seek(0L, SeekOrigin.Begin);
				BinaryReader reader = new BinaryReader(memoryStream);
				GraphSerializationContext ctx = new GraphSerializationContext(reader, null, i);
				graphs[i].DeserializeExtraInfo(ctx);
			}
		}
		if (!flag)
		{
			return;
		}
		int totCount = 0;
		for (int j = 0; j < graphs.Length; j++)
		{
			if (graphs[j] == null)
			{
				continue;
			}
			graphs[j].GetNodes(delegate(GraphNode node)
			{
				totCount = Math.Max(node.NodeIndex, totCount);
				if (node.NodeIndex == -1)
				{
					Debug.LogError("Graph contains destroyed nodes. This is a bug.");
				}
				return true;
			});
		}
		ZipEntry zipEntry2 = zip["graph_references.binary"];
		if (zipEntry2 == null)
		{
			throw new Exception("Node references not found in the data. Was this loaded from an older version of the A* Pathfinding Project?");
		}
		MemoryStream memoryStream2 = new MemoryStream();
		zipEntry2.Extract(memoryStream2);
		memoryStream2.Seek(0L, SeekOrigin.Begin);
		BinaryReader reader2 = new BinaryReader(memoryStream2);
		int num = reader2.ReadInt32();
		GraphNode[] int2Node = new GraphNode[num + 1];
		try
		{
			for (int k = 0; k < graphs.Length; k++)
			{
				if (graphs[k] != null)
				{
					graphs[k].GetNodes(delegate(GraphNode node)
					{
						int2Node[reader2.ReadInt32()] = node;
						return true;
					});
				}
			}
		}
		catch (Exception innerException)
		{
			throw new Exception("Some graph(s) has thrown an exception during GetNodes, or some graph(s) have deserialized more or fewer nodes than were serialized", innerException);
		}
		reader2.Close();
		for (int l = 0; l < graphs.Length; l++)
		{
			if (graphs[l] != null)
			{
				zipEntry2 = zip["graph" + l + "_references.binary"];
				if (zipEntry2 == null)
				{
					throw new Exception("Node references for graph " + l + " not found in the data. Was this loaded from an older version of the A* Pathfinding Project?");
				}
				memoryStream2 = new MemoryStream();
				zipEntry2.Extract(memoryStream2);
				memoryStream2.Seek(0L, SeekOrigin.Begin);
				reader2 = new BinaryReader(memoryStream2);
				GraphSerializationContext ctx2 = new GraphSerializationContext(reader2, int2Node, l);
				graphs[l].GetNodes(delegate(GraphNode node)
				{
					node.DeserializeReferences(ctx2);
					return true;
				});
			}
		}
	}

	public void PostDeserialization()
	{
		for (int i = 0; i < graphs.Length; i++)
		{
			if (graphs[i] != null)
			{
				graphs[i].PostDeserialization();
			}
		}
	}

	private void DeserializeNodes(int index, BinaryReader reader)
	{
	}

	private void DeserializeNodeConnections(int index, BinaryReader reader)
	{
	}

	public void DeserializeEditorSettings(GraphEditorBase[] graphEditors)
	{
		if (graphEditors == null)
		{
			return;
		}
		for (int i = 0; i < graphEditors.Length; i++)
		{
			if (graphEditors[i] == null)
			{
				continue;
			}
			for (int j = 0; j < graphs.Length; j++)
			{
				if (graphs[j] != null && graphEditors[i].target == graphs[j])
				{
					ZipEntry zipEntry = zip["graph" + j + "_editor.json"];
					if (zipEntry != null)
					{
						string @string = GetString(zipEntry);
						JsonReader jsonReader = new JsonReader(@string, readerSettings);
						GraphEditorBase obj = graphEditors[i];
						jsonReader.PopulateObject(ref obj);
						graphEditors[i] = obj;
						break;
					}
				}
			}
		}
	}

	private string GetString(ZipEntry entry)
	{
		MemoryStream memoryStream = new MemoryStream();
		entry.Extract(memoryStream);
		memoryStream.Position = 0L;
		StreamReader streamReader = new StreamReader(memoryStream);
		string result = streamReader.ReadToEnd();
		memoryStream.Position = 0L;
		streamReader.Dispose();
		return result;
	}

	private GraphMeta DeserializeMeta(ZipEntry entry)
	{
		if (entry == null)
		{
			throw new Exception("No metadata found in serialized data.");
		}
		string @string = GetString(entry);
		JsonReader jsonReader = new JsonReader(@string, readerSettings);
		return (GraphMeta)jsonReader.Deserialize(typeof(GraphMeta));
	}

	public static void SaveToFile(string path, byte[] data)
	{
		using FileStream fileStream = new FileStream(path, FileMode.Create);
		fileStream.Write(data, 0, data.Length);
	}

	public static byte[] LoadFromFile(string path)
	{
		using FileStream fileStream = new FileStream(path, FileMode.Open);
		byte[] array = new byte[(int)fileStream.Length];
		fileStream.Read(array, 0, (int)fileStream.Length);
		return array;
	}
}
