namespace DeepNestLib.NestProject
{
  using System.Collections.Generic;

  public class WrappableList<TCommonInterface, TConcrete> : List<TCommonInterface>, IList<TCommonInterface, TConcrete>
    where TConcrete : class, TCommonInterface
  {
    public WrappableList()
    {
    }

    public WrappableList(IList<TCommonInterface, TConcrete> items)
    {
      foreach (var item in items)
      {
        this.Add(item);
      }
    }

    public void Add(TConcrete item)
    {
      base.Add(item);
    }
  }
}