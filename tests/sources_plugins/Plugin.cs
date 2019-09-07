using System.IO;
using System.Collections.Generic;

public class Plugin
{
    public enum FileType
    {
        Unknown,
        DLL,
        Script,
        SharedObject
    }

    public static Dictionary<FileType, string[]> AcceptedFilesTypes = new Dictionary<FileType, string[]>()
    {
        [FileType.DLL] = new string[] { ".dll" },
        [FileType.Script] = new string[] { ".py", ".rb", ".sh", ".pl" },
        [FileType.SharedObject] = new string[] {".so"}
    };

    public string FileName { get; private set; }
    public string FilePath { get; private set; }
    public string FileExtension { get; private set; }
    public string Name { get; private set; }

    public FileType Type { get; private set; }

    public OSAttribute.TargetFlag OsAuthorized = 0;
    public double Hearthbeat { get; set; } = 1;
    public bool Activated { get; set; } = false;

    public Plugin(string path)
    {
        FilePath = Path.GetFullPath(path);
        FileName = Path.GetFileName(path);
        FileExtension = Path.GetExtension(FileName);
        Type = getPluginType();
        Name = Path.GetFileNameWithoutExtension(FilePath);
    }

    private FileType getPluginType()
    {
        foreach (var pair in AcceptedFilesTypes)
        {
            foreach (string ext in pair.Value)
            {
                if (ext.Equals(FileExtension))
                {
                    return pair.Key;
                }
            }
        }

        return FileType.Unknown;
    }
}
