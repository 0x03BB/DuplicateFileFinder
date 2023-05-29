using System.Diagnostics;
using System.IO.Hashing;
using Microsoft.EntityFrameworkCore;
using DuplicateFileFinder.Repository;

namespace DuplicateFileFinder;

internal sealed class DuplicateFileFinder
{
    private readonly IProgress<(ProgressPhase progressPhase, int total, int current)> _progress;
    private readonly Lazy<XxHash3> _hasher = new(false);

    public DuplicateFileFinder(IProgress<(ProgressPhase progressPhase, int total, int current)> progress)
    {
        _progress = progress;
    }

    public async Task<OneOf<(List<List<FileInfoSlim>> Collection, int Total), Error<string>>> FindDuplicatesAsync(string databasePath, CancellationToken cancellationToken = default)
    {
        var directoryPath = Path.GetDirectoryName(databasePath);
        if (string.IsNullOrEmpty(directoryPath))
        {
            return new Error<string>("Error: The selected directory does not exist.");
        }

        var (filesToHash, fileTotal) = GetSameSizeFiles(directoryPath, cancellationToken);

        return await GetDuplicateFilesAndUpdateDatabase(databasePath, directoryPath, filesToHash, fileTotal, cancellationToken);
    }

    private (List<List<FileInfo>> Collection, int Total) GetSameSizeFiles(string directoryPath, CancellationToken cancellationToken = default)
    {
        var directory = new DirectoryInfo(directoryPath);
        var dictionary = new Dictionary<long, List<FileInfo>>();

        var files = directory.EnumerateFiles("*", SearchOption.AllDirectories);
        int counter = 0;
        foreach (var file in files)
        {
            cancellationToken.ThrowIfCancellationRequested();

            if (dictionary.TryGetValue(file.Length, out var fileInfoList))
            {
                fileInfoList.Add(file);
            }
            else
            {
                dictionary.Add(file.Length, new List<FileInfo> { file });
            }

            counter++;
            _progress.Report((ProgressPhase.Finding, counter, 0));
        }

        var list = new List<List<FileInfo>>();
        counter = 0;
        foreach (var fileInfoList in dictionary.Where(kvp => kvp.Value.Count > 1).Select(kvp => kvp.Value))
        {
            list.Add(fileInfoList);
            counter += fileInfoList.Count;
        }

        return (list, counter);
    }

    private async ValueTask<(List<List<FileInfoSlim>> Collection, int Total)> GetDuplicateFilesAndUpdateDatabase(string databasePath, string directoryPath, List<List<FileInfo>> filesToHash, int fileTotal, CancellationToken cancellationToken = default)
    {
        var dictionary = new Dictionary<ulong, List<FileInfoSlim>>();

        int counter = 0;
        using (var db = new DuplicateFileFinderContext(databasePath))
        {
            await db.Database.MigrateAsync(cancellationToken);
            using var transaction = await db.Database.BeginTransactionAsync(cancellationToken);
            var version = DateTime.Now.ToBinary();

            foreach (var fileInfoList in filesToHash)
            {
                foreach (var fileInfo in fileInfoList)
                {
                    var relativePath = Path.GetRelativePath(directoryPath, fileInfo.FullName);
                    var dbFile = await db.FileInfoSlims.FindAsync(new object[] { relativePath }, cancellationToken);

                    if (dbFile is not null) // File exists in database.
                    {
                        if (fileInfo.LastWriteTimeUtc != dbFile.Modified) // Need to rehash.
                        {
                            var hash = await HashFile(fileInfo, cancellationToken);
                            if (hash.HasValue)
                            {
                                dbFile.Hash = hash.Value;
                                dbFile.Modified = fileInfo.LastWriteTimeUtc;
                                dbFile.Version = version;
                            }
                            else
                            {
                                db.FileInfoSlims.Remove(dbFile);
                                // TODO: Report skipped file.
                                Debug.WriteLine(fileInfo.FullName);
                                dbFile = null;
                            }
                        }
                        else
                        {
                            dbFile.Version = version;
                        }
                    }
                    else // New file.
                    {
                        var hash = await HashFile(fileInfo, cancellationToken);
                        if (hash.HasValue)
                        {
                            dbFile = new FileInfoSlim()
                            {
                                Path = relativePath,
                                Hash = hash.Value,
                                Modified = fileInfo.LastWriteTimeUtc,
                                Version = version
                            };
                            db.FileInfoSlims.Add(dbFile);
                        }
                        else
                        {
                            // TODO: Report skipped file.
                            Debug.WriteLine(fileInfo.FullName);
                        }
                    }

                    if (dbFile is not null)
                    {
                        if (dictionary.TryGetValue(dbFile.Hash, out var fileInfoSlimList))
                        {
                            fileInfoSlimList.Add(dbFile);
                        }
                        else
                        {
                            dictionary.Add(dbFile.Hash, new List<FileInfoSlim> { dbFile });
                        }
                    }

                    counter++;
                    _progress.Report((ProgressPhase.Hashing, fileTotal, counter));
                }
            }

            await db.SaveChangesAsync(cancellationToken);

            await db.FileInfoSlims.Where(fileInfoSlim => fileInfoSlim.Version != version).ExecuteDeleteAsync(cancellationToken);

            await transaction.CommitAsync(cancellationToken);
        }

        var list = new List<List<FileInfoSlim>>();
        counter = 0;
        foreach (var fileInfoSlimList in dictionary.Where(kvp => kvp.Value.Count > 1).Select(kvp => kvp.Value))
        {
            fileInfoSlimList.Sort((fi1, fi2) => fi1.Modified.CompareTo(fi2.Modified));
            list.Add(fileInfoSlimList);
            counter += fileInfoSlimList.Count - 1;
        }

        return (list, counter);
    }

    private async ValueTask<ulong?> HashFile(FileInfo file, CancellationToken cancellationToken = default)
    {
        try
        {
            using var stream = file.Open(FileMode.Open, FileAccess.Read, FileShare.Read);
            Debug.Assert(file.Length == stream.Length);

            await _hasher.Value.AppendAsync(stream, cancellationToken);
            var hash = _hasher.Value.GetCurrentHashAsUInt64();
            _hasher.Value.Reset();
            return hash;
        }
        catch (Exception)
        {
            return null;
        }
    }
}
