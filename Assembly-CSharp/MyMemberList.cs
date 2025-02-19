using System.Collections.Generic;

public class MyMemberList
{
	public List<MyMemberInf> traineeList;

	public List<MyMemberInf> instructorList;

	public MyMemberList()
	{
		traineeList = new List<MyMemberInf>();
		instructorList = new List<MyMemberInf>();
	}

	public void ClearList()
	{
		traineeList.Clear();
		instructorList.Clear();
	}
}
