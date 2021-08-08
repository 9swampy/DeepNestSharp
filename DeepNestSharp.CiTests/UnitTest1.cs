namespace DeepNestSharp.CiTests
{
  using System;
  using System.Collections.Generic;
  using System.Collections.ObjectModel;
  using System.Threading.Tasks;
  using DeepNestLib.NestProject;
  using DeepNestSharp.Ui.Models;
  using FluentAssertions;
  using Xunit;

  public class TestAddWrappableConcrete
  {
    [Fact]
    public void GivenWrappableListWhenAddITemThenCountShouldIncrement()
    {
      var sut = new WrappableList<IDetailLoadInfo, DetailLoadInfo>();
      sut.Add(new DetailLoadInfo());

      sut.Count.Should().Be(1);
    }

    [Fact]
    public void GivenObservableCollectionWhenAddITemThenCountShouldIncrement()
    {
      var sut = new ObservableCollection<IDetailLoadInfo, DetailLoadInfo, ObservableDetailLoadInfo>(new List<IDetailLoadInfo>(), x => new ObservableDetailLoadInfo(x));
      sut.Add(new DetailLoadInfo());

      sut.Count.Should().Be(1);
    }

    [Fact]
    public void GivenObservableCollectionWhenAddITemThenCountOnWrappedShouldIncrement()
    {
      var sut = new ObservableCollection<IDetailLoadInfo, DetailLoadInfo, ObservableDetailLoadInfo>(new List<IDetailLoadInfo>(), x => new ObservableDetailLoadInfo(x));
      sut.Add(new DetailLoadInfo());

      sut.items.Count.Should().Be(1);
    }
  }

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
