namespace DeepNestLib.IO
{
  public interface IRelativePathHelper
  {
    string GetSolutionDirectory();

    string ConvertToRelativePath(string path);

    public string ConvertToFullPath(string path);
  }
}