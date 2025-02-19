using System;
using System.Collections.Generic;

[Serializable]
public class TutorData
{
	public List<TutorLessonData> lessons;

	public TutorData()
	{
		lessons = new List<TutorLessonData>(3);
	}
}
