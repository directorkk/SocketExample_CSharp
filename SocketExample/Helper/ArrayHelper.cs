using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace LouieTool
{
    public static class ArrayHelper
    {
        public static IEnumerable<List<T>> SplitList<T>(List<T> BaseList, int Size = 30)
        {
            for (int i = 0; i < BaseList.Count; i += Size)
            {
                yield return BaseList.GetRange(i, Math.Min(Size, BaseList.Count - i));
            }
        }
    }
}