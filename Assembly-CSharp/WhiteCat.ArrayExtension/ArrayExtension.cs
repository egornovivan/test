using System;
using System.Text;

namespace WhiteCat.ArrayExtension;

public static class ArrayExtension
{
	public static void SetValues<T>(this T[] array, T value, int index = 0, int count = 0)
	{
		count = ((count > 0) ? (count + index) : array.Length);
		while (index < count)
		{
			array[index++] = value;
		}
	}

	public static void SetValues<T>(this T[,] array, T value, int beginRow = 0, int beginCol = 0, int endRow = 0, int endCol = 0)
	{
		if (endRow <= 0)
		{
			endRow = array.GetLength(0);
		}
		if (endCol <= 0)
		{
			endCol = array.GetLength(1);
		}
		for (int i = beginRow; i < endRow; i++)
		{
			for (int j = beginCol; j < endCol; j++)
			{
				array[i, j] = value;
			}
		}
	}

	public static void Traverse(this Array array, Action<int, int[]> onElement, Action<int, int[]> beginDimension = null, Action<int, int[]> endDimension = null)
	{
		if (array.Length != 0)
		{
			TraverseDimension(array, onElement, beginDimension, endDimension, 0, new int[array.Rank]);
		}
	}

	private static void TraverseDimension(Array array, Action<int, int[]> onElement, Action<int, int[]> beginDimension, Action<int, int[]> endDimension, int dimension, int[] indices)
	{
		int length = array.GetLength(dimension);
		bool flag = dimension + 1 == array.Rank;
		beginDimension?.Invoke(dimension, indices);
		for (int i = 0; i < length; i++)
		{
			indices[dimension] = i;
			if (flag)
			{
				onElement?.Invoke(dimension, indices);
			}
			else
			{
				TraverseDimension(array, onElement, beginDimension, endDimension, dimension + 1, indices);
			}
		}
		endDimension?.Invoke(dimension, indices);
	}

	public static string GetContentString(this Array array)
	{
		StringBuilder builder = new StringBuilder(array.Length * 4);
		array.Traverse(delegate(int d, int[] i)
		{
			if (i[d] != 0)
			{
				builder.Append(',');
			}
			builder.Append(' ');
			object value = array.GetValue(i);
			if (object.ReferenceEquals(value, null))
			{
				builder.Append("null");
			}
			else if (value.GetType() == typeof(string))
			{
				builder.Append('"');
				builder.Append(value);
				builder.Append('"');
			}
			else
			{
				builder.Append(value);
			}
		}, delegate(int d, int[] i)
		{
			if (d != 0)
			{
				if (i[d - 1] != 0)
				{
					builder.Append(',');
				}
				builder.Append('\n');
				while (d != 0)
				{
					builder.Append('\t');
					d--;
				}
			}
			builder.Append('{');
		}, delegate(int d, int[] i)
		{
			if (d + 1 == array.Rank)
			{
				builder.Append(" }");
			}
			else
			{
				builder.Append('\n');
				while (d != 0)
				{
					builder.Append('\t');
					d--;
				}
				builder.Append('}');
			}
		});
		return builder.ToString();
	}
}
