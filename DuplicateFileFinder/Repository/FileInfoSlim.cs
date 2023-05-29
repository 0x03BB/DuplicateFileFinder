using System.ComponentModel.DataAnnotations;

namespace DuplicateFileFinder.Repository;

internal sealed class FileInfoSlim
{
    [Key]
    public required string Path { get; set; }
    public required DateTime Modified { get; set; }
    public required ulong Hash { get; set; }
    public required long Version { get; set; }

    public override string ToString() => Path + "\t" + Modified.ToString();
}
