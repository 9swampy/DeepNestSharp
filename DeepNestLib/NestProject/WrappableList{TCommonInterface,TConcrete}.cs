namespace DeepNestLib.NestProject
{
  using System.Collections.Generic;

  public class WrappableList<TCommonInterface, TConcrete> : List<TCommonInterface>, IList<TCommonInterface, TConcrete>
    where TConcrete : class, TCommonInterface
  {
    public void Add(TConcrete item)
    {
      base.Add(item);
    }
  }
}