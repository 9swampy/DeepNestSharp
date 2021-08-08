namespace DeepNestSharp.Domain.Models
{
  public interface IWrapper<TCommonInterface, TIn>
    where TIn : class, TCommonInterface
  {
    TIn Item { get; }
  }
}