using System;
using System.Xml.Serialization;

[Serializable]
public class TutorStepData
{
	[XmlAttribute]
	public string name;

	public string description;

	[XmlAttribute]
	public string imageFileName;

	public TutorStepData(int i, int j)
	{
		name = "Step:" + (j + 1);
		description = "description excample.";
		imageFileName = string.Empty + (i + 1) + "_" + (j + 1);
	}

	public TutorStepData()
	{
	}
}
