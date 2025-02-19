using System.Xml;

namespace PatheaScript;

public abstract class ParseObj
{
	protected Factory mFactory;

	protected XmlNode mInfo;

	public void SetInfo(Factory factory, XmlNode xmlNode)
	{
		mFactory = factory;
		mInfo = xmlNode;
	}

	public abstract bool Parse();
}
