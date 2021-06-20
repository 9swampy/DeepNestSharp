﻿namespace DeepNestLib
{
    public class SvgNestConfig
    {
        public PlacementTypeEnum placementType = PlacementTypeEnum.Box;
        public double curveTolerance = 0.72;
        public double scale = 25;
        public double clipperScale = 10000000;
        public bool exploreConcave = false;
        public int mutationRate = 10;
        public int populationSize = 10;
        public int rotations = 4;
        public double spacing = 10;
        public double sheetSpacing = 0;
        public bool useHoles = false;
        public double timeRatio = 0.5;
        public bool mergeLines = false;
        public bool simplify;
    }
}
