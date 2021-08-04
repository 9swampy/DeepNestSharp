namespace DeepNestSharp.CiTests
{
  using System;
  using System.Collections.ObjectModel;
  using System.Threading.Tasks;
  using Xunit;
  
  public class UnitTest1
  {
    ObservableCollection<int> collection;

    [Fact]
    public void Test1()
    {
      collection = new ObservableCollection<int>();
      var t = new Task(() => Worker());
      t.Start();
    }

    public void Worker()
    {
      collection.Add(1);
    }
  }
}
