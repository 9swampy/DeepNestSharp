namespace DeepNestLib
{
  using System;
  using System.IO;
  using System.Text.Json;
  using System.Text.Json.Serialization;
#if NCRUNCH
  using System.Text;
#endif

  public class SheetNfp : NfpCandidateList
  {
    public new const string FileDialogFilter = "DeepNest SheetNfp (*.dnsnfp)|*.dnsnfp|All files (*.*)|*.*";

    public SheetNfp(NfpCandidateList nfpCandidateList)
      : base(nfpCandidateList.Items, nfpCandidateList.Sheet, nfpCandidateList.Part)
    {
    }

    [JsonConstructor]
    public SheetNfp(INfp[] items, ISheet sheet, INfp part)
      : base(items, sheet, part)
    {
    }

    // Inner NFP aka SheetNfp
    public SheetNfp(INfpHelper nfpHelper, ISheet sheet, INfp part, double clipperScale)
      : this(nfpHelper.GetInnerNfp(sheet, part, MinkowskiCache.Cache, clipperScale) ?? new INfp[0], sheet, part)
    {
    }

    internal bool CanAcceptPart
    {
      get
      {
        if (this.Length > 0)
        {
          if (this[0].Length == 0)
          {
            throw new ArgumentException();
          }
          else
          {
            return true;
          }
        }

        return false;
      }
    }

    public static new SheetNfp FromJson(string json)
    {
      var options = new JsonSerializerOptions();
      options.Converters.Add(new SheetJsonConverter());
      options.Converters.Add(new NfpJsonConverter());
      var nfpCandidateList = JsonSerializer.Deserialize<NfpCandidateList>(json, options);
      return new SheetNfp(nfpCandidateList);
    }

    public static new SheetNfp LoadFromFile(string fileName)
    {
      using (StreamReader inputFile = new StreamReader(fileName))
      {
        return FromJson(inputFile.ReadToEnd());
      }
    }
  }
}