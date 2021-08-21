namespace DeepNestSharp.CiTests
{
  using System.Collections.Generic;
  using DeepNestLib.NestProject;
  using DeepNestSharp.Domain.Models;
  using FakeItEasy;
  using FluentAssertions;
  using Xunit;

  public class ObservableWrappableConcreteFixture
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
      var sut = new ObservableCollection<IDetailLoadInfo, DetailLoadInfo, ObservableDetailLoadInfo>(new WrappableList<IDetailLoadInfo, DetailLoadInfo>(), x => new ObservableDetailLoadInfo(x));
      sut.Add(new DetailLoadInfo());

      sut.Count.Should().Be(1);
    }

    [Fact]
    public void GivenObservableCollectionWhenAddITemThenCountOnWrappedShouldIncrement()
    {
      var sut = new ObservableCollection<IDetailLoadInfo, DetailLoadInfo, ObservableDetailLoadInfo>(new WrappableList<IDetailLoadInfo, DetailLoadInfo>(), x => new ObservableDetailLoadInfo(x));
      sut.Add(new DetailLoadInfo());

      sut.ItemsWrapped.Count.Should().Be(1);
    }

    [Fact]
    public void GivenWrappableListWhenObservableCollectionCreatedThenWrappedListCountMatches()
    {
      var items = new WrappableList<IDetailLoadInfo, DetailLoadInfo>();
      items.Add(new DetailLoadInfo());
      var sut = new ObservableCollection<IDetailLoadInfo, DetailLoadInfo, ObservableDetailLoadInfo>(items, x => new ObservableDetailLoadInfo(x));

      sut.ItemsWrapped.Count.Should().Be(1);
    }

    [Fact]
    public void GivenWrappableListWhenObservableCollectionCreatedThenCountMatches()
    {
      var items = new WrappableList<IDetailLoadInfo, DetailLoadInfo>();
      items.Add(new DetailLoadInfo());
      var sut = new ObservableCollection<IDetailLoadInfo, DetailLoadInfo, ObservableDetailLoadInfo>(items, x => new ObservableDetailLoadInfo(x));

      sut.Count.Should().Be(1);
    }

    [Fact]
    public void ObservableCollectionShouldNotBeReadOnly()
    {
      var sut = new ObservableCollection<IDetailLoadInfo, DetailLoadInfo, ObservableDetailLoadInfo>(x => new ObservableDetailLoadInfo(x));
      sut.IsReadOnly.Should().BeFalse();
    }

    [Fact]
    public void GivenWrappableListWhenIndexedThenWrappedItemShouldBeSame()
    {
      var items = new WrappableList<IDetailLoadInfo, DetailLoadInfo>();
      items.Add(new DetailLoadInfo());
      var sut = (IList<IDetailLoadInfo>)new ObservableCollection<IDetailLoadInfo, DetailLoadInfo, ObservableDetailLoadInfo>(items, x => new ObservableDetailLoadInfo(x));

      ((ObservableDetailLoadInfo)sut[0]).Item.Should().Be(items[0]);
    }

    [Fact]
    public void GivenWrappableListWhenInsertThenWrappedItemShouldBeSameAtIndex()
    {
      var items = new WrappableList<IDetailLoadInfo, DetailLoadInfo>();
      items.Add(new DetailLoadInfo());
      var sut = (IList<IDetailLoadInfo>)new ObservableCollection<IDetailLoadInfo, DetailLoadInfo, ObservableDetailLoadInfo>(items, x => new ObservableDetailLoadInfo(x));
      var inserted = new DetailLoadInfo();
      sut.Insert(0, inserted);

      ((ObservableDetailLoadInfo)sut[0]).Item.Should().Be(inserted);
    }

    [Fact]
    public void GivenWrappableListWhenInsertThenOriginalListShouldBeInsertedInToo()
    {
      var items = new WrappableList<IDetailLoadInfo, DetailLoadInfo>();
      items.Add(new DetailLoadInfo());
      var sut = (IList<IDetailLoadInfo>)new ObservableCollection<IDetailLoadInfo, DetailLoadInfo, ObservableDetailLoadInfo>(items, x => new ObservableDetailLoadInfo(x));
      var inserted = new DetailLoadInfo();
      sut.Insert(0, inserted);

      items.Count.Should().Be(2);
    }

    [Fact]
    public void GivenWrappableListWhenInsertThenOriginalShouldBeIndexIncremented()
    {
      var items = new WrappableList<IDetailLoadInfo, DetailLoadInfo>();
      items.Add(new DetailLoadInfo());
      var sut = (IList<IDetailLoadInfo>)new ObservableCollection<IDetailLoadInfo, DetailLoadInfo, ObservableDetailLoadInfo>(items, x => new ObservableDetailLoadInfo(x));
      var inserted = new DetailLoadInfo()
      {
        Quantity = 2
      };
      sut.Insert(0, inserted);

      ((ObservableDetailLoadInfo)sut[1]).Item.Should().Be(items[1], "the wrapped list is still the original list.");
    }

    [Fact]
    public void GivenWrappableListWhenIndexOfThenOriginalItemShouldBeLocated()
    {
      var items = new WrappableList<IDetailLoadInfo, DetailLoadInfo>();
      items.Add(new DetailLoadInfo());
      var sut = (IList<IDetailLoadInfo>)new ObservableCollection<IDetailLoadInfo, DetailLoadInfo, ObservableDetailLoadInfo>(items, x => new ObservableDetailLoadInfo(x));

      sut.IndexOf(items[0]).Should().Be(0);
    }

    [Fact]
    public void GivenWrappableListWhenIndexOfThenWrapperShouldBeLocated()
    {
      var items = new WrappableList<IDetailLoadInfo, DetailLoadInfo>();
      items.Add(new DetailLoadInfo());
      var sut = (IList<IDetailLoadInfo>)new ObservableCollection<IDetailLoadInfo, DetailLoadInfo, ObservableDetailLoadInfo>(items, x => new ObservableDetailLoadInfo(x));

      ((ObservableDetailLoadInfo)sut[0]).Item.Should().Be(items[0]);
      sut[0].Should().BeOfType<ObservableDetailLoadInfo>();
      if (sut[0] is ObservableDetailLoadInfo wrapper)
      {
        sut.IndexOf(wrapper).Should().Be(0);
      }
    }

    [Fact]
    public void GivenWrappableListWhenIndexOfNonConcreteThenShouldReturnFalse()
    {
      var items = new WrappableList<IDetailLoadInfo, DetailLoadInfo>();
      items.Add(new DetailLoadInfo());
      var sut = (IList<IDetailLoadInfo>)new ObservableCollection<IDetailLoadInfo, DetailLoadInfo, ObservableDetailLoadInfo>(items, x => new ObservableDetailLoadInfo(x));

      sut.IndexOf(A.Fake<IDetailLoadInfo>()).Should().Be(-1);
    }

    [Fact]
    public void GivenWrappableListWhenContainThenFindWrapped()
    {
      var items = new WrappableList<IDetailLoadInfo, DetailLoadInfo>();
      items.Add(new DetailLoadInfo());
      var sut = (IList<IDetailLoadInfo>)new ObservableCollection<IDetailLoadInfo, DetailLoadInfo, ObservableDetailLoadInfo>(items, x => new ObservableDetailLoadInfo(x));

      sut.Contains(items[0]).Should().BeTrue();
    }

    [Fact]
    public void GivenWrappableListWhenContainThenFindWrapper()
    {
      var items = new WrappableList<IDetailLoadInfo, DetailLoadInfo>();
      items.Add(new DetailLoadInfo());
      var sut = (IList<IDetailLoadInfo>)new ObservableCollection<IDetailLoadInfo, DetailLoadInfo, ObservableDetailLoadInfo>(items, x => new ObservableDetailLoadInfo(x));

      sut.Contains(sut[0]).Should().BeTrue();
    }

    [Fact]
    public void GivenWrappableListWhenContainNonConcreteThenReturnFalse()
    {
      var items = new WrappableList<IDetailLoadInfo, DetailLoadInfo>();
      items.Add(new DetailLoadInfo());
      var sut = (IList<IDetailLoadInfo>)new ObservableCollection<IDetailLoadInfo, DetailLoadInfo, ObservableDetailLoadInfo>(items, x => new ObservableDetailLoadInfo(x));

      sut.Contains(A.Fake<IDetailLoadInfo>()).Should().BeFalse();
    }

    [Fact]
    public void GivenWrappableListWhenCopyToThenReturnFalse()
    {
      var items = new WrappableList<IDetailLoadInfo, DetailLoadInfo>();
      items.Add(new DetailLoadInfo());
      var sut = (IList<IDetailLoadInfo>)new ObservableCollection<IDetailLoadInfo, DetailLoadInfo, ObservableDetailLoadInfo>(items, x => new ObservableDetailLoadInfo(x));

      var target = new IDetailLoadInfo[sut.Count];
      sut.CopyTo(target, 0);

      target[0].Should().Be(sut[0]);
    }

    [Fact]
    public void GivenWrappableListWhenRemoveWrappedThenConfirmRemove()
    {
      var items = new WrappableList<IDetailLoadInfo, DetailLoadInfo>();
      items.Add(new DetailLoadInfo());
      var toRemove = new DetailLoadInfo();
      items.Add(toRemove);
      var sut = (IList<IDetailLoadInfo>)new ObservableCollection<IDetailLoadInfo, DetailLoadInfo, ObservableDetailLoadInfo>(items, x => new ObservableDetailLoadInfo(x));


      sut.Remove(toRemove).Should().BeTrue();
    }

    [Fact]
    public void GivenWrappableListWhenRemoveWrapperThenConfirmRemove()
    {
      var items = new WrappableList<IDetailLoadInfo, DetailLoadInfo>();
      items.Add(new DetailLoadInfo());
      var toRemove = new DetailLoadInfo();
      items.Add(toRemove);
      var sut = (IList<IDetailLoadInfo>)new ObservableCollection<IDetailLoadInfo, DetailLoadInfo, ObservableDetailLoadInfo>(items, x => new ObservableDetailLoadInfo(x));

      sut.Remove(sut[0]).Should().BeTrue();
    }

    [Fact]
    public void GivenWrappableListWhenRemoveWrappedThenWrappedDecrementCount()
    {
      var items = new WrappableList<IDetailLoadInfo, DetailLoadInfo>();
      items.Add(new DetailLoadInfo());
      var toRemove = new DetailLoadInfo();
      items.Add(toRemove);
      var sut = (IList<IDetailLoadInfo>)new ObservableCollection<IDetailLoadInfo, DetailLoadInfo, ObservableDetailLoadInfo>(items, x => new ObservableDetailLoadInfo(x));

      sut.Remove(toRemove);

      sut.Count.Should().Be(1);
    }

    [Fact]
    public void GivenWrappableListWhenRemoveWrapperThenWrapperDecrementCount()
    {
      var items = new WrappableList<IDetailLoadInfo, DetailLoadInfo>();
      items.Add(new DetailLoadInfo());
      var toRemove = new DetailLoadInfo();
      items.Add(toRemove);
      var sut = (IList<IDetailLoadInfo>)new ObservableCollection<IDetailLoadInfo, DetailLoadInfo, ObservableDetailLoadInfo>(items, x => new ObservableDetailLoadInfo(x));

      sut[0].Should().BeOfType<ObservableDetailLoadInfo>();
      sut.Remove(sut[0]);

      sut.Count.Should().Be(1);
    }

    [Fact]
    public void GivenWrappableListWhenGetByIndexFromWrapperListThenReturnWrapper()
    {
      var items = new WrappableList<IDetailLoadInfo, DetailLoadInfo>();
      items.Add(new DetailLoadInfo());
      var toRemove = new DetailLoadInfo();
      items.Add(toRemove);
      var sut = (IList<IDetailLoadInfo>)new ObservableCollection<IDetailLoadInfo, DetailLoadInfo, ObservableDetailLoadInfo>(items, x => new ObservableDetailLoadInfo(x));

      sut[0].Should().BeOfType<ObservableDetailLoadInfo>();
    }

    [Fact]
    public void GivenWrappableListWhenRemoveNonConcreteThenReturnNotFound()
    {
      var items = new WrappableList<IDetailLoadInfo, DetailLoadInfo>();
      items.Add(new DetailLoadInfo());
      var toRemove = new DetailLoadInfo();
      items.Add(toRemove);
      var sut = (IList<IDetailLoadInfo>)new ObservableCollection<IDetailLoadInfo, DetailLoadInfo, ObservableDetailLoadInfo>(items, x => new ObservableDetailLoadInfo(x));

      sut.Remove(A.Fake<IDetailLoadInfo>()).Should().BeFalse();
    }
  }
}
