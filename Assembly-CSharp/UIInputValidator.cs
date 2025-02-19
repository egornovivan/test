using UnityEngine;

[RequireComponent(typeof(UIInput))]
[AddComponentMenu("NGUI/Interaction/Input Validator")]
public class UIInputValidator : MonoBehaviour
{
	public enum Validation
	{
		None,
		Integer,
		Float,
		Alphanumeric,
		Username,
		Name,
		FileName,
		FilePath,
		CharacterName,
		ServerName,
		Password,
		PositiveInteger
	}

	public Validation logic;

	private void Start()
	{
		GetComponent<UIInput>().validator = Validate;
	}

	private char Validate(string text, char ch)
	{
		if (logic == Validation.None || !base.enabled)
		{
			return ch;
		}
		if (logic == Validation.Integer)
		{
			if (ch >= '0' && ch <= '9')
			{
				return ch;
			}
			if (ch == '-' && text.Length == 0)
			{
				return ch;
			}
		}
		else if (logic == Validation.Float)
		{
			if (ch >= '0' && ch <= '9')
			{
				return ch;
			}
			if (ch == '-' && text.Length == 0)
			{
				return ch;
			}
			if (ch == '.' && !text.Contains("."))
			{
				return ch;
			}
		}
		else if (logic == Validation.Alphanumeric)
		{
			if (ch >= 'A' && ch <= 'Z')
			{
				return ch;
			}
			if (ch >= 'a' && ch <= 'z')
			{
				return ch;
			}
			if (ch >= '0' && ch <= '9')
			{
				return ch;
			}
		}
		else if (logic == Validation.Username)
		{
			if (ch >= 'A' && ch <= 'Z')
			{
				return (char)(ch - 65 + 97);
			}
			if (ch >= 'a' && ch <= 'z')
			{
				return ch;
			}
			if (ch >= '0' && ch <= '9')
			{
				return ch;
			}
		}
		else if (logic == Validation.Name)
		{
			char c = ((text.Length <= 0) ? ' ' : text[text.Length - 1]);
			if (ch >= 'a' && ch <= 'z')
			{
				return ch;
			}
			if (ch >= 'A' && ch <= 'Z')
			{
				return ch;
			}
			switch (ch)
			{
			case '\'':
				if (c != ' ' && c != '\'' && !text.Contains("'"))
				{
					return ch;
				}
				break;
			case ' ':
				if (c != ' ' && c != '\'')
				{
					return ch;
				}
				break;
			}
		}
		else if (logic == Validation.FilePath)
		{
			if (ch != ':' && ch != '*' && ch != '\r' && ch != '\n' && ch != '?' && ch != '"' && ch != '<' && ch != '>' && ch != '|')
			{
				return ch;
			}
		}
		else if (logic == Validation.FileName)
		{
			if (ch != '/' && ch != '\\' && ch != ':' && ch != '*' && ch != '\r' && ch != '\n' && ch != '?' && ch != '"' && ch != '<' && ch != '>' && ch != '|')
			{
				return ch;
			}
		}
		else if (logic == Validation.CharacterName)
		{
			char c2 = ((text.Length <= 0) ? ' ' : text[text.Length - 1]);
			if (ch >= 'a' && ch <= 'z')
			{
				return ch;
			}
			if (ch >= 'A' && ch <= 'Z')
			{
				return ch;
			}
			switch (ch)
			{
			case ' ':
				if (c2 != ' ' && c2 != '\'')
				{
					return ch;
				}
				break;
			case '0':
			case '1':
			case '2':
			case '3':
			case '4':
			case '5':
			case '6':
			case '7':
			case '8':
			case '9':
				return ch;
			default:
				if (ch == '-' || ch == '.' || ch == '_' || ch == '=' || ch == '[' || ch == ']' || ch == '{' || ch == '}' || ch == '(' || ch == ')' || ch == ':')
				{
					return ch;
				}
				break;
			}
		}
		else if (logic == Validation.ServerName)
		{
			if (ch >= 'a' && ch <= 'z')
			{
				return ch;
			}
			if (ch >= 'A' && ch <= 'Z')
			{
				return ch;
			}
			if (ch >= '0' && ch <= '9')
			{
				return ch;
			}
		}
		else if (logic == Validation.Password)
		{
			if (ch >= 'a' && ch <= 'z')
			{
				return ch;
			}
			if (ch >= 'A' && ch <= 'Z')
			{
				return ch;
			}
			if (ch >= '0' && ch <= '9')
			{
				return ch;
			}
		}
		else if (logic == Validation.PositiveInteger)
		{
			if (text.Length > 1 && text[0] == '0')
			{
				text = text.Substring(1);
			}
			if (ch >= '0' && ch <= '9')
			{
				return ch;
			}
		}
		return '\0';
	}
}
