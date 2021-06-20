//namespace SvgDxfConverter.CiTests
//{
//    using System;
//    using System.Collections.Generic;
//    using global::DeepNestLib;
//    using FluentAssertions;
//    using Xunit;
//    using Svg;

//    public class SvgNet
//    {
//        [Fact]
//        public void GivenSvgFileWhenImportedShouldNotBeNull()
//        {
//            var uri = @"D:\kabeja-0.4\ExportedDirect.svg";
//            var svgDoc = SvgDocument.Open<SvgDocument>(uri, null);
//            svgDoc.Should().NotBeNull();
//            svgDoc.Content.
//        }

//        [Fact]
//        public void GivenKiadedSvgFileWhenPutInNestedContextShouldNotBeNull()
//        {
//            var uri = @"D:\kabeja-0.4\ExportedDirect.svg";
//            var svgDoc = SvgDocument.Open<SvgDocument>(uri, null);
            


//        }

//        private SvgDocument svg;
//        private SvgElement svgRoot;

//        private SvgElement Load(SvgDocument svg, int? scale, float scalingFactor)
//        {
//            var root = svg.Children[0]; //   firstElementChild;

//            this.svg = svg;
//            this.svgRoot = root;

//            // get local scaling factor from svg root "width" dimension
//            string widthString;
//            float width;
//            var hasWidth = root.TryGetAttribute("width", out widthString);
//            if (hasWidth)
//            {
//                width = float.Parse(widthString);
//            }
//            string viewBox;
//            var hasViewBox = root.TryGetAttribute("viewBox", out viewBox);
//            var hasTransforms = root.Transforms.Count > 0;
            
//            if (!hasWidth || !hasViewBox)
//            {
//               root.Transforms.Add(new Svg.Transforms.SvgScale(scalingFactor));
//            }

//            viewBox = viewBox.trim().split(/[\s,] +/);

//            if (!width || viewBox.length < 4)
//            {
//                return this.svgRoot;
//            }

//            var pxwidth = viewBox[2];

//            // localscale is in pixels/inch, regardless of units
//            var localscale = null;

//            if (/in/.test(width)){
//                width = Number(width.replace(/[^ 0 - 9\.] / g, ""));
//                localscale = pxwidth / width;
//            }
//			else if (/ mm /.test(width))
//            {
//                width = Number(width.replace(/[^ 0 - 9\.] / g, ""));
//                localscale = (25.4 * pxwidth) / width;
//            }
//            else if (/ cm /.test(width))
//            {
//                width = Number(width.replace(/[^ 0 - 9\.] / g, ""));
//                localscale = (2.54 * pxwidth) / width;
//            }
//            else if (/ pt /.test(width))
//            {
//                width = Number(width.replace(/[^ 0 - 9\.] / g, ""));
//                localscale = (72 * pxwidth) / width;
//            }
//            else if (/ pc /.test(width))
//            {
//                width = Number(width.replace(/[^ 0 - 9\.] / g, ""));
//                localscale = (6 * pxwidth) / width;
//            }
//            // these are css "pixels"
//            else if (/ px /.test(width))
//            {
//                width = Number(width.replace(/[^ 0 - 9\.] / g, ""));
//                localscale = (96 * pxwidth) / width;
//            }

//            if (localscale === null)
//            {
//                localscale = scalingFactor;
//            }
//            else if (scalingFactor)
//            {
//                localscale *= scalingFactor;
//            }

//            // no scaling factor
//            if (localscale === null)
//            {
//                console.log("no scale");
//                return this.svgRoot;
//            }

//            transform = root.getAttribute("transform") || "";

//            transform += " scale(" + (scale / localscale) + ")";

//            root.setAttribute("transform", transform);

//            this.conf.scale *= scale / localscale;
//        }

//        [Fact]
//        public void GiovenSvgFileWhenLoadedToANestedContextShouldNotBeNull()
//        {
//            var nestingContext = new NestingContext();
//            nestingContext.ImportFromRawDetail(SvgParser.LoadSvg(@"SimpleNestExport.svg"), 0).Should().NotBeNull();
//        }
//    }
//}
