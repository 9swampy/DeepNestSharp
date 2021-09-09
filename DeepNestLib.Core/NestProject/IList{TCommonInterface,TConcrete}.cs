namespace DeepNestLib.NestProject
{
  using System.Collections.Generic;

  public interface IList<TCommonInterface, TConcrete> : IList<TCommonInterface>
    where TConcrete : class, TCommonInterface
  {
    void Add(TConcrete item);
  }
}
