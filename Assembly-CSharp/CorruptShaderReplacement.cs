using System.IO;
using System.Xml.Serialization;
using UnityEngine;

public class CorruptShaderReplacement : MonoBehaviour
{
	private const string c_shaderReplaceTblName = "shaderReplaceTbl";

	private static ShaderPairsTbl s_shaderPairsTbl;

	public string[] corruptShaderNames = new string[0];

	public Shader[] validShaders = new Shader[0];

	private void Start()
	{
		ReadShaderPairsTbl(bFindShellShader: false, bFindEntityShader: true);
	}

	public static Shader FindValidShader(string shadername)
	{
		if (s_shaderPairsTbl != null)
		{
			int num = s_shaderPairsTbl.shaderPairs.Length;
			for (int i = 0; i < num; i++)
			{
				if (shadername == s_shaderPairsTbl.shaderPairs[i].shellName)
				{
					return s_shaderPairsTbl.shaderPairs[i].entity;
				}
			}
		}
		return null;
	}

	public static void ReadShaderPairsTbl(bool bFindShellShader, bool bFindEntityShader)
	{
		TextAsset textAsset = Resources.Load("shaderReplaceTbl") as TextAsset;
		StringReader stringReader = new StringReader(textAsset.text);
		if (stringReader == null)
		{
			return;
		}
		XmlSerializer xmlSerializer = new XmlSerializer(typeof(ShaderPairsTbl));
		s_shaderPairsTbl = (ShaderPairsTbl)xmlSerializer.Deserialize(stringReader);
		stringReader.Close();
		if (s_shaderPairsTbl == null)
		{
			return;
		}
		int num = s_shaderPairsTbl.shaderPairs.Length;
		if (bFindShellShader)
		{
			for (int i = 0; i < num; i++)
			{
				s_shaderPairsTbl.shaderPairs[i].shell = Shader.Find(s_shaderPairsTbl.shaderPairs[i].shellName);
			}
		}
		if (bFindEntityShader)
		{
			for (int j = 0; j < num; j++)
			{
				s_shaderPairsTbl.shaderPairs[j].entity = Shader.Find(s_shaderPairsTbl.shaderPairs[j].entityName);
			}
		}
	}

	public static bool ReplaceShaderShellWithEntity(GameObject root)
	{
		if (s_shaderPairsTbl == null)
		{
			return false;
		}
		bool result = false;
		int num = s_shaderPairsTbl.shaderPairs.Length;
		Renderer[] componentsInChildren = root.GetComponentsInChildren<Renderer>(includeInactive: true);
		int num2 = componentsInChildren.Length;
		for (int i = 0; i < num2; i++)
		{
			Material[] sharedMaterials = componentsInChildren[i].sharedMaterials;
			int num3 = sharedMaterials.Length;
			for (int j = 0; j < num3; j++)
			{
				if (sharedMaterials[j] == null || sharedMaterials[j].shader == null)
				{
					if (sharedMaterials[j] == null)
					{
						Debug.LogError("Error on replacing " + componentsInChildren[i].name + "'s MatNo." + j + " for it has not shader");
					}
					else
					{
						Debug.LogError("Error on replacing " + componentsInChildren[i].name + "'s Mat " + sharedMaterials[j].name + " for it has not shader");
					}
					continue;
				}
				string text = sharedMaterials[j].shader.name;
				for (int k = 0; k < num; k++)
				{
					if (text == s_shaderPairsTbl.shaderPairs[k].shellName)
					{
						if (s_shaderPairsTbl.shaderPairs[k].entity == null || s_shaderPairsTbl.shaderPairs[k].entity.name != s_shaderPairsTbl.shaderPairs[k].entityName)
						{
							s_shaderPairsTbl.shaderPairs[k].entity = Shader.Find(s_shaderPairsTbl.shaderPairs[k].entityName);
						}
						sharedMaterials[j].shader = s_shaderPairsTbl.shaderPairs[k].entity;
						result = true;
						break;
					}
				}
			}
		}
		return result;
	}

	public static bool ReplaceShaderEntityWithShell(GameObject root)
	{
		if (s_shaderPairsTbl == null)
		{
			return false;
		}
		bool result = false;
		int num = s_shaderPairsTbl.shaderPairs.Length;
		Renderer[] componentsInChildren = root.GetComponentsInChildren<Renderer>(includeInactive: true);
		int num2 = componentsInChildren.Length;
		for (int i = 0; i < num2; i++)
		{
			Material[] sharedMaterials = componentsInChildren[i].sharedMaterials;
			int num3 = sharedMaterials.Length;
			for (int j = 0; j < num3; j++)
			{
				if (sharedMaterials[j] == null || sharedMaterials[j].shader == null)
				{
					if (sharedMaterials[j] == null)
					{
						Debug.LogError("Error on replacing " + componentsInChildren[i].name + "'s MatNo." + j + " for it has not shader");
					}
					else
					{
						Debug.LogError("Error on replacing " + componentsInChildren[i].name + "'s Mat " + sharedMaterials[j].name + " for it has not shader");
					}
					continue;
				}
				string text = sharedMaterials[j].shader.name;
				for (int k = 0; k < num; k++)
				{
					if (text == s_shaderPairsTbl.shaderPairs[k].entityName)
					{
						if (s_shaderPairsTbl.shaderPairs[k].shell == null || s_shaderPairsTbl.shaderPairs[k].shell.name != s_shaderPairsTbl.shaderPairs[k].shellName)
						{
							s_shaderPairsTbl.shaderPairs[k].shell = Shader.Find(s_shaderPairsTbl.shaderPairs[k].shellName);
						}
						sharedMaterials[j].shader = s_shaderPairsTbl.shaderPairs[k].shell;
						result = true;
						break;
					}
				}
			}
		}
		return result;
	}
}
