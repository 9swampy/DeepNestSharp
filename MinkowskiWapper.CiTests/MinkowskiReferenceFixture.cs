namespace MinkowskiWapper.CiTests
{
    using FluentAssertions;
    using System;
    using System.IO;
    using System.Linq;
    using Xunit;

    public class MinkowskiReferenceFixture
    {
#if x86
        [Fact]
        public void PreX86DllShouldExist()
        {
            new FileInfo("minkowski_x86.dll").Exists.Should().BeTrue();
        }
#elif x64
        [Fact]
        public void PreX64DllShouldExist()
        {
            new FileInfo("minkowski_x64.dll").Exists.Should().BeTrue();
        }
#else
        [Fact]
        public void PreAnyCpuDllShouldExist()
        {
            new FileInfo("minkowski.dll").Exists.Should().BeTrue();
        }
#endif

        [Fact]
#if x86
        public void GivenMinkowskiDependencyWhenX86ThenCallExternalDllWithoutThrowing() 
#elif x64
        public void GivenMinkowskiDependencyWhenX64ThenCallExternalDllWithoutThrowing()
#else
       public void GivenMinkowskiDependencyWhenAnyCPUThenCallExternalDllWithoutThrowing() 
#endif
        {
#if x64
            Action act = () => Minkowski.MinkowskiWrapper.setData(
                8,
                new[] { -155.50000000000023, -80.500000000000114, 3155.5, -80.500000000000114, 3155.5, 1580.5, -155.50000000000023, 1580.5 },
                1,
                new[] { 8L },
                new[] { 3005D, 1505D, -5D, 1505D, -5D, -5D, 3005D, -5D },
                8,
                new[] { 16D, 16D, -5D, 16D, -5D, -5D, 16D, -5D });
#else
            Action act = () => Minkowski.MinkowskiWrapper.setData(
                8, 
                new[] { -155.50000000000023, -80.500000000000114, 3155.5, -80.500000000000114, 3155.5, 1580.5, -155.50000000000023, 1580.5 }, 
                1,
                new[] { 8 },
                new[] { 3005D, 1505D, -5D, 1505D, -5D, -5D, 3005D, -5D }, 
                8, 
                new[] { 16D, 16D, -5D, 16D, -5D, -5D, 16D, -5D });
#endif


            act.Should().NotThrow();
        }

        [Fact]
#if x86
        public void GivenMinkowskiDependencyWhenX86ThenCallExternalDll()
#elif x64
        public void GivenMinkowskiDependencyWhenX64ThenCallExternalDll()
#else
       public void GivenMinkowskiDependencyWhenAnyCPUThenCallExternalDll()
#endif
        {
#if x64
            long[] longs = (new[] { 1 }).Select(o => (long)o).ToArray();
            Minkowski.MinkowskiWrapper.setData(1, new[] { 1d }, 1, longs, new[] { 1d }, 1, new[] { 1d });
#else
            Minkowski.MinkowskiWrapper.setData(1, new[] { 1d }, 1, new[] { 1 }, new[] { 1d }, 1, new[] { 1d });
#endif
        }

        [Fact]
        public void DebugPreprocessorDirective()
        {
#if x86
            System.Diagnostics.Debug.Print("Hello x86");
#elif x64
            System.Diagnostics.Debug.Print("Hello x64");
#else
            System.Diagnostics.Debug.Print("Hello AnyCPU");
#endif
        }
    }
}
