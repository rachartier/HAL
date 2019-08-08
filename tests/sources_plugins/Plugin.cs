using System.IO;
using System.Collections.Generic;

public class Plugin 
{
	public enum FileType
	{
		UNKNOWN,
		DLL,
		SHARED_OBJECT,
		SCRIPT
	}

	private static Dictionary<FileType, string[]> acceptedFilesTypes = new Dictionary<FileType, string[]>()
	{
		[FileType.DLL] = new string[] {".dll"},
		[FileType.SHARED_OBJECT] = new string[] {".so"},
		[FileType.SCRIPT] = new string[] {".py", ".rb", ".sh"},
	};

	public string FileName {get;private set;}
	public string FilePath {get;private set;}
	public string FileExtension {get;private set;}
	public string Name {get;private set;}

	public FileType Type {get;private set;}

	public Plugin(string path)
	{
		if(!File.Exists(path))
			throw new FileNotFoundException();

		FilePath = path;
		FileName = Path.GetFileName(path);
		FileExtension = Path.GetExtension(FileName);
		Type = getPluginType();
		Name = Path.GetFileNameWithoutExtension(FilePath);
	}

	private FileType getPluginType()
	{
		foreach(var pair in acceptedFilesTypes)
		{
			foreach(string ext in pair.Value)
			{
				if(ext.Equals(FileExtension))
				{
					return pair.Key;
				}
			}
		}

		return FileType.UNKNOWN;
	}
}
