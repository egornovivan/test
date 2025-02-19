using System;

namespace SkillSystem;

public class AttFilterCtrl
{
	private AttFilter mFilter;

	public AttFilterCtrl(SkEntity skEntity, string para, Action func)
	{
		string[] array = para.Split(';');
		string[] array2 = array;
		foreach (string text in array2)
		{
			string[] array3 = text.Split(',');
			switch (array3[0].ToLower())
			{
			case "event":
				mFilter = new AttFilter_Event(skEntity, Convert.ToInt32(array3[1]), func);
				break;
			case "time":
				mFilter = new AttFilter_Time(Convert.ToSingle(array3[1]));
				break;
			}
		}
	}

	public bool CheckFilter()
	{
		return mFilter.CheckFilter();
	}

	public void Destroy()
	{
		mFilter.Destroy();
	}
}
