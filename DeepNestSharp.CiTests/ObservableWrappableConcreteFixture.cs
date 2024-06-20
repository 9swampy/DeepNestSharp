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
      WrappableList<IDetailLoadInfo, DetailLoadInfo> sut = new WrappableList<IDetailLoadInfo, DetailLoadInfo>();
      sut.Add(new DetailLoadInfo());

      sut.Count.Should().Be(1);
    }

    [Fact]
    public void GivenObservableCollectionWhenAddITemThenCountShouldIncrement()
    {
      ObservableCollection<IDetailLoadInfo, DetailLoadInfo, ObservableDetailLoadInfo> sut = new ObservableCollection<IDetailLoadInfo, DetailLoadInfo, ObservableDetailLoadInfo>(new WrappableList<IDetailLoadInfo, DetailLoadInfo>(), x => new ObservableDetailLoadInfo(x));
      sut.Add(new DetailLoadInfo());

      sut.Count.Should().Be(1);
    }

    [Fact]
    public void GivenObservableCollectionWhenAddITemThenCountOnWrappedShouldIncrement()
    {
      ObservableCollection<IDetailLoadInfo, DetailLoadInfo, ObservableDetailLoadInfo> sut = new ObservableCollection<IDetailLoadInfo, DetailLoadInfo, ObservableDetailLoadInfo>(new WrappableList<IDetailLoadInfo, DetailLoadInfo>(), x => new ObservableDetailLoadInfo(x));
      sut.Add(new DetailLoadInfo());

      sut.ItemsWrapped.Count.Should().Be(1);
    }

    [Fact]
    public void GivenWrappableListWhenObservableCollectionCreatedThenWrappedListCountMatches()
    {
      WrappableList<IDetailLoadInfo, DetailLoadInfo> items = new WrappableList<IDetailLoadInfo, DetailLoadInfo>();
      items.Add(new DetailLoadInfo());
      ObservableCollection<IDetailLoadInfo, DetailLoadInfo, ObservableDetailLoadInfo> sut = new ObservableCollection<IDetailLoadInfo, DetailLoadInfo, ObservableDetailLoadInfo>(items, x => new ObservableDetailLoadInfo(x));

      sut.ItemsWrapped.Count.Should().Be(1);
    }

    [Fact]
    public void GivenWrappableListWhenObservableCollectionCreatedThenCountMatches()
    {
      WrappableList<IDetailLoadInfo, DetailLoadInfo> items = new WrappableList<IDetailLoadInfo, DetailLoadInfo>();
      items.Add(new DetailLoadInfo());
      ObservableCollection<IDetailLoadInfo, DetailLoadInfo, ObservableDetailLoadInfo> sut = new ObservableCollection<IDetailLoadInfo, DetailLoadInfo, ObservableDetailLoadInfo>(items, x => new ObservableDetailLoadInfo(x));

      sut.Count.Should().Be(1);
    }

    [Fact]
    public void ObservableCollectionShouldNotBeReadOnly()
    {
      ObservableCollection<IDetailLoadInfo, DetailLoadInfo, ObservableDetailLoadInfo> sut = new ObservableCollection<IDetailLoadInfo, DetailLoadInfo, ObservableDetailLoadInfo>(x => new ObservableDetailLoadInfo(x));
      sut.IsReadOnly.Should().BeFalse();
    }

    [Fact]
    public void GivenWrappableListWhenIndexedThenWrappedItemShouldBeSame()
    {
      WrappableList<IDetailLoadInfo, DetailLoadInfo> items = new WrappableList<IDetailLoadInfo, DetailLoadInfo>();
      items.Add(new DetailLoadInfo());
      IList<IDetailLoadInfo> sut = (IList<IDetailLoadInfo>)new ObservableCollection<IDetailLoadInfo, DetailLoadInfo, ObservableDetailLoadInfo>(items, x => new ObservableDetailLoadInfo(x));

      ((ObservableDetailLoadInfo)sut[0]).Item.Should().Be(items[0]);
    }

    [Fact]
    public void GivenWrappableListWhenInsertThenWrappedItemShouldBeSameAtIndex()
    {
      WrappableList<IDetailLoadInfo, DetailLoadInfo> items = new WrappableList<IDetailLoadInfo, DetailLoadInfo>();
      items.Add(new DetailLoadInfo());
      IList<IDetailLoadInfo> sut = (IList<IDetailLoadInfo>)new ObservableCollection<IDetailLoadInfo, DetailLoadInfo, ObservableDetailLoadInfo>(items, x => new ObservableDetailLoadInfo(x));
      DetailLoadInfo inserted = new DetailLoadInfo();
      sut.Insert(0, inserted);

      ((ObservableDetailLoadInfo)sut[0]).Item.Should().Be(inserted);
    }

    [Fact]
    public void GivenWrappableListWhenInsertThenOriginalListShouldBeInsertedInToo()
    {
      WrappableList<IDetailLoadInfo, DetailLoadInfo> items = new WrappableList<IDetailLoadInfo, DetailLoadInfo>();
      items.Add(new DetailLoadInfo());
      IList<IDetailLoadInfo> sut = (IList<IDetailLoadInfo>)new ObservableCollection<IDetailLoadInfo, DetailLoadInfo, ObservableDetailLoadInfo>(items, x => new ObservableDetailLoadInfo(x));
      DetailLoadInfo inserted = new DetailLoadInfo();
      sut.Insert(0, inserted);

      items.Count.Should().Be(2);
    }

    [Fact]
    public void GivenWrappableListWhenInsertThenOriginalShouldBeIndexIncremented()
    {
      WrappableList<IDetailLoadInfo, DetailLoadInfo> items = new WrappableList<IDetailLoadInfo, DetailLoadInfo>();
      items.Add(new DetailLoadInfo());
      IList<IDetailLoadInfo> sut = (IList<IDetailLoadInfo>)new ObservableCollection<IDetailLoadInfo, DetailLoadInfo, ObservableDetailLoadInfo>(items, x => new ObservableDetailLoadInfo(x));
      DetailLoadInfo inserted = new DetailLoadInfo()
      {
        Quantity = 2
      };
      sut.Insert(0, inserted);

      ((ObservableDetailLoadInfo)sut[1]).Item.Should().Be(items[1], "the wrapped list is still the original list.");
    }

    [Fact]
    public void GivenWrappableListWhenIndexOfThenOriginalItemShouldBeLocated()
    {
      WrappableList<IDetailLoadInfo, DetailLoadInfo> items = new WrappableList<IDetailLoadInfo, DetailLoadInfo>();
      items.Add(new DetailLoadInfo());
      IList<IDetailLoadInfo> sut = (IList<IDetailLoadInfo>)new ObservableCollection<IDetailLoadInfo, DetailLoadInfo, ObservableDetailLoadInfo>(items, x => new ObservableDetailLoadInfo(x));

      sut.IndexOf(items[0]).Should().Be(0);
    }

    [Fact]
    public void GivenWrappableListWhenIndexOfThenWrapperShouldBeLocated()
    {
      WrappableList<IDetailLoadInfo, DetailLoadInfo> items = new WrappableList<IDetailLoadInfo, DetailLoadInfo>();
      items.Add(new DetailLoadInfo());
      IList<IDetailLoadInfo> sut = (IList<IDetailLoadInfo>)new ObservableCollection<IDetailLoadInfo, DetailLoadInfo, ObservableDetailLoadInfo>(items, x => new ObservableDetailLoadInfo(x));

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
      WrappableList<IDetailLoadInfo, DetailLoadInfo> items = new WrappableList<IDetailLoadInfo, DetailLoadInfo>();
      items.Add(new DetailLoadInfo());
      IList<IDetailLoadInfo> sut = (IList<IDetailLoadInfo>)new ObservableCollection<IDetailLoadInfo, DetailLoadInfo, ObservableDetailLoadInfo>(items, x => new ObservableDetailLoadInfo(x));

      sut.IndexOf(A.Fake<IDetailLoadInfo>()).Should().Be(-1);
    }

    [Fact]
    public void GivenWrappableListWhenContainThenFindWrapped()
    {
      WrappableList<IDetailLoadInfo, DetailLoadInfo> items = new WrappableList<IDetailLoadInfo, DetailLoadInfo>();
      items.Add(new DetailLoadInfo());
      IList<IDetailLoadInfo> sut = (IList<IDetailLoadInfo>)new ObservableCollection<IDetailLoadInfo, DetailLoadInfo, ObservableDetailLoadInfo>(items, x => new ObservableDetailLoadInfo(x));

      sut.Contains(items[0]).Should().BeTrue();
    }

    [Fact]
    public void GivenWrappableListWhenContainThenFindWrapper()
    {
      WrappableList<IDetailLoadInfo, DetailLoadInfo> items = new WrappableList<IDetailLoadInfo, DetailLoadInfo>();
      items.Add(new DetailLoadInfo());
      IList<IDetailLoadInfo> sut = (IList<IDetailLoadInfo>)new ObservableCollection<IDetailLoadInfo, DetailLoadInfo, ObservableDetailLoadInfo>(items, x => new ObservableDetailLoadInfo(x));

      sut.Contains(sut[0]).Should().BeTrue();
    }

    [Fact]
    public void GivenWrappableListWhenContainNonConcreteThenReturnFalse()
    {
      WrappableList<IDetailLoadInfo, DetailLoadInfo> items = new WrappableList<IDetailLoadInfo, DetailLoadInfo>();
      items.Add(new DetailLoadInfo());
      IList<IDetailLoadInfo> sut = (IList<IDetailLoadInfo>)new ObservableCollection<IDetailLoadInfo, DetailLoadInfo, ObservableDetailLoadInfo>(items, x => new ObservableDetailLoadInfo(x));

      sut.Contains(A.Fake<IDetailLoadInfo>()).Should().BeFalse();
    }

    [Fact]
    public void GivenWrappableListWhenCopyToThenReturnFalse()
    {
      WrappableList<IDetailLoadInfo, DetailLoadInfo> items = new WrappableList<IDetailLoadInfo, DetailLoadInfo>();
      items.Add(new DetailLoadInfo());
      IList<IDetailLoadInfo> sut = (IList<IDetailLoadInfo>)new ObservableCollection<IDetailLoadInfo, DetailLoadInfo, ObservableDetailLoadInfo>(items, x => new ObservableDetailLoadInfo(x));

      IDetailLoadInfo[] target = new IDetailLoadInfo[sut.Count];
      sut.CopyTo(target, 0);

      target[0].Should().Be(sut[0]);
    }

    [Fact]
    public void GivenWrappableListWhenRemoveWrappedThenConfirmRemove()
    {
      WrappableList<IDetailLoadInfo, DetailLoadInfo> items = new WrappableList<IDetailLoadInfo, DetailLoadInfo>();
      items.Add(new DetailLoadInfo());
      DetailLoadInfo toRemove = new DetailLoadInfo();
      items.Add(toRemove);
      IList<IDetailLoadInfo> sut = (IList<IDetailLoadInfo>)new ObservableCollection<IDetailLoadInfo, DetailLoadInfo, ObservableDetailLoadInfo>(items, x => new ObservableDetailLoadInfo(x));


      sut.Remove(toRemove).Should().BeTrue();
    }

    [Fact]
    public void GivenWrappableListWhenRemoveWrapperThenConfirmRemove()
    {
      WrappableList<IDetailLoadInfo, DetailLoadInfo> items = new WrappableList<IDetailLoadInfo, DetailLoadInfo>();
      items.Add(new DetailLoadInfo());
      DetailLoadInfo toRemove = new DetailLoadInfo();
      items.Add(toRemove);
      IList<IDetailLoadInfo> sut = (IList<IDetailLoadInfo>)new ObservableCollection<IDetailLoadInfo, DetailLoadInfo, ObservableDetailLoadInfo>(items, x => new ObservableDetailLoadInfo(x));

      sut.Remove(sut[0]).Should().BeTrue();
    }

    [Fact]
    public void GivenWrappableListWhenRemoveWrappedThenWrappedDecrementCount()
    {
      WrappableList<IDetailLoadInfo, DetailLoadInfo> items = new WrappableList<IDetailLoadInfo, DetailLoadInfo>();
      items.Add(new DetailLoadInfo());
      DetailLoadInfo toRemove = new DetailLoadInfo();
      items.Add(toRemove);
      IList<IDetailLoadInfo> sut = (IList<IDetailLoadInfo>)new ObservableCollection<IDetailLoadInfo, DetailLoadInfo, ObservableDetailLoadInfo>(items, x => new ObservableDetailLoadInfo(x));

      sut.Remove(toRemove);

      sut.Count.Should().Be(1);
    }

    [Fact]
    public void GivenWrappableListWhenRemoveWrapperThenWrapperDecrementCount()
    {
      WrappableList<IDetailLoadInfo, DetailLoadInfo> items = new WrappableList<IDetailLoadInfo, DetailLoadInfo>();
      items.Add(new DetailLoadInfo());
      DetailLoadInfo toRemove = new DetailLoadInfo();
      items.Add(toRemove);
      IList<IDetailLoadInfo> sut = (IList<IDetailLoadInfo>)new ObservableCollection<IDetailLoadInfo, DetailLoadInfo, ObservableDetailLoadInfo>(items, x => new ObservableDetailLoadInfo(x));

      sut[0].Should().BeOfType<ObservableDetailLoadInfo>();
      sut.Remove(sut[0]);

      sut.Count.Should().Be(1);
    }

    [Fact]
    public void GivenWrappableListWhenGetByIndexFromWrapperListThenReturnWrapper()
    {
      WrappableList<IDetailLoadInfo, DetailLoadInfo> items = new WrappableList<IDetailLoadInfo, DetailLoadInfo>();
      items.Add(new DetailLoadInfo());
      DetailLoadInfo toRemove = new DetailLoadInfo();
      items.Add(toRemove);
      IList<IDetailLoadInfo> sut = (IList<IDetailLoadInfo>)new ObservableCollection<IDetailLoadInfo, DetailLoadInfo, ObservableDetailLoadInfo>(items, x => new ObservableDetailLoadInfo(x));

      sut[0].Should().BeOfType<ObservableDetailLoadInfo>();
    }

    [Fact]
    public void GivenWrappableListWhenRemoveNonConcreteThenReturnNotFound()
    {
      WrappableList<IDetailLoadInfo, DetailLoadInfo> items = new WrappableList<IDetailLoadInfo, DetailLoadInfo>();
      items.Add(new DetailLoadInfo());
      DetailLoadInfo toRemove = new DetailLoadInfo();
      items.Add(toRemove);
      IList<IDetailLoadInfo> sut = (IList<IDetailLoadInfo>)new ObservableCollection<IDetailLoadInfo, DetailLoadInfo, ObservableDetailLoadInfo>(items, x => new ObservableDetailLoadInfo(x));

      sut.Remove(A.Fake<IDetailLoadInfo>()).Should().BeFalse();
    }
  }
}
