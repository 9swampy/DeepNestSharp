namespace DeepNestLib.NestProject
{
  public interface ISheetLoadInfo
  {
    double Height { get; set; }

    string Path { get; set; }

    int Quantity { get; set; }

    SheetTypeEnum SheetType { get; }

    double Width { get; set; }
  }
}