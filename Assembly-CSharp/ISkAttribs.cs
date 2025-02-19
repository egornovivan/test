using System.Collections.Generic;

public interface ISkAttribs
{
	ISkEntity entity { get; }

	IExpFuncSet expFunc { get; }

	IList<float> raws { get; }

	IList<float> sums { get; }

	IList<bool> modflags { get; }

	PackBase pack { get; set; }

	float buffMul { get; set; }

	float buffPreAdd { get; set; }

	float buffPostAdd { get; set; }
}
