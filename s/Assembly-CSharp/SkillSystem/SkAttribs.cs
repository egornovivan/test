namespace SkillSystem;

public class SkAttribs
{
	public static int nAttribs = 97;

	public float _preAdd;

	public float _mul;

	public float _postAdd;

	public SkNumericAttribs NumAttribs { get; set; }

	public SkAttribs()
	{
		NumAttribs = new SkNumericAttribs(nAttribs);
	}
}
