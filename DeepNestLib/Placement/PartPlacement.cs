namespace DeepNestLib.Placement
{
  using System;
  using System.Linq;
  using System.Text.Json;
  using System.Text.Json.Serialization;

  public class PartPlacement : IPartPlacement
  {
    public PartPlacement(INfp part)
    {
      this.Part = part;
    }

    [JsonIgnore]
    public double? MergedLength
    {
      get
      {
        return null;
      }

      set
      {
        throw new NotImplementedException();
      }
    }

    [JsonIgnore]
    public object MergedSegments
    {
      get
      {
        return null;
      }

      set
      {
        throw new NotImplementedException();
      }
    }

    public int Id { get; set; }

    /// <summary>
    /// A hull of the part captured only when not Gravity or BoundingBox (ie. Squeeze).
    /// </summary>
    [JsonIgnore]
    public INfp Hull { get; set; }

    /// <summary>
    /// A hull of the sheet captured only when not Gravity or BoundingBox (ie. Squeeze).
    /// </summary>
    [JsonIgnore]
    public INfp HullSheet { get; set; }

    [JsonIgnore]
    public double MaxX => this.X + this.Part.Points.Max(p => p.X);

    [JsonIgnore]
    public double MaxY => this.Y + this.Part.Points.Max(p => p.Y);

    [JsonIgnore]
    public double MinX => this.X + this.Part.Points.Min(p => p.X);

    [JsonIgnore]
    public double MinY => this.Y + this.Part.Points.Min(p => p.Y);

    /// <summary>
    /// Rotation of the part (sheets I don't think ever get rotated, so this would be absolute).
    /// </summary>
    [JsonConverter(typeof(DoublePrecisionConverter))]
    public double Rotation { get; set; }

    /// <summary>
    /// Offset of the part relative to the sheet.
    /// </summary>
    [JsonConverter(typeof(DoublePrecisionConverter))]
    public double X { get; set; }

    /// <summary>
    /// Offset of the part relative to the sheet.
    /// </summary>
    [JsonConverter(typeof(DoublePrecisionConverter))]
    public double Y { get; set; }

    /// <summary>
    /// Source of the part placed.
    /// </summary>
    public int Source { get; set; }

    public INfp Part { get; }
  }
}
