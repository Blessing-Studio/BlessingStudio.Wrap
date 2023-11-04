using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace BlessingStudio.Wrap.Protocol.Attributes
{
    [AttributeUsage(AttributeTargets.Field)]
    public class FieldAttribute : Attribute
    {
        public int index;
        public ValueType valueType;
        public FieldAttribute(int index, ValueType valueType)
        {
            this.index = index;
            this.valueType = valueType;
        }
    }
}
