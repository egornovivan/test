using System;
using System.Collections.Generic;

public static class VCEHistory
{
	public const int MAX_ACTION_CNT = 32;

	public static bool s_Modified;

	private static List<VCEAction> m_Undos;

	private static List<VCEAction> m_Redos;

	public static void Init()
	{
		m_Undos = new List<VCEAction>();
		m_Redos = new List<VCEAction>();
	}

	public static void Destroy()
	{
		if (m_Undos != null)
		{
			foreach (VCEAction undo in m_Undos)
			{
				undo.Destroy();
			}
			m_Undos.Clear();
			m_Undos = null;
		}
		if (m_Redos != null)
		{
			foreach (VCEAction redo in m_Redos)
			{
				redo.Destroy();
			}
			m_Redos.Clear();
			m_Redos = null;
		}
		GC.Collect();
	}

	public static void Clear()
	{
		ClearUndos();
		ClearRedos();
	}

	private static void ClearUndos()
	{
		if (m_Undos == null)
		{
			return;
		}
		foreach (VCEAction undo in m_Undos)
		{
			undo.Destroy();
		}
		m_Undos.Clear();
	}

	private static void ClearRedos()
	{
		if (m_Redos == null)
		{
			return;
		}
		foreach (VCEAction redo in m_Redos)
		{
			redo.Destroy();
		}
		m_Redos.Clear();
	}

	public static void AddAction(VCEAction action)
	{
		if (action.Modifies.Count != 0)
		{
			ClearRedos();
			m_Undos.Insert(0, action);
			if (m_Undos.Count > 32)
			{
				m_Undos[32].Destroy();
				m_Undos.RemoveAt(32);
			}
		}
	}

	public static bool Undo()
	{
		if (m_Undos.Count > 0)
		{
			VCEAction vCEAction = m_Undos[0];
			vCEAction.Undo();
			m_Redos.Insert(0, vCEAction);
			m_Undos.RemoveAt(0);
			GC.Collect();
			return true;
		}
		return false;
	}

	public static bool Redo()
	{
		if (m_Redos.Count > 0)
		{
			VCEAction vCEAction = m_Redos[0];
			vCEAction.Redo();
			m_Undos.Insert(0, vCEAction);
			m_Redos.RemoveAt(0);
			GC.Collect();
			return true;
		}
		return false;
	}

	public static bool CanUndo()
	{
		if (m_Undos == null)
		{
			return false;
		}
		if (m_Undos.Count > 0)
		{
			return true;
		}
		return false;
	}

	public static bool CanRedo()
	{
		if (m_Redos == null)
		{
			return false;
		}
		if (m_Redos.Count > 0)
		{
			return true;
		}
		return false;
	}
}
