namespace DeepNestLib.CiTests
{
    using System;
    using System.Collections.Generic;
    using System.IO;
    using System.Linq;
    using FakeItEasy;
    using FluentAssertions;
    using IxMilia.Dxf.Entities;
    using Xunit;

    public class DxfGenerator
    {
        public DxfPolyline Rectangle(double side)
        {
            return Rectangle(side, side);
        }

        public DxfPolyline Rectangle(double sideA, double sideB)
        {
            return new DxfPolyline(new DxfVertex[]
            {
                                                        new DxfVertex(new IxMilia.Dxf.DxfPoint(0D, sideB, 0D)),
                                                        new DxfVertex(new IxMilia.Dxf.DxfPoint(sideA, sideB, 0D)),
                                                        new DxfVertex(new IxMilia.Dxf.DxfPoint(sideA, 0D, 0D)),
                                                        new DxfVertex(new IxMilia.Dxf.DxfPoint(0D, 0D, 0D)),
                                                        new DxfVertex(new IxMilia.Dxf.DxfPoint(0D, sideB, 0D)),
            });
        }
    }
}
