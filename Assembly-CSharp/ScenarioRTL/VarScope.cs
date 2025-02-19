using System.Collections.Generic;
using System.IO;

namespace ScenarioRTL;

public class VarScope
{
	private VarScope m_ParentScope;

	private Dictionary<string, Var> m_Vars;

	public Var this[string varname]
	{
		get
		{
			if (m_Vars.ContainsKey(varname))
			{
				return m_Vars[varname];
			}
			if (m_ParentScope != null)
			{
				return m_ParentScope[varname];
			}
			return Var.zero;
		}
		set
		{
			m_Vars[varname] = value;
		}
	}

	public string[] declaredVars
	{
		get
		{
			string[] array = new string[m_Vars.Count];
			m_Vars.Keys.CopyTo(array, 0);
			return array;
		}
	}

	public VarScope()
	{
		m_ParentScope = null;
		m_Vars = new Dictionary<string, Var>();
	}

	private VarScope(VarScope parent_scope)
	{
		m_ParentScope = parent_scope;
		m_Vars = new Dictionary<string, Var>();
	}

	public bool VarDeclared(string varname)
	{
		return m_Vars.ContainsKey(varname);
	}

	public VarScope CreateChild()
	{
		return new VarScope(this);
	}

	public void Clear()
	{
		m_Vars.Clear();
	}

	public void Import(BinaryReader r)
	{
		Clear();
		int num = r.ReadInt32();
		for (int i = 0; i < num; i++)
		{
			string key = r.ReadString();
			string data = r.ReadString();
			Var value = default(Var);
			value.data = data;
			m_Vars[key] = value;
		}
	}

	public void Export(BinaryWriter w)
	{
		w.Write(m_Vars.Count);
		foreach (KeyValuePair<string, Var> var in m_Vars)
		{
			w.Write(var.Key);
			w.Write(var.Value.data);
		}
	}
}
