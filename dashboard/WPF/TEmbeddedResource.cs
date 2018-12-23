using System;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;

namespace System
{
    public static class TEmbeddedResource
    {
        static TEmbeddedResource()
        {
            var asm = Assembly.GetEntryAssembly();
            string resName = asm.GetName().Name + ".g.resources";
            using (var stream = asm.GetManifestResourceStream(resName))
            using (var reader = new System.Resources.ResourceReader(stream))
            {
                Resources = reader.Cast<DictionaryEntry>().Where(t => t.Value is Stream).ToDictionary(t => t.Key.ToString(), k => (Stream)k.Value);
            }
        }
        public static Dictionary<string, Stream> Resources { get; private set; }
        public static Stream GetResource(string name)
        {
            return Resources.FirstOrDefault(t => t.Key.EndsWith(name, StringComparison.InvariantCultureIgnoreCase)).Value;
        }
        ///// <summary>
        ///// Extracts an embedded file out of a given assembly.
        ///// </summary>
        ///// <param name="assemblyName">The namespace of your assembly.</param>
        ///// <param name="fileName">The name of the file to extract.</param>
        ///// <returns>A stream containing the file data.</returns>
        //public static Stream Open(string assemblyName, string fileName)
        //{
        //    return Assembly.Load(assemblyName)?.OpenFileStream(fileName);
        //}
        //public static Stream OpenFileStream(this Assembly assembly, string fileName)
        //{
        //    return assembly.GetManifestResourceStream(assembly.GetName().Name + "." + fileName);
        //}
        //public static Stream OpenFileStream(string fileName)
        //{
        //    return GetResource( Assembly.GetCallingAssembly()?.OpenFileStream(fileName);
        //}
        public static byte[] OpenFileByteArray(string fileName)
        {
            return GetResource(fileName)?.ToByteArray();
        }
        public static byte[] ToByteArray(this Stream input)
        {
            using (MemoryStream ms = new MemoryStream())
            {
                input.Seek(0, SeekOrigin.Begin);
                input.CopyTo(ms);
                return ms.ToArray();
            }
        }

    }
}
