namespace DeepNestLib.CiTests.DummyFactories
{
  using DeepNestLib.IO;
  using FakeItEasy;
  using System;

  internal class DummyRelativePathHelper : DummyFactory<IRelativePathHelper>
  {
    protected override IRelativePathHelper Create()
    {
      var result = new RelativePathHelper(AppDomain.CurrentDomain.BaseDirectory);
      //var result = A.Fake<IRelativePathHelper>();
      //A.CallTo(() => result.ConvertToRelativePath(A<string>._)).ReturnsLazily(o => (string)o.Arguments[0]);
      //A.CallTo(() => result.GetSolutionDirectory()).Returns("C:\\Temp");
      return result;
    }
  }
}
