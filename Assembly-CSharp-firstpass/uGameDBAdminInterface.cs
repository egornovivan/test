using System.Collections;
using System.Collections.Generic;
using Jboy;
using uGameDB;
using UnityEngine;

public class uGameDBAdminInterface : MonoBehaviour
{
	private class Entry
	{
		public Bucket Bucket;

		public string Key;

		public object Value;

		public bool Expanded;
	}

	private class BucketData
	{
		public Bucket Bucket;

		public List<Entry> Entries;
	}

	private const string InfoText = "This is a simple example of how you can implement a Riak admin interface in Unity using uGameDB.\n\nThe uGameDBAdminInterface component draws this GUI and allows you to list and see the full contents of the database. You are also able to remove specific entries and clear (remove) whole buckets. Note that this admin tool is not recommended for large-scale databases as the amount of data that is retrieved can be huge.\n\nTo the right you can see some information about the uGameDB client. This is the same data that you get on-screen if you use the uGameDBStatisticsGUI utility script.\n\nBelow you can see the list of buckets and keys in the database. Please refer to the uGameDBAdminInterface.cs file for more information.";

	private const int WindowMargin = 10;

	public bool ShowInfoHeader = true;

	public int JsonIndentSize = 25;

	private bool _showInfo = true;

	private Vector2 _scrollPosInfo;

	private Vector2 _scrollPosData;

	private List<BucketData> _buckets = new List<BucketData>();

	private bool _enableRefresh = true;

	public void Start()
	{
		StartCoroutine(RefreshList());
	}

	private IEnumerator RefreshList()
	{
		if (_enableRefresh)
		{
			_enableRefresh = false;
			while (!Database.isConnected)
			{
				yield return new WaitForSeconds(0.5f);
			}
			yield return StartCoroutine(RefreshBuckets());
			_enableRefresh = true;
		}
	}

	private IEnumerator RefreshBuckets()
	{
		List<BucketData> newBuckets = new List<BucketData>();
		GetBucketsRequest getBucketsReq = Bucket.GetBuckets();
		yield return getBucketsReq.WaitUntilDone();
		if (getBucketsReq.isSuccessful)
		{
			foreach (Bucket bucket in getBucketsReq.GetBucketEnumerable())
			{
				BucketData bucketData = new BucketData
				{
					Bucket = bucket
				};
				GetKeysRequest getKeysReq = bucket.GetKeys();
				yield return getKeysReq.WaitUntilDone();
				if (!getKeysReq.isSuccessful)
				{
					continue;
				}
				bucketData.Entries = new List<Entry>();
				foreach (string key in getKeysReq.GetKeyEnumerable())
				{
					Entry entry = new Entry
					{
						Bucket = bucket,
						Key = key,
						Value = "[unknown data]"
					};
					GetRequest getReq = bucket.Get(key);
					yield return getReq.WaitUntilDone();
					if (getReq.isSuccessful)
					{
						if (getReq.GetEncoding() == Encoding.Json)
						{
							entry.Value = getReq.GetValue<JsonTree>();
						}
						else
						{
							entry.Value = getReq.GetValueRaw();
						}
					}
					bucketData.Entries.Add(entry);
				}
				bucketData.Entries.Sort((Entry a, Entry b) => a.Key.CompareTo(b.Key));
				newBuckets.Add(bucketData);
			}
			newBuckets.Sort((BucketData a, BucketData b) => a.Bucket.name.CompareTo(b.Bucket.name));
		}
		_buckets = newBuckets;
	}

	private IEnumerator Remove(Bucket bucket, string key)
	{
		RemoveRequest removeRequest = bucket.Remove(key);
		yield return removeRequest.WaitUntilDone();
		if (removeRequest.hasFailed)
		{
			Debug.LogError(string.Concat("Unable to remove '", removeRequest.key, "' from ", removeRequest.bucket, ". ", removeRequest.GetErrorString()));
		}
		StartCoroutine(RefreshList());
	}

	private IEnumerator ClearBucket(Bucket bucket)
	{
		GetKeysRequest getKeysRequest = bucket.GetKeys();
		yield return getKeysRequest.WaitUntilDone();
		if (getKeysRequest.hasFailed)
		{
			Debug.LogError(string.Concat("Unable to clear ", bucket, ". ", getKeysRequest.GetErrorString()));
			yield break;
		}
		List<RemoveRequest> removeRequests = new List<RemoveRequest>();
		foreach (string key in getKeysRequest.GetKeyEnumerable())
		{
			removeRequests.Add(bucket.Remove(key));
		}
		foreach (RemoveRequest removeRequest in removeRequests)
		{
			yield return removeRequest.WaitUntilDone();
			if (removeRequest.hasFailed)
			{
				Debug.LogError(string.Concat("Unable to remove '", removeRequest.key, "' from ", removeRequest.bucket, ". ", removeRequest.GetErrorString()));
			}
		}
		StartCoroutine(RefreshList());
	}

	public void OnGUI()
	{
		GUI.depth = 0;
		GUILayout.BeginArea(new Rect(10f, 10f, Screen.width - 20, Screen.height - 20));
		DrawAdminInterfaceGUI();
		GUILayout.EndArea();
	}

	private void DrawAdminInterfaceGUI()
	{
		GUILayout.BeginVertical();
		if (ShowInfoHeader)
		{
			DrawInfoArea();
		}
		DrawAdminArea();
		GUILayout.EndVertical();
	}

	private void DrawInfoArea()
	{
		GUILayout.BeginVertical();
		if (GUILayout.Button("Toggle Info"))
		{
			_showInfo = !_showInfo;
		}
		if (_showInfo)
		{
			GUILayout.BeginHorizontal("Box", GUILayout.Height(1f));
			_scrollPosInfo = GUILayout.BeginScrollView(_scrollPosInfo);
			GUILayout.TextArea("This is a simple example of how you can implement a Riak admin interface in Unity using uGameDB.\n\nThe uGameDBAdminInterface component draws this GUI and allows you to list and see the full contents of the database. You are also able to remove specific entries and clear (remove) whole buckets. Note that this admin tool is not recommended for large-scale databases as the amount of data that is retrieved can be huge.\n\nTo the right you can see some information about the uGameDB client. This is the same data that you get on-screen if you use the uGameDBStatisticsGUI utility script.\n\nBelow you can see the list of buckets and keys in the database. Please refer to the uGameDBAdminInterface.cs file for more information.", "Label");
			GUILayout.EndScrollView();
			uGameDBStatisticsGUI.DrawStatisticsBox(250);
			GUILayout.EndHorizontal();
		}
		GUILayout.EndVertical();
	}

	private void DrawAdminArea()
	{
		GUILayout.BeginVertical();
		DrawDataList();
		if (GUILayout.Button("Refresh"))
		{
			StartCoroutine(RefreshList());
		}
		GUILayout.EndVertical();
	}

	private void DrawDataList()
	{
		GUILayout.BeginVertical("Box");
		GUILayout.Label("Database Contents");
		if (_buckets.Count == 0)
		{
			GUILayout.BeginVertical("Box");
			GUILayout.FlexibleSpace();
			GUILayout.BeginHorizontal();
			GUILayout.FlexibleSpace();
			GUILayout.Label("Database is empty");
			GUILayout.FlexibleSpace();
			GUILayout.EndHorizontal();
			GUILayout.FlexibleSpace();
			GUILayout.EndVertical();
		}
		else
		{
			_scrollPosData = GUILayout.BeginScrollView(_scrollPosData);
			foreach (BucketData bucket in _buckets)
			{
				DrawBucket(bucket);
			}
			GUILayout.FlexibleSpace();
			GUILayout.EndScrollView();
		}
		GUILayout.EndVertical();
	}

	private void DrawBucket(BucketData b)
	{
		GUILayout.BeginHorizontal("box");
		GUILayout.BeginVertical();
		GUILayout.BeginHorizontal();
		GUILayout.Label("Bucket: " + b.Bucket.name);
		GUILayout.FlexibleSpace();
		if (GUILayout.Button("Clear"))
		{
			StartCoroutine(ClearBucket(b.Bucket));
		}
		GUILayout.EndHorizontal();
		if (b.Entries != null)
		{
			foreach (Entry entry in b.Entries)
			{
				DrawKeyValue(entry);
			}
		}
		else
		{
			DrawUnknownKeyValue();
		}
		GUILayout.EndVertical();
		GUILayout.EndHorizontal();
	}

	private void DrawKeyValue(Entry entry)
	{
		GUILayout.BeginHorizontal("box");
		GUILayout.Label("Key: " + entry.Key, GUILayout.Width((float)Screen.width / 4f));
		if (entry.Value is JsonTree)
		{
			if (GUILayout.Button((!entry.Expanded) ? "+" : "-"))
			{
				entry.Expanded = !entry.Expanded;
			}
			if (entry.Expanded)
			{
				DrawJsonDataValueExpanded((JsonTree)entry.Value);
			}
			else
			{
				DrawJsonDataValueCollapsed((JsonTree)entry.Value);
			}
		}
		else
		{
			GUILayout.Label("Value: " + entry.Value);
		}
		GUILayout.FlexibleSpace();
		if (GUILayout.Button("Remove"))
		{
			StartCoroutine(Remove(entry.Bucket, entry.Key));
		}
		GUILayout.EndHorizontal();
	}

	private void DrawJsonDataValueCollapsed(JsonTree jsonTree)
	{
		GUILayout.BeginHorizontal();
		GUILayout.TextField(jsonTree.ToString(), "Label");
		GUILayout.EndHorizontal();
	}

	private void DrawJsonDataValueExpanded(JsonTree jsonTree)
	{
		if (jsonTree.IsObject)
		{
			GUILayout.BeginVertical();
			GUILayout.Label("{");
			foreach (JsonObject.JsonProperty item in jsonTree.AsObject)
			{
				GUILayout.BeginHorizontal();
				GUILayout.Space(JsonIndentSize);
				GUILayout.Label(item.Name);
				GUILayout.Label(":");
				DrawJsonDataValueExpanded(item.Value);
				GUILayout.FlexibleSpace();
				GUILayout.EndHorizontal();
			}
			GUILayout.Label("}");
			GUILayout.EndVertical();
		}
		else if (jsonTree.IsArray)
		{
			GUILayout.BeginVertical();
			GUILayout.Label("[");
			foreach (JsonTree item2 in (IEnumerable<JsonTree>)jsonTree.AsArray)
			{
				GUILayout.BeginHorizontal();
				GUILayout.Space(JsonIndentSize);
				DrawJsonDataValueExpanded(item2);
				GUILayout.FlexibleSpace();
				GUILayout.EndHorizontal();
			}
			GUILayout.Label("]");
			GUILayout.EndVertical();
		}
		else
		{
			GUILayout.BeginHorizontal();
			GUILayout.TextField(jsonTree.ToString(), "Label");
			GUILayout.FlexibleSpace();
			GUILayout.EndHorizontal();
		}
	}

	private void DrawUnknownKeyValue()
	{
		GUILayout.BeginHorizontal("box");
		GUILayout.Label("[unknown keys/values]");
		GUILayout.FlexibleSpace();
		GUILayout.EndHorizontal();
	}
}
