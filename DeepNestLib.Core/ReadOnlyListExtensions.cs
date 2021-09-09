namespace DeepNestLib
{
  using System.Collections.Generic;

  public static class ReadOnlyListExtensions
  {
    /// <summary>
    /// Searches for the specified object and returns the zero-based index of the first occurrence within the entire <see cref="System.Collections.Generic.List{T}"/>.
    /// </summary>
    /// <typeparam name="T">Generic type parameter.</typeparam>
    /// <param name="list">The <see cref="System.Collections.Generic.List{T}"/> to search.</param>
    /// <param name="item">The object to locate in the <see cref="System.Collections.Generic.List{T}"/>. The value can be null for reference types.</param>
    /// <returns>The zero-based index of the first occurrence of item within the entire <see cref="System.Collections.Generic.List{T}"/>, if found; otherwise, -1.</returns>
    public static int IndexOf<T>(this IReadOnlyList<T> list, T item)
    {
      for (int i = 0; i < list.Count; i++)
      {
        if (list[i].Equals(item))
        {
          return i;
        }
      }

      return -1;
    }
  }
}
