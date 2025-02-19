using System;
using System.Collections;
using System.Collections.Generic;
using System.Reflection;
using Pathfinding.Serialization;
using Pathfinding.Util;
using UnityEngine;

namespace Pathfinding;

[Serializable]
public class AstarData
{
	[NonSerialized]
	public NavMeshGraph navmesh;

	[NonSerialized]
	public GridGraph gridGraph;

	[NonSerialized]
	public LayerGridGraph layerGridGraph;

	[NonSerialized]
	public PointGraph pointGraph;

	[NonSerialized]
	public RecastGraph recastGraph;

	public Type[] graphTypes;

	[NonSerialized]
	public NavGraph[] graphs = new NavGraph[0];

	[NonSerialized]
	public UserConnection[] userConnections = new UserConnection[0];

	public bool hasBeenReverted;

	[SerializeField]
	private byte[] data;

	public uint dataChecksum;

	public byte[] data_backup;

	public TextAsset file_cachedStartup;

	public byte[] data_cachedStartup;

	[SerializeField]
	public bool cacheStartup;

	public AstarPath active => AstarPath.active;

	public byte[] GetData()
	{
		return data;
	}

	public void SetData(byte[] data, uint checksum)
	{
		this.data = data;
		dataChecksum = checksum;
	}

	public void Awake()
	{
		userConnections = new UserConnection[0];
		graphs = new NavGraph[0];
		if (cacheStartup && file_cachedStartup != null)
		{
			LoadFromCache();
		}
		else
		{
			DeserializeGraphs();
		}
	}

	public void UpdateShortcuts()
	{
		navmesh = (NavMeshGraph)FindGraphOfType(typeof(NavMeshGraph));
		gridGraph = (GridGraph)FindGraphOfType(typeof(GridGraph));
		layerGridGraph = (LayerGridGraph)FindGraphOfType(typeof(LayerGridGraph));
		pointGraph = (PointGraph)FindGraphOfType(typeof(PointGraph));
		recastGraph = (RecastGraph)FindGraphOfType(typeof(RecastGraph));
	}

	public void LoadFromCache()
	{
		AstarPath.active.BlockUntilPathQueueBlocked();
		if (file_cachedStartup != null)
		{
			byte[] bytes = file_cachedStartup.bytes;
			DeserializeGraphs(bytes);
			GraphModifier.TriggerEvent(GraphModifier.EventType.PostCacheLoad);
		}
		else
		{
			Debug.LogError("Can't load from cache since the cache is empty");
		}
	}

	public byte[] SerializeGraphs()
	{
		return SerializeGraphs(SerializeSettings.Settings);
	}

	public byte[] SerializeGraphs(SerializeSettings settings)
	{
		uint checksum;
		return SerializeGraphs(settings, out checksum);
	}

	public byte[] SerializeGraphs(SerializeSettings settings, out uint checksum)
	{
		AstarPath.active.BlockUntilPathQueueBlocked();
		AstarSerializer astarSerializer = new AstarSerializer(this, settings);
		astarSerializer.OpenSerialize();
		SerializeGraphsPart(astarSerializer);
		byte[] result = astarSerializer.CloseSerialize();
		checksum = astarSerializer.GetChecksum();
		return result;
	}

	public void SerializeGraphsPart(AstarSerializer sr)
	{
		sr.SerializeGraphs(graphs);
		sr.SerializeUserConnections(userConnections);
		sr.SerializeNodes();
		sr.SerializeExtraInfo();
	}

	public void DeserializeGraphs()
	{
		if (data != null)
		{
			DeserializeGraphs(data);
		}
	}

	private void ClearGraphs()
	{
		if (graphs == null)
		{
			return;
		}
		for (int i = 0; i < graphs.Length; i++)
		{
			if (graphs[i] != null)
			{
				graphs[i].OnDestroy();
			}
		}
		graphs = null;
		UpdateShortcuts();
	}

	public void OnDestroy()
	{
		ClearGraphs();
	}

	public void DeserializeGraphs(byte[] bytes)
	{
		AstarPath.active.BlockUntilPathQueueBlocked();
		try
		{
			if (bytes != null)
			{
				AstarSerializer astarSerializer = new AstarSerializer(this);
				if (astarSerializer.OpenDeserialize(bytes))
				{
					DeserializeGraphsPart(astarSerializer);
					astarSerializer.CloseDeserialize();
					UpdateShortcuts();
				}
				else
				{
					Debug.Log("Invalid data file (cannot read zip).\nThe data is either corrupt or it was saved using a 3.0.x or earlier version of the system");
				}
				active.VerifyIntegrity();
				return;
			}
			throw new ArgumentNullException("Bytes should not be null when passed to DeserializeGraphs");
		}
		catch (Exception ex)
		{
			Debug.LogWarning("Caught exception while deserializing data.\n" + ex);
			data_backup = bytes;
		}
	}

	public void DeserializeGraphsAdditive(byte[] bytes)
	{
		AstarPath.active.BlockUntilPathQueueBlocked();
		try
		{
			if (bytes != null)
			{
				AstarSerializer astarSerializer = new AstarSerializer(this);
				if (astarSerializer.OpenDeserialize(bytes))
				{
					DeserializeGraphsPartAdditive(astarSerializer);
					astarSerializer.CloseDeserialize();
				}
				else
				{
					Debug.Log("Invalid data file (cannot read zip).");
				}
				active.VerifyIntegrity();
				return;
			}
			throw new ArgumentNullException("Bytes should not be null when passed to DeserializeGraphs");
		}
		catch (Exception ex)
		{
			Debug.LogWarning("Caught exception while deserializing data.\n" + ex);
		}
	}

	public void DeserializeGraphsPart(AstarSerializer sr)
	{
		ClearGraphs();
		graphs = sr.DeserializeGraphs();
		if (graphs != null)
		{
			for (int i = 0; i < graphs.Length; i++)
			{
				if (graphs[i] != null)
				{
					graphs[i].graphIndex = (uint)i;
				}
			}
		}
		userConnections = sr.DeserializeUserConnections();
		sr.DeserializeExtraInfo();
		int j;
		for (j = 0; j < graphs.Length; j++)
		{
			if (graphs[j] != null)
			{
				graphs[j].GetNodes(delegate(GraphNode node)
				{
					node.GraphIndex = (uint)j;
					return true;
				});
			}
		}
		sr.PostDeserialization();
	}

	public void DeserializeGraphsPartAdditive(AstarSerializer sr)
	{
		if (graphs == null)
		{
			graphs = new NavGraph[0];
		}
		if (userConnections == null)
		{
			userConnections = new UserConnection[0];
		}
		List<NavGraph> list = new List<NavGraph>(graphs);
		list.AddRange(sr.DeserializeGraphs());
		graphs = list.ToArray();
		if (graphs != null)
		{
			for (int i = 0; i < graphs.Length; i++)
			{
				if (graphs[i] != null)
				{
					graphs[i].graphIndex = (uint)i;
				}
			}
		}
		List<UserConnection> list2 = new List<UserConnection>(userConnections);
		list2.AddRange(sr.DeserializeUserConnections());
		userConnections = list2.ToArray();
		sr.DeserializeNodes();
		int j;
		for (j = 0; j < graphs.Length; j++)
		{
			if (graphs[j] != null)
			{
				graphs[j].GetNodes(delegate(GraphNode node)
				{
					node.GraphIndex = (uint)j;
					return true;
				});
			}
		}
		sr.DeserializeExtraInfo();
		sr.PostDeserialization();
		for (int k = 0; k < graphs.Length; k++)
		{
			for (int l = k + 1; l < graphs.Length; l++)
			{
				if (graphs[k] != null && graphs[l] != null && graphs[k].guid == graphs[l].guid)
				{
					Debug.LogWarning("Guid Conflict when importing graphs additively. Imported graph will get a new Guid.\nThis message is (relatively) harmless.");
					graphs[k].guid = Pathfinding.Util.Guid.NewGuid();
					break;
				}
			}
		}
	}

	public void FindGraphTypes()
	{
		Assembly assembly = Assembly.GetAssembly(typeof(AstarPath));
		Type[] types = assembly.GetTypes();
		List<Type> list = new List<Type>();
		Type[] array = types;
		foreach (Type type in array)
		{
			for (Type baseType = type.BaseType; baseType != null; baseType = baseType.BaseType)
			{
				if (object.Equals(baseType, typeof(NavGraph)))
				{
					list.Add(type);
					break;
				}
			}
		}
		graphTypes = list.ToArray();
	}

	public Type GetGraphType(string type)
	{
		for (int i = 0; i < graphTypes.Length; i++)
		{
			if (graphTypes[i].Name == type)
			{
				return graphTypes[i];
			}
		}
		return null;
	}

	public NavGraph CreateGraph(string type)
	{
		Debug.Log("Creating Graph of type '" + type + "'");
		for (int i = 0; i < graphTypes.Length; i++)
		{
			if (graphTypes[i].Name == type)
			{
				return CreateGraph(graphTypes[i]);
			}
		}
		Debug.LogError("Graph type (" + type + ") wasn't found");
		return null;
	}

	public NavGraph CreateGraph(Type type)
	{
		NavGraph navGraph = Activator.CreateInstance(type) as NavGraph;
		navGraph.active = active;
		return navGraph;
	}

	public NavGraph AddGraph(string type)
	{
		NavGraph navGraph = null;
		for (int i = 0; i < graphTypes.Length; i++)
		{
			if (graphTypes[i].Name == type)
			{
				navGraph = CreateGraph(graphTypes[i]);
			}
		}
		if (navGraph == null)
		{
			Debug.LogError("No NavGraph of type '" + type + "' could be found");
			return null;
		}
		AddGraph(navGraph);
		return navGraph;
	}

	public NavGraph AddGraph(Type type)
	{
		NavGraph navGraph = null;
		for (int i = 0; i < graphTypes.Length; i++)
		{
			if (object.Equals(graphTypes[i], type))
			{
				navGraph = CreateGraph(graphTypes[i]);
			}
		}
		if (navGraph == null)
		{
			Debug.LogError(string.Concat("No NavGraph of type '", type, "' could be found, ", graphTypes.Length, " graph types are avaliable"));
			return null;
		}
		AddGraph(navGraph);
		return navGraph;
	}

	public void AddGraph(NavGraph graph)
	{
		AstarPath.active.BlockUntilPathQueueBlocked();
		for (int i = 0; i < graphs.Length; i++)
		{
			if (graphs[i] == null)
			{
				graphs[i] = graph;
				graph.active = active;
				graph.Awake();
				graph.graphIndex = (uint)i;
				UpdateShortcuts();
				return;
			}
		}
		if (graphs != null && (long)graphs.Length >= 255L)
		{
			throw new Exception("Graph Count Limit Reached. You cannot have more than " + 255u + " graphs. Some compiler directives can change this limit, e.g ASTAR_MORE_AREAS, look under the 'Optimizations' tab in the A* Inspector");
		}
		List<NavGraph> list = new List<NavGraph>(graphs);
		list.Add(graph);
		graphs = list.ToArray();
		UpdateShortcuts();
		graph.active = active;
		graph.Awake();
		graph.graphIndex = (uint)(graphs.Length - 1);
	}

	public bool RemoveGraph(NavGraph graph)
	{
		graph.SafeOnDestroy();
		int i;
		for (i = 0; i < graphs.Length && graphs[i] != graph; i++)
		{
		}
		if (i == graphs.Length)
		{
			return false;
		}
		graphs[i] = null;
		UpdateShortcuts();
		return true;
	}

	public static NavGraph GetGraph(GraphNode node)
	{
		if (node == null)
		{
			return null;
		}
		AstarPath astarPath = AstarPath.active;
		if (astarPath == null)
		{
			return null;
		}
		AstarData astarData = astarPath.astarData;
		if (astarData == null)
		{
			return null;
		}
		if (astarData.graphs == null)
		{
			return null;
		}
		uint graphIndex = node.GraphIndex;
		if (graphIndex >= astarData.graphs.Length)
		{
			return null;
		}
		return astarData.graphs[graphIndex];
	}

	public GraphNode GetNode(int graphIndex, int nodeIndex)
	{
		return GetNode(graphIndex, nodeIndex, graphs);
	}

	public GraphNode GetNode(int graphIndex, int nodeIndex, NavGraph[] graphs)
	{
		throw new NotImplementedException();
	}

	public NavGraph FindGraphOfType(Type type)
	{
		if (graphs != null)
		{
			for (int i = 0; i < graphs.Length; i++)
			{
				if (graphs[i] != null && object.Equals(graphs[i].GetType(), type))
				{
					return graphs[i];
				}
			}
		}
		return null;
	}

	public IEnumerable FindGraphsOfType(Type type)
	{
		if (graphs == null)
		{
			yield break;
		}
		for (int i = 0; i < graphs.Length; i++)
		{
			if (graphs[i] != null && object.Equals(graphs[i].GetType(), type))
			{
				yield return graphs[i];
			}
		}
	}

	public IEnumerable GetUpdateableGraphs()
	{
		if (graphs == null)
		{
			yield break;
		}
		for (int i = 0; i < graphs.Length; i++)
		{
			if (graphs[i] != null && graphs[i] is IUpdatableGraph)
			{
				yield return graphs[i];
			}
		}
	}

	public IEnumerable GetRaycastableGraphs()
	{
		if (graphs == null)
		{
			yield break;
		}
		for (int i = 0; i < graphs.Length; i++)
		{
			if (graphs[i] != null && graphs[i] is IRaycastableGraph)
			{
				yield return graphs[i];
			}
		}
	}

	public int GetGraphIndex(NavGraph graph)
	{
		if (graph == null)
		{
			throw new ArgumentNullException("graph");
		}
		if (graphs != null)
		{
			for (int i = 0; i < graphs.Length; i++)
			{
				if (graph == graphs[i])
				{
					return i;
				}
			}
		}
		Debug.LogError("Graph doesn't exist");
		return -1;
	}

	public int GuidToIndex(Pathfinding.Util.Guid guid)
	{
		if (graphs == null)
		{
			return -1;
		}
		for (int i = 0; i < graphs.Length; i++)
		{
			if (graphs[i] != null && graphs[i].guid == guid)
			{
				return i;
			}
		}
		return -1;
	}

	public NavGraph GuidToGraph(Pathfinding.Util.Guid guid)
	{
		if (graphs == null)
		{
			return null;
		}
		for (int i = 0; i < graphs.Length; i++)
		{
			if (graphs[i] != null && graphs[i].guid == guid)
			{
				return graphs[i];
			}
		}
		return null;
	}
}
