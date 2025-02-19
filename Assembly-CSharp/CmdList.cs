using System.Collections;
using System.Collections.Generic;

public class CmdList : IEnumerable<string>, IEnumerable
{
	public class Cmd
	{
		public string name;

		public CmdExe exe;
	}

	public delegate void CmdExe();

	private List<Cmd> mList = new List<Cmd>(2);

	public int count => mList.Count;

	IEnumerator<string> IEnumerable<string>.GetEnumerator()
	{
		return mList.ConvertAll((Cmd item) => item.name).GetEnumerator();
	}

	IEnumerator IEnumerable.GetEnumerator()
	{
		return mList.GetEnumerator();
	}

	public void Add(string cmdName, CmdExe cmdExe)
	{
		mList.Add(new Cmd
		{
			name = cmdName,
			exe = cmdExe
		});
	}

	public void Add(Cmd cmd)
	{
		mList.Add(cmd);
	}

	public Cmd Get(int index)
	{
		if (index >= mList.Count)
		{
			return null;
		}
		return mList[index];
	}

	public bool Remove(string name)
	{
		return 0 < mList.RemoveAll((Cmd item) => (name == item.name) ? true : false);
	}

	public void Clear()
	{
		mList.Clear();
	}

	public void ExecuteCmd(string name)
	{
		mList.Find((Cmd item) => (item.name == name) ? true : false)?.exe();
	}
}
