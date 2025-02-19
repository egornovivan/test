using uLink;
using UnityEngine;

namespace CustomData;

public class CustomAppearanceData
{
	public string account;

	public string name;

	public int[] customStyle = new int[9];

	public float height;

	public float width;

	public float[] bodydata = new float[20];

	public Color skincolor;

	public Color eyecolor;

	public Color haircolor;

	public static void WriteAppearanceData(uLink.BitStream stream, object obj, params object[] codecOptions)
	{
		CustomAppearanceData customAppearanceData = (CustomAppearanceData)obj;
		stream.Write(customAppearanceData.account);
		stream.Write(customAppearanceData.name);
		stream.Write(customAppearanceData.customStyle);
		stream.Write(customAppearanceData.height);
		stream.Write(customAppearanceData.width);
		stream.Write(customAppearanceData.bodydata);
		stream.Write(customAppearanceData.skincolor);
		stream.Write(customAppearanceData.eyecolor);
		stream.Write(customAppearanceData.haircolor);
	}

	public static object ReadAppearanceData(uLink.BitStream stream, params object[] codecOptions)
	{
		CustomAppearanceData customAppearanceData = new CustomAppearanceData();
		customAppearanceData.account = stream.Read<string>(new object[0]);
		customAppearanceData.name = stream.Read<string>(new object[0]);
		customAppearanceData.customStyle = stream.Read<int[]>(new object[0]);
		customAppearanceData.height = stream.Read<float>(new object[0]);
		customAppearanceData.width = stream.Read<float>(new object[0]);
		customAppearanceData.bodydata = stream.Read<float[]>(new object[0]);
		customAppearanceData.skincolor = stream.Read<Color>(new object[0]);
		customAppearanceData.eyecolor = stream.Read<Color>(new object[0]);
		customAppearanceData.haircolor = stream.Read<Color>(new object[0]);
		return customAppearanceData;
	}
}
