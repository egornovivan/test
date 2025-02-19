using System;
using System.Xml.Serialization;

[Serializable]
public class ShaderPairsTbl
{
	[XmlArray("ShaderPairs")]
	[XmlArrayItem("Pair", typeof(ShaderPair))]
	public ShaderPair[] shaderPairs { get; set; }
}
