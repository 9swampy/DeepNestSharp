namespace DeepNestLib
{
    using System.Collections.Generic;

    public interface INfp
    {
        float Rotation { get; }

        int Source { get; }

        SvgPoint[] Points { get; }

        IList<NFP> Children { get; }
    }
}