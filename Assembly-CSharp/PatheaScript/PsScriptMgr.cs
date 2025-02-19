using System.Collections.Generic;
using System.IO;

namespace PatheaScript;

public class PsScriptMgr
{
	private List<PsScript> mScriptList = new List<PsScript>(10);

	private List<int> mScriptRemoveList = new List<int>(2);

	private List<int> mScriptLoadList = new List<int>(2);

	private VariableMgr mVarMgr;

	public Factory Factory { get; private set; }

	public EventProxyMgr EventProxyMgr { get; private set; }

	public List<PsScript> CurScript => mScriptList;

	private PsScriptMgr(Factory f)
	{
		Factory = f;
		EventProxyMgr = new EventProxyMgr(this);
	}

	public Variable GetVar(string varName)
	{
		return mVarMgr.GetVar(varName);
	}

	public bool AddVar(string varName, Variable var)
	{
		return mVarMgr.AddVar(varName, var);
	}

	private void LoadEntry()
	{
		AddToLoadList(Factory.EntryScriptId);
	}

	public PsScript FindScriptById(int id)
	{
		return mScriptList.Find((PsScript q) => (q.Id == id) ? true : false);
	}

	private void RemoveScript()
	{
		if (mScriptRemoveList.Count <= 0)
		{
			return;
		}
		foreach (int mScriptRemove in mScriptRemoveList)
		{
			PsScript psScript = FindScriptById(mScriptRemove);
			if (psScript == null)
			{
				Debug.Log("no script with id:" + mScriptRemove + " found to terminat");
				continue;
			}
			Debug.Log(string.Concat(psScript, " terminated"));
			psScript.Reset();
			mScriptList.Remove(psScript);
		}
		mScriptRemoveList.Clear();
	}

	private void LoadScript()
	{
		if (mScriptLoadList.Count <= 0)
		{
			return;
		}
		int[] array = mScriptLoadList.ToArray();
		int[] array2 = array;
		foreach (int num in array2)
		{
			PsScript psScript = FindScriptById(num);
			if (psScript != null)
			{
				Debug.LogError("Duplicated Script:" + psScript);
				continue;
			}
			PsScript psScript2 = PsScript.Load(this, num);
			if (psScript2 != null && psScript2.Init())
			{
				mScriptList.Add(psScript2);
				mScriptLoadList.Remove(num);
				Debug.Log("begin script:" + psScript2.Id);
				InternalEvent.Instance.EmitScriptBegin(psScript2);
			}
		}
	}

	private void TickScript()
	{
		foreach (PsScript mScript in mScriptList)
		{
			if (mScript.Tick() == TickResult.Finished)
			{
				AddToRemoveList(mScript.Id);
			}
		}
	}

	public void Tick()
	{
		EventProxyMgr.Tick();
		LoadScript();
		TickScript();
		RemoveScript();
	}

	public void AddToRemoveList(int q)
	{
		mScriptRemoveList.Add(q);
	}

	public void AddToLoadList(int id)
	{
		mScriptLoadList.Add(id);
	}

	public static PsScriptMgr Create(Factory factory)
	{
		factory.Init();
		PsScriptMgr psScriptMgr = new PsScriptMgr(factory);
		psScriptMgr.mVarMgr = new VariableMgr();
		psScriptMgr.LoadEntry();
		return psScriptMgr;
	}

	public static PsScriptMgr Deserialize(Factory factory, byte[] data)
	{
		if (data == null)
		{
			return null;
		}
		factory.Init();
		PsScriptMgr psScriptMgr = new PsScriptMgr(factory);
		MemoryStream input = new MemoryStream(data, writable: false);
		using BinaryReader binaryReader = new BinaryReader(input);
		int count = binaryReader.ReadInt32();
		byte[] data2 = binaryReader.ReadBytes(count);
		psScriptMgr.mVarMgr = VariableMgr.Import(data2);
		int num = binaryReader.ReadInt32();
		for (int i = 0; i < num; i++)
		{
			int id = binaryReader.ReadInt32();
			PsScript psScript = PsScript.Load(psScriptMgr, id);
			if (psScript != null && psScript.Init())
			{
				psScriptMgr.mScriptList.Add(psScript);
				psScript.Restore(binaryReader);
			}
		}
		DeserializeList(psScriptMgr.mScriptLoadList, binaryReader);
		DeserializeList(psScriptMgr.mScriptRemoveList, binaryReader);
		return psScriptMgr;
	}

	public static byte[] Serialize(PsScriptMgr mgr)
	{
		MemoryStream memoryStream = new MemoryStream(10240);
		using (BinaryWriter binaryWriter = new BinaryWriter(memoryStream))
		{
			byte[] array = VariableMgr.Export(mgr.mVarMgr);
			binaryWriter.Write(array.Length);
			binaryWriter.Write(array);
			binaryWriter.Write(mgr.mScriptList.Count);
			foreach (PsScript mScript in mgr.mScriptList)
			{
				binaryWriter.Write(mScript.Id);
				mScript.Store(binaryWriter);
			}
			SerializeList(mgr.mScriptLoadList, binaryWriter);
			SerializeList(mgr.mScriptRemoveList, binaryWriter);
		}
		return memoryStream.ToArray();
	}

	private static void SerializeList(List<int> list, BinaryWriter w)
	{
		w.Write(list.Count);
		foreach (int item in list)
		{
			w.Write(item);
		}
	}

	private static void DeserializeList(List<int> list, BinaryReader r)
	{
		int num = r.ReadInt32();
		for (int i = 0; i < num; i++)
		{
			list.Add(r.ReadInt32());
		}
	}
}
