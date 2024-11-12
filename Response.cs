using System;

[Serializable]
public class Response<T>
{
	public T data;

	public RespStatus status;
}
