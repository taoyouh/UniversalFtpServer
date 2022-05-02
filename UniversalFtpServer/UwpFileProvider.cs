using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Windows.Storage;
using Zhaobang.FtpServer.File;

namespace UniversalFtpServer
{
    class UwpFileProvider : IFileProvider
    {
        string rootFolder;
        string workFolder = string.Empty;

        public UwpFileProvider(string rootFolder)
        {
            this.rootFolder = rootFolder;
        }

        public async Task CreateDirectoryAsync(string path)
        {
            string fullPath = GetLocalPath(path);
            string parentPath = Path.GetDirectoryName(fullPath);
            string name = Path.GetFileName(fullPath);
            StorageFolder parent = await StorageFolder.GetFolderFromPathAsync(parentPath);
            await parent.CreateFolderAsync(name);
        }

        public async Task<Stream> CreateFileForWriteAsync(string path)
        {
            string fullPath = GetLocalPath(path);
            string parentPath = Path.GetDirectoryName(fullPath);
            string name = Path.GetFileName(fullPath);
            StorageFolder parent = await StorageFolder.GetFolderFromPathAsync(parentPath);
            StorageFile file = await parent.CreateFileAsync(name, CreationCollisionOption.ReplaceExisting);
            return await file.OpenStreamForWriteAsync();
        }

        public async Task DeleteAsync(string path)
        {
            string fullPath = GetLocalPath(path);
            StorageFile file = await StorageFile.GetFileFromPathAsync(fullPath);
            await file.DeleteAsync();
        }

        public async Task DeleteDirectoryAsync(string path)
        {
            string fullPath = GetLocalPath(path);
            StorageFolder folder = await StorageFolder.GetFolderFromPathAsync(fullPath);
            await folder.DeleteAsync();
        }

        public async Task<IEnumerable<FileSystemEntry>> GetListingAsync(string path)
        {
            string fullPath = GetLocalPath(path);
            StorageFolder folder = await StorageFolder.GetFolderFromPathAsync(fullPath);
            List<FileSystemEntry> result = new List<FileSystemEntry>();
            foreach (var item in await folder.GetItemsAsync())
            {
                var properties = await item.GetBasicPropertiesAsync();
                FileSystemEntry entry = new FileSystemEntry()
                {
                    IsDirectory = item.IsOfType(StorageItemTypes.Folder),
                    IsReadOnly = item.Attributes.HasFlag(Windows.Storage.FileAttributes.ReadOnly),
                    LastWriteTime = properties.DateModified.ToUniversalTime().DateTime,
                    Length = (long)properties.Size,
                    Name = item.Name
                };
                result.Add(entry);
            }
            return result;
        }

        public async Task<IEnumerable<string>> GetNameListingAsync(string path)
        {
            string fullPath = GetLocalPath(path);
            StorageFolder folder = await StorageFolder.GetFolderFromPathAsync(fullPath);
            List<string> result = new List<string>();
            foreach (var item in await folder.GetItemsAsync())
            {
                result.Add(item.Name);
            }
            return result;
        }

        public string GetWorkingDirectory()
        {
            return "/" + workFolder;
        }

        public async Task<Stream> OpenFileForReadAsync(string path)
        {
            string fullPath = GetLocalPath(path);
            var file = await StorageFile.GetFileFromPathAsync(fullPath);
            return await file.OpenStreamForReadAsync();
        }

        public async Task<Stream> OpenFileForWriteAsync(string path)
        {
            string fullPath = GetLocalPath(path);
            var file = await StorageFile.GetFileFromPathAsync(fullPath);
            return await file.OpenStreamForWriteAsync();
        }

        public async Task RenameAsync(string fromPath, string toPath)
        {
            string fromFullPath = GetLocalPath(fromPath);
            string toFullPath = GetLocalPath(toPath);

            IStorageItem item = null;
            try
            {
                item = await StorageFile.GetFileFromPathAsync(fromFullPath);
                goto rename;
            }
            catch { }
            try
            {
                item = await StorageFolder.GetFolderFromPathAsync(fromFullPath);
            }
            catch { }
            if (item == null)
            {
                throw new FileNoAccessException("Can't find the item to rename");
            }

        rename:
            if (Path.GetDirectoryName(fromFullPath) != Path.GetDirectoryName(toFullPath))
            {
                string toFullPathParent = Path.GetDirectoryName(toFullPath);
                StorageFolder destinationFolder = await StorageFolder.GetFolderFromPathAsync(toFullPathParent);
                if (item is IStorageFile file)
                {
                    await file.MoveAsync(destinationFolder, Path.GetFileName(toFullPath));
                }
                else if (item is IStorageFolder folder)
                {
                    if (!(await MoveFolder(folder, destinationFolder)))
                        throw new FileBusyException("Some items can't be moved");
                }
                else
                {
                    throw new FileBusyException("Items of unknown type can't be moved");
                }
            }
            else
            {
                await item.RenameAsync(Path.GetFileName(toPath));
            }
        }

        private async Task<bool> MoveFolder(IStorageFolder folder, IStorageFolder destination)
        {
            foreach (var file in await folder.GetFilesAsync())
            {
                await file.MoveAsync(destination);
            }
            foreach (var subFolder in await folder.GetFoldersAsync())
            {
                var destSubFolder = await destination.CreateFolderAsync(subFolder.Name);
                await MoveFolder(subFolder, destSubFolder);
            }
            if (!(await folder.GetItemsAsync()).Any())
            {
                await folder.DeleteAsync();
                return true;
            }
            else
            {
                return false;
            }
        }

        public bool SetWorkingDirectory(string path)
        {
            try
            {
                var localPath = GetLocalPath(path);
                var folderTask = StorageFolder.GetFolderFromPathAsync(localPath);
                folderTask.AsTask().Wait();
                if (folderTask.Status == Windows.Foundation.AsyncStatus.Completed)
                {
                    workFolder = GetFtpPath(localPath);
                    return true;
                }
                else
                {
                    return false;
                }
            }
            catch
            {
                return false;
            }
        }

        /// <exception cref="ArgumentException"/>
        /// <exception cref="ArgumentNullException"/>
        /// <exception cref="UnauthorizedAccessException"/>
        /// <exception cref="SecurityException"/>
        /// <exception cref="NotSupportedException"/>
        /// <exception cref="PathTooLongException"/>
        private string GetLocalPath(string path)
        {
            string fullPath = Path.Combine(workFolder, path).TrimStart('/', '\\');
            string localPath = Path.GetFullPath(Path.Combine(rootFolder, fullPath)).TrimEnd('/', '\\');
            string baseLocalPath = Path.GetFullPath(rootFolder).TrimEnd('/', '\\');
            if (!Path.GetFullPath(localPath).Contains(baseLocalPath))
                throw new UnauthorizedAccessException("User tried to access out of base directory");
            return localPath;
        }

        private string GetFtpPath(string localPath)
        {
            string localFullPath = Path.GetFullPath(localPath).TrimEnd('/', '\\');
            string baseFullPath = Path.GetFullPath(rootFolder).TrimEnd('/', '\\');
            if (!localFullPath.Contains(baseFullPath))
            {
                throw new UnauthorizedAccessException("User tried to access out of base directory");
            }
            return localFullPath.Replace(baseFullPath, string.Empty).TrimStart('/', '\\').Replace('\\', '/');
        }
    }
}
