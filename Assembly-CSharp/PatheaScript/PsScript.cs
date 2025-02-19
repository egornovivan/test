using System;
using System.IO;
using System.Xml;

namespace PatheaScript;

public class PsScript : Storeable
{
	public enum EResult
	{
		Accomplished,
		Failed,
		Abort,
		Max
	}

	private TriggerGroup mTriggerGroup;

	private VariableMgr mVarMgr;

	private PsScriptMgr mMgr;

	public int Id { get; private set; }

	public string Name { get; private set; }

	public EResult Result { get; private set; }

	public PsScriptMgr Parent => mMgr;

	private PsScript(PsScriptMgr mgr)
	{
		mMgr = mgr;
		mVarMgr = new VariableMgr();
		Result = EResult.Max;
	}

	public Variable GetVar(string varName, bool bFindInParent = true)
	{
		Variable var = mVarMgr.GetVar(varName);
		if (var != null)
		{
			return var;
		}
		if (!bFindInParent)
		{
			return var;
		}
		return Parent.GetVar(varName);
	}

	public bool AddVar(string varName, Variable var)
	{
		return mVarMgr.AddVar(varName, var);
	}

	public static PsScript Load(PsScriptMgr mgr, int id)
	{
		string scriptPath = mgr.Factory.GetScriptPath(id);
		try
		{
			XmlDocument xmlDocument = new XmlDocument();
			xmlDocument.Load(scriptPath);
			XmlNode xmlNode = xmlDocument.SelectSingleNode("//MISSION");
			if (xmlNode == null)
			{
				return null;
			}
			PsScript psScript = new PsScript(mgr);
			psScript.Id = id;
			psScript.Name = Util.GetString(xmlNode, "name");
			TriggerGroup triggerGroup = new TriggerGroup(psScript);
			triggerGroup.SetInfo(mgr.Factory, xmlNode);
			if (!triggerGroup.Parse())
			{
				return null;
			}
			psScript.mTriggerGroup = triggerGroup;
			return psScript;
		}
		catch (Exception ex)
		{
			Debug.LogError(ex.ToString());
			return null;
		}
	}

	public override string ToString()
	{
		return $"Script[id={Id}, name={Name}]";
	}

	public void RequireStop(EResult eResult)
	{
		Result = eResult;
		mTriggerGroup.RequireStop();
		Debug.Log(string.Concat(this, " require stop."));
		InternalEvent.Instance.EmitScriptEnd(this);
	}

	public bool Init()
	{
		if (!mTriggerGroup.Init())
		{
			return false;
		}
		return true;
	}

	public TickResult Tick()
	{
		return mTriggerGroup.Tick();
	}

	public void Reset()
	{
		mTriggerGroup.Reset();
	}

	public void Store(BinaryWriter w)
	{
		w.Write((sbyte)Result);
		byte[] array = VariableMgr.Export(mVarMgr);
		w.Write(array.Length);
		w.Write(array);
		mTriggerGroup.Store(w);
	}

	public void Restore(BinaryReader r)
	{
		Result = (EResult)r.ReadSByte();
		int count = r.ReadInt32();
		byte[] data = r.ReadBytes(count);
		mVarMgr = VariableMgr.Import(data);
		mTriggerGroup.Restore(r);
	}
}
