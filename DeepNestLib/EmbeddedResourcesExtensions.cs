namespace DeepNestLib
{
  using System;
  using System.IO;
  using System.Reflection;
  using System.Text.RegularExpressions;

  public static class EmbeddedResourcesExtensions
  {
    public static Stream GetEmbeddedResourceStream(this Assembly assembly, string relativeResourcePath)
    {
      if (string.IsNullOrEmpty(relativeResourcePath))
      {
        throw new ArgumentNullException("relativeResourcePath");
      }

      var resourcePath = string.Format("{0}.{1}", Regex.Replace(assembly.ManifestModule.Name, @"\.(exe|dll)$", string.Empty, RegexOptions.IgnoreCase), relativeResourcePath);
      var stream = assembly.GetManifestResourceStream(resourcePath);
      if (stream == null)
      {
        throw new ArgumentException(string.Format("The specified embedded resource \"{0}\" is not found.", relativeResourcePath));
      }

      return stream;
    }
  }
}
