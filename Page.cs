using System;
using System.Collections.Generic;

[Serializable]
public class Page<T>
{
	public List<T> content;

	public int totalPages;

	public int totalElements;

	public bool first;

	public bool last;

	public int size;

	public int number;

	public int numberOfElements;
}
