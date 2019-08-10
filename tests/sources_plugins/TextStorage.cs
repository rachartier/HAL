using System;

public class TextStorage : IStorage
{
	public StorageCode Save<T>(T obj)
	{
		Console.WriteLine(obj);
		return StorageCode.SUCCESS;
	}
}
