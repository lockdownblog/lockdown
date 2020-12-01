using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Lockdown.Tests
{
    static class TestingUtils
    {

        public static IEnumerable<string> GetFilesIncludingSubfolders(string path)
        {
            var paths = new List<string>();

            if (!Directory.Exists(path))
            {
                return paths;
            }

            var directories = Directory.GetDirectories(path);

            foreach (var directory in directories)
            {
                paths.AddRange(GetFilesIncludingSubfolders(directory));
            }

            paths.AddRange(Directory.GetFiles(path).ToList());
            return paths;
        }
    }
}
