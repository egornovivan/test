using System.Collections.Generic;

public class BSSelectedBoxModify : BSModify
{
	private BSSelectBrush select_brush;

	private Dictionary<IntVector3, byte> old_selection;

	private Dictionary<IntVector3, byte> new_selection;

	private IBSDataSource data_source;

	public BSSelectedBoxModify(Dictionary<IntVector3, byte> old_value, Dictionary<IntVector3, byte> new_value, BSSelectBrush brush)
	{
		old_selection = old_value;
		new_selection = new_value;
		select_brush = brush;
		data_source = brush.dataSource;
	}

	public override bool Redo()
	{
		if (select_brush != null)
		{
			select_brush.ResetSelection(new_selection);
		}
		return true;
	}

	public override bool Undo()
	{
		if (select_brush != null)
		{
			select_brush.ResetSelection(old_selection);
		}
		return true;
	}

	public override bool IsNull()
	{
		return select_brush == null || select_brush.dataSource != data_source;
	}
}
