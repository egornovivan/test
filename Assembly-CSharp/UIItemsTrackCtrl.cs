using System;
using System.Collections;
using System.Collections.Generic;
using ItemAsset;
using Pathea;
using UnityEngine;

public class UIItemsTrackCtrl : UIBaseWnd
{
	public enum TrackType
	{
		Script,
		ISO
	}

	public class ItemTrackData
	{
		public string itemName;

		public int itemCount;

		public int scriptID;

		public Dictionary<int, int> costDic = new Dictionary<int, int>(2);

		public TrackType type { get; private set; }

		public ItemTrackData(TrackType type)
		{
			this.type = type;
		}
	}

	private const string complateTitleFormat = "{0} X{1} [{2}]";

	private const string complateTitleColor = "[C8C800]{0}[-]";

	private const string complateCountColor = "[B4B4B4]{0} {1}[-]\n";

	private const string uncompletedCountColor = "[FFFFFF]{0} {1}[-]\n";

	private const string countFormat = "{0}/{1}";

	[SerializeField]
	private UIMissionTree itemsTree;

	[SerializeField]
	private UIDraggablePanel dragPanel;

	private Dictionary<UIMissionNode, ItemTrackData> _nodeDataDic = new Dictionary<UIMissionNode, ItemTrackData>(4);

	private Coroutine _coroutine;

	public Action<int, bool> ScriptTrackChanged;

	private PlayerPackage _playerPackage;

	private PlayerPackage playerPackage
	{
		get
		{
			if (_playerPackage == null)
			{
				PlayerPackageCmpt cmpt = PeSingleton<PeCreature>.Instance.mainPlayer.GetCmpt<PlayerPackageCmpt>();
				_playerPackage = cmpt.package;
			}
			return _playerPackage;
		}
	}

	private void OnEnable()
	{
		_coroutine = StartCoroutine(UpdateItemsTrack());
	}

	private void OnDisable()
	{
		if (_coroutine != null)
		{
			StopCoroutine(_coroutine);
		}
	}

	public bool ContainsScript(int scriptID)
	{
		foreach (ItemTrackData value in _nodeDataDic.Values)
		{
			if (value.scriptID == scriptID)
			{
				return true;
			}
		}
		return false;
	}

	public bool ContainsIso(string isoName)
	{
		foreach (ItemTrackData value in _nodeDataDic.Values)
		{
			if (value.type == TrackType.ISO && value.itemName == isoName)
			{
				return true;
			}
		}
		return false;
	}

	public void AddIso(string isoName, Dictionary<int, int> m_Cost)
	{
		if (ContainsIso(isoName))
		{
			return;
		}
		ItemTrackData itemTrackData = new ItemTrackData(TrackType.ISO);
		itemTrackData.itemName = isoName;
		itemTrackData.itemCount = 1;
		foreach (KeyValuePair<int, int> item in m_Cost)
		{
			if (item.Value > 0)
			{
				itemTrackData.costDic.Add(item.Key, item.Value);
			}
		}
		AddTrackData(itemTrackData);
	}

	public void RemoveIso(string isoName)
	{
		foreach (KeyValuePair<UIMissionNode, ItemTrackData> item in _nodeDataDic)
		{
			if (item.Value.type == TrackType.ISO && item.Value.itemName == isoName)
			{
				DeleteNode(item.Key);
				break;
			}
		}
	}

	public void UpdateOrAddScript(Replicator.Formula ms, int multiple)
	{
		if (ms == null)
		{
			return;
		}
		ItemTrackData itemTrackData = null;
		foreach (ItemTrackData value in _nodeDataDic.Values)
		{
			if (value.type == TrackType.Script && value.scriptID == ms.id)
			{
				itemTrackData = value;
				break;
			}
		}
		bool flag = false;
		if (itemTrackData == null)
		{
			itemTrackData = new ItemTrackData(TrackType.Script);
			itemTrackData.scriptID = ms.id;
			itemTrackData.itemName = ItemProto.GetName(ms.productItemId);
			flag = true;
		}
		else
		{
			itemTrackData.costDic.Clear();
		}
		itemTrackData.itemCount = multiple * ms.m_productItemCount;
		foreach (Replicator.Formula.Material material in ms.materials)
		{
			itemTrackData.costDic.Add(material.itemId, material.itemCount * multiple);
		}
		if (flag)
		{
			AddTrackData(itemTrackData);
		}
	}

	public void RemoveScript(int scriptID)
	{
		foreach (KeyValuePair<UIMissionNode, ItemTrackData> item in _nodeDataDic)
		{
			if (item.Value.type == TrackType.Script && item.Value.scriptID == scriptID)
			{
				DeleteNode(item.Key);
				break;
			}
		}
	}

	public void EnableWndDrag(bool enable)
	{
		UIDragObject[] componentsInChildren = base.transform.GetComponentsInChildren<UIDragObject>();
		if (componentsInChildren != null && componentsInChildren.Length > 0)
		{
			for (int i = 0; i < componentsInChildren.Length; i++)
			{
				componentsInChildren[i].enabled = enable;
			}
		}
	}

	public override void Show()
	{
		base.Show();
		ResetContentPos();
	}

	private bool AddTrackData(ItemTrackData data)
	{
		UIMissionNode uIMissionNode = itemsTree.AddMissionNode(null, string.Empty, enableCkTag: false, enableBtnDel: true, canSelected: false);
		uIMissionNode.mLbTitle.maxLineCount = 1;
		uIMissionNode.e_BtnDelete += DeleteNode;
		UIMissionNode uIMissionNode2 = itemsTree.AddMissionNode(uIMissionNode, string.Empty, enableCkTag: false, enableBtnDel: false, canSelected: false);
		uIMissionNode2.mLbTitle.maxLineCount = 0;
		_nodeDataDic.Add(uIMissionNode, data);
		UpdateNodeText(uIMissionNode);
		uIMissionNode.ChangeExpand();
		if (!GameUI.Instance.mItemsTrackWnd.isShow)
		{
			GameUI.Instance.mItemsTrackWnd.Show();
		}
		if (data != null && data.type == TrackType.Script && ScriptTrackChanged != null)
		{
			ScriptTrackChanged(data.scriptID, arg2: true);
		}
		return true;
	}

	private void DeleteNode(object obj)
	{
		UIMissionNode uIMissionNode = obj as UIMissionNode;
		if ((bool)uIMissionNode)
		{
			ItemTrackData itemTrackData = null;
			if (_nodeDataDic.ContainsKey(uIMissionNode))
			{
				itemTrackData = _nodeDataDic[uIMissionNode];
				_nodeDataDic.Remove(uIMissionNode);
			}
			itemsTree.DeleteMissionNode(uIMissionNode);
			if (itemTrackData != null && itemTrackData.type == TrackType.Script && ScriptTrackChanged != null)
			{
				ScriptTrackChanged(itemTrackData.scriptID, arg2: false);
			}
		}
	}

	private void ResetContentPos()
	{
		dragPanel.enabled = false;
		itemsTree.RepositionContent();
		StartCoroutine(DelayActiveDragIterator());
	}

	private IEnumerator DelayActiveDragIterator()
	{
		yield return null;
		dragPanel.enabled = true;
	}

	private IEnumerator UpdateItemsTrack()
	{
		while (true)
		{
			foreach (UIMissionNode node in _nodeDataDic.Keys)
			{
				UpdateNodeText(node);
			}
			yield return new WaitForSeconds(0.5f);
		}
	}

	private void UpdateNodeText(UIMissionNode node)
	{
		if (node == null || !_nodeDataDic.ContainsKey(node))
		{
			return;
		}
		ItemTrackData itemTrackData = _nodeDataDic[node];
		if (itemTrackData == null)
		{
			return;
		}
		UIMissionNode uIMissionNode = null;
		if (node.mChilds.Count > 0)
		{
			uIMissionNode = node.mChilds[0];
		}
		bool flag = true;
		if (playerPackage != null)
		{
			string text = string.Empty;
			foreach (KeyValuePair<int, int> item in itemTrackData.costDic)
			{
				string arg = ItemProto.GetName(item.Key);
				int count = playerPackage.GetCount(item.Key);
				string empty = string.Empty;
				empty = $"{count}/{item.Value}";
				bool flag2 = count >= item.Value;
				text += string.Format((!flag2) ? "[FFFFFF]{0} {1}[-]\n" : "[B4B4B4]{0} {1}[-]\n", arg, empty);
				if (!flag2)
				{
					flag = false;
				}
			}
			if ((bool)uIMissionNode)
			{
				uIMissionNode.mLbTitle.text = text;
			}
		}
		string text2 = itemTrackData.itemName;
		if (flag)
		{
			text2 = $"[C8C800]{$"{itemTrackData.itemName} X{itemTrackData.itemCount} [{PELocalization.GetString(8000694)}]"}[-]";
		}
		node.mLbTitle.text = text2;
	}
}
