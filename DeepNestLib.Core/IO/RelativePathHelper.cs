namespace DeepNestLib.IO
{
  using System.IO;

  public class RelativePathHelper : IRelativePathHelper
  {
    public const string SolutionDirSubstitution = "${SolutionDir}";

    private readonly string applicationResourceAssemblyLocation;

    public RelativePathHelper(string applicationResourceAssemblyLocation)
    {
      this.applicationResourceAssemblyLocation = applicationResourceAssemblyLocation;
    }

    public string ConvertToRelativePath(string path)
    {
      return path.Replace(GetSolutionDirectory(), SolutionDirSubstitution);
    }

    public string ConvertToFullPath(string path)
    {
      return path.Replace(SolutionDirSubstitution, GetSolutionDirectory());
    }

    public string GetSolutionDirectory()
    {
      var dirInfo = new FileInfo(applicationResourceAssemblyLocation).Directory;
      if (dirInfo == null || dirInfo?.Name == null || !dirInfo.Exists)
      {
        return applicationResourceAssemblyLocation;
      }

      while (dirInfo.Name != "DeepNestSharp" && !dirInfo.Name.EndsWith(".CiTests"))
      {
        dirInfo = dirInfo?.Parent;
      }

      return dirInfo == null || dirInfo.Parent == null
                      ? applicationResourceAssemblyLocation
                      : dirInfo.Parent.FullName;
    }
  }
}
