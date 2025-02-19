using System.Collections.Generic;
using UnityEngine;

public class HeadInfoMgr
{
	public class NameInfo
	{
		public bool colored;

		public string text;

		public string coloredText;

		public string iconName;

		public string Text
		{
			get
			{
				if (colored)
				{
					return coloredText;
				}
				return text;
			}
		}

		public string Icon => iconName;
	}

	public class HeadInfo
	{
		public Transform targetTran;

		public NameInfo nameInfo;
	}

	private static HeadInfoMgr instance;

	private List<HeadInfo> mList = new List<HeadInfo>(10);

	public static HeadInfoMgr Instance
	{
		get
		{
			if (instance == null)
			{
				instance = new HeadInfoMgr();
			}
			return instance;
		}
	}

	public IEnumerable<HeadInfo> Infos => mList;

	private HeadInfo FindHeadInfo(Transform trans)
	{
		return mList.Find((HeadInfo info) => object.ReferenceEquals(info.targetTran, trans) ? true : false);
	}

	private HeadInfo GetHeadInfo(Transform trans)
	{
		HeadInfo headInfo = FindHeadInfo(trans);
		if (headInfo == null)
		{
			headInfo = new HeadInfo();
			headInfo.targetTran = trans;
			headInfo.nameInfo = new NameInfo();
		}
		return headInfo;
	}

	public void Remove(Transform trans)
	{
		mList.RemoveAll((HeadInfo info) => object.ReferenceEquals(info.targetTran, trans) ? true : false);
	}

	public void SetText(Transform trans, string text)
	{
		HeadInfo headInfo = GetHeadInfo(trans);
		headInfo.nameInfo.text = text;
		headInfo.nameInfo.coloredText = "[ffa7ff]" + text + "[-]";
	}

	public void SetColor(Transform trans, bool flag)
	{
		HeadInfo headInfo = GetHeadInfo(trans);
		headInfo.nameInfo.colored = flag;
	}

	public void SetIcon(Transform trans, string icon)
	{
		HeadInfo headInfo = GetHeadInfo(trans);
		headInfo.nameInfo.iconName = icon;
	}
}
