using System.Collections.Generic;
using System.Linq;
using Intellidesk.AcadNet.Common.Interfaces;

namespace FileExplorer.Extensions
{
    public static class FolderExtensions
    {
        public static List<string> ToList(this string fullPath)
        {
            var folders = fullPath.Split('\\').Select(x => x.ToUpper()).ToList();
            if (fullPath.EndsWith("\\"))
                folders.RemoveAt(folders.Count - 1);
            return folders;
        }
        public static List<string> ToList(this IFolder folder)
        {
            var folders = folder.FullPath.Split('\\').ToList();
            if (folder.FullPath.EndsWith("\\"))
                folders.RemoveAt(folders.Count - 1);
            return folders;
        }

        public static IFolder Find(this IFolder folder, string fullPath)
        {
            if (folder.Folders != null && folder.Folders.Any())
                foreach (var localFolder in folder.Folders)
                {
                    if (localFolder.FullPath.ToUpper() == fullPath.ToUpper())
                        return localFolder;

                    Find(localFolder, fullPath);
                }
            return null;
        }

        public static IEnumerable<IFolder> FindFlatten(this IEnumerable<IFolder> items, string fullPath)
        {
            var enumerable = items as IList<IFolder> ?? items.ToList();
            return enumerable.SelectMany(x => FindFlatten(x.Folders.Where(f => f.FullPath.ToLower() == fullPath.ToLower()), fullPath)).Concat(enumerable);
        }

        public static IEnumerable<IFolder> FindFlatten(this IFolder root, string fullPath)
        {
            var flattened = new List<IFolder> { root };
            var children = root.Folders;
            if (children != null)
            {
                foreach (var child in children)
                {
                    flattened.AddRange(FindFlatten(child, fullPath));
                }
            }
            return flattened.Where(x => x.FullPath == fullPath);
        }

        public static IEnumerable<IFolder> GetTreeFolders(this IFolder rootNode)
        {
            yield return rootNode;
            //return rootNode.Folders.SelectMany(childNode => childNode.GetTreeFolders());
            foreach (var childNode in rootNode.Folders)
            {
                foreach (var child in childNode.GetTreeFolders())
                    yield return child;
            }
        }
    }
}
