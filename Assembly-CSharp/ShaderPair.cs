using System;
using System.Xml.Serialization;
using UnityEngine;

[Serializable]
public class ShaderPair
{
	public Shader shell;

	public Shader entity;

	[XmlAttribute("ShellShader")]
	public string shellName { get; set; }

	[XmlAttribute("EntityShader")]
	public string entityName { get; set; }
}
