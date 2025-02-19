using System.IO;
using System.Text;

namespace Pathea.IO;

public static class StringIO
{
	public static string LoadFromFile(string filename, Encoding encoding)
	{
		using FileStream fileStream = new FileStream(filename, FileMode.Open, FileAccess.Read, FileShare.Read);
		int num = (int)fileStream.Length;
		byte[] array = new byte[num];
		fileStream.Read(array, 0, num);
		string @string = encoding.GetString(array);
		fileStream.Close();
		return @string;
	}

	public static void SaveToFile(string filename, string content, Encoding encoding)
	{
		using FileStream fileStream = new FileStream(filename, FileMode.Create, FileAccess.Write, FileShare.None);
		byte[] bytes = encoding.GetBytes(content);
		fileStream.Write(bytes, 0, bytes.Length);
		fileStream.Close();
	}
}
