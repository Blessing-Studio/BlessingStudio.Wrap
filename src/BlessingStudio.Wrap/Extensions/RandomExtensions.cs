using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace BlessingStudio.Wrap.Extensions
{
    public static class RandomExtensions
    {
        public static T Choose<T>(this IList<T> list)
        {
            int len = list.Count;
            int i = new Random().Next(0, len);
            return list[i];
        }
    }
}
