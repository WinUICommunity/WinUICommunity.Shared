using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Threading.Tasks;
using Windows.Storage;

namespace WinUICommunity.Shared.Internal;

internal static class FileLoader
{
    internal static async Task<string> LoadText(string relativeFilePath)
    {
        StorageFile file = null;
        if (InternalHelper.IsPackaged)
        {
            var sourceUri = new Uri("ms-appx:///" + relativeFilePath);
            file = await StorageFile.GetFileFromApplicationUriAsync(sourceUri);
        }
        else
        {
            var sourcePath = Path.GetFullPath(Path.Combine(Path.GetDirectoryName(Assembly.GetEntryAssembly().Location), relativeFilePath));
            file = await StorageFile.GetFileFromPathAsync(sourcePath);
        }

        return await FileIO.ReadTextAsync(file);
    }

    internal static async Task<IList<string>> LoadLines(string relativeFilePath)
    {
        var fileContents = await LoadText(relativeFilePath);
        return fileContents.Split(Environment.NewLine).ToList();
    }
}
