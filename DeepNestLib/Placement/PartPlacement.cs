namespace DeepNestLib.Placement
{
  using System;
  using System.Linq;
  using System.Text.Json.Serialization;

  public class PartPlacement : IPartPlacement
  {
    public PartPlacement(INfp part)
    {
      this.Part = part;
    }

    /// <inheritdoc />
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

    /// <inheritdoc />
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

    /// <inheritdoc />
    public int Id { get; set; }

    /// <inheritdoc />
    [JsonIgnore]
    public bool IsExact => Part.IsExact;

    /// <inheritdoc />
    [JsonIgnore]
    public bool IsDragging { get; set; }

    /// <inheritdoc />
    [JsonIgnore]
    public INfp Hull { get; set; }

    /// <inheritdoc />
    [JsonIgnore]
    public INfp HullSheet { get; set; }

    /// <inheritdoc />
    [JsonIgnore]
    public double MaxX => this.X + this.Part.Points.Max(p => p.X);

    /// <inheritdoc />
    [JsonIgnore]
    public double MaxY => this.Y + this.Part.Points.Max(p => p.Y);

    /// <inheritdoc />
    [JsonIgnore]
    public double MinX => this.X + this.Part.Points.Min(p => p.X);

    /// <inheritdoc />
    [JsonIgnore]
    public double MinY => this.Y + this.Part.Points.Min(p => p.Y);

    /// <inheritdoc />
    [JsonConverter(typeof(DoublePrecisionConverter))]
    public double Rotation { get; set; }

    /// <inheritdoc />
    [JsonConverter(typeof(DoublePrecisionConverter))]
    public double X { get; set; }

    /// <inheritdoc />
    [JsonConverter(typeof(DoublePrecisionConverter))]
    public double Y { get; set; }

    /// <inheritdoc />
    public int Source { get; set; }

    /// <inheritdoc />
    public INfp Part { get; }
  }
}
