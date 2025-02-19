namespace ScenarioRTL.IO;

public class ParamRaw
{
	private string[] names;

	private string[] values;

	public int count
	{
		get
		{
			return names.Length;
		}
		set
		{
			int num = ((value > 0) ? value : 0);
			names = new string[num];
			values = new string[num];
		}
	}

	public string this[int index] => values[index];

	public string this[string pname]
	{
		get
		{
			for (int i = 0; i < names.Length; i++)
			{
				if (names[i] == pname)
				{
					return values[i];
				}
			}
			return string.Empty;
		}
	}

	public ParamRaw(int cnt)
	{
		count = cnt;
	}

	public string GetName(int index)
	{
		return names[index];
	}

	public string GetValue(int index)
	{
		return values[index];
	}

	public void Set(int index, string name, string value)
	{
		names[index] = name;
		values[index] = value;
	}

	public override string ToString()
	{
		string text = string.Empty;
		int i;
		for (i = 0; i < 1 && i < count; i++)
		{
			text = text + names[i] + " = " + values[i];
			if (i != count - 1)
			{
				text += "; ";
			}
		}
		if (i < count)
		{
			text += "...";
		}
		return text;
	}
}
