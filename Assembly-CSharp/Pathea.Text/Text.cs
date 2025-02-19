namespace Pathea.Text;

public static class Text
{
	public static string CapitalWords(string s, bool everyword = false)
	{
		string text = string.Empty;
		bool flag = false;
		foreach (char c in s)
		{
			if (everyword && c == ' ')
			{
				flag = false;
			}
			if (c >= 'A' && c <= 'Z')
			{
				flag = true;
			}
			if (!flag && c >= 'a' && c <= 'z')
			{
				text += (char)(c - 32);
				flag = true;
			}
			else
			{
				text += c;
			}
		}
		return text;
	}

	public static string SingleLine(string s)
	{
		s = s.Replace("\r\n", " ");
		return s.Replace('\n', ' ');
	}

	public static string MakeFileName(string name)
	{
		string text = string.Empty;
		for (int i = 0; i < name.Length; i++)
		{
			text = ((name[i] != '/' && name[i] != '\\' && name[i] != ':' && name[i] != '*' && name[i] != '\r' && name[i] != '\n' && name[i] != '?' && name[i] != '"' && name[i] != '<' && name[i] != '>' && name[i] != '|' && name[i] != '\b') ? (text + name[i]) : (text + " "));
		}
		return text;
	}
}
