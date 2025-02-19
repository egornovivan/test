using System.Collections.Generic;

public class VCEAction
{
	public List<VCEModify> Modifies;

	public VCEAction()
	{
		Modifies = new List<VCEModify>();
	}

	public void Destroy()
	{
		if (Modifies != null)
		{
			Modifies.Clear();
			Modifies = null;
		}
	}

	public void Clear()
	{
		if (Modifies != null)
		{
			Modifies.Clear();
		}
	}

	public void Undo()
	{
		for (int num = Modifies.Count - 1; num >= 0; num--)
		{
			Modifies[num].Undo();
		}
		VCEHistory.s_Modified = true;
	}

	public void Redo()
	{
		for (int i = 0; i < Modifies.Count; i++)
		{
			Modifies[i].Redo();
		}
		VCEHistory.s_Modified = true;
	}

	public void DoButNotRegister()
	{
		for (int i = 0; i < Modifies.Count; i++)
		{
			Modifies[i].Redo();
		}
	}

	public void Do()
	{
		Redo();
		Register();
	}

	public void Register()
	{
		VCEHistory.AddAction(this);
		VCEHistory.s_Modified = true;
	}
}
