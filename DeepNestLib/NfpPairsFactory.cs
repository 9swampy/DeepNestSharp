﻿namespace DeepNestLib
{
  using System;
  using System.Collections.Generic;
  using System.Diagnostics;
  using System.Drawing;
  using System.Linq;
  using System.Runtime.InteropServices;
  using System.Threading.Tasks;
  using ClipperLib;
  using Minkowski;

  public class NfpPairsFactory
  {
    private static volatile object preProcessSyncLock = new object();

    private readonly IWindowUnk window;

    public NfpPairsFactory(IWindowUnk window)
    {
      this.window = window;
    }

    public List<NfpPair> Generate(bool useParallel, List<NFP> parts)
    {
      List<NfpPair> pairs = new List<NfpPair>();
      if (useParallel)
      {
        Parallel.For(0, parts.Count, i =>
        {
          {
            foreach (var pair in CreatePair(i, parts))
            {
              var doc = new DbCacheKey(pair.Asource, pair.Bsource, pair.ARotation, pair.BRotation);
              AddToPairs(pairs, pair, doc);
            }
          }
        });
      }
      else
      {
        for (var i = 0; i < parts.Count; i++)
        {
          foreach (var pair in CreatePair(i, parts))
          {
            var doc = new DbCacheKey(pair.Asource, pair.Bsource, pair.ARotation, pair.BRotation);
            AddToPairs(pairs, pair, doc);
          }
        }
      }

      return pairs;
    }

    private IEnumerable<NfpPair> CreatePair(int i, List<NFP> parts)
    {
      var b = parts[i];
      for (var j = 0; j < i; j++)
      {
        var a = parts[j];
        yield return new NfpPair()
        {
          A = a,
          B = b,
          ARotation = a.Rotation,
          BRotation = b.Rotation,
          Asource = a.Source,
          Bsource = b.Source,
        };
      }
    }

    private void AddToPairs(List<NfpPair> pairs, NfpPair key, DbCacheKey doc)
    {
      lock (preProcessSyncLock)
      {
        if (!this.InPairs(key, pairs.ToArray()) && !window.Has(doc))
        {
          pairs.Add(key);
        }
      }
    }

    private bool InPairs(NfpPair key, NfpPair[] p)
    {
      for (var i = 0; i < p.Length; i++)
      {
        if (p[i].Asource == key.Asource && p[i].Bsource == key.Bsource && p[i].ARotation == key.ARotation && p[i].BRotation == key.BRotation)
        {
          return true;
        }
      }

      return false;
    }
  }
}