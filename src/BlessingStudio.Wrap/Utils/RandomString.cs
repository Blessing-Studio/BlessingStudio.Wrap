using BlessingStudio.Wrap.Extensions;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace BlessingStudio.Wrap.Utils;

public class RandomString
{
    public Random Random { get; set; } = Random.Shared;
    public bool GenNumber {  get; set; }
    public bool GenChar { get; set; } = true;
    public List<int> NumChars { get; set; } = new()
    {
        1, 2, 3, 4, 5, 6, 7, 8, 9
    };
    public List<char> Chars { get; set; } = "abcdefg".ToCharArray().ToList();
    public RandomString(int seed) {  Random = new Random(seed); }
    public RandomString() { }
    public string Next(int minLen, int maxLen)
    {
        if(!GenNumber && !GenChar)
        {
            throw new ArgumentException("Arguments can not be null.");
        }

        StringBuilder builder = new();
        List<char> chars = new();
        if(GenChar) chars.AddRange(Chars);
        if(GenNumber) chars.AddRange(NumChars.Select(x => x.ToString()[0]));

        int len = Random.Next(minLen, maxLen);
        for(int i = 0; i < len; i++)
        {
            builder.Append(chars.Choose());
        }

        return builder.ToString();
    }
    public string Next(int len) { return Next(len, len); }
}
