using System;
using System.Xml.Serialization;

[Serializable]
public class ShaderPairsTbl
{
	[XmlArrayItem("Pair", typeof(ShaderPair))]
	[XmlArray("ShaderPairs")]
	public ShaderPair[] shaderPairs { get; set; }
}
