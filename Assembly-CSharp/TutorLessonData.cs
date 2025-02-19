using System;
using System.Collections.Generic;
using System.Xml.Serialization;

[Serializable]
public class TutorLessonData
{
	[XmlAttribute]
	public string lessonName;

	public List<TutorStepData> steps;

	public TutorLessonData(int i)
	{
		lessonName = "lesson:" + (i + 1);
		steps = new List<TutorStepData>(3);
	}

	public TutorLessonData()
	{
	}
}
