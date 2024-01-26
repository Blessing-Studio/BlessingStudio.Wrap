namespace BlessingStudio.Wrap.Protocol.Attributes;

[AttributeUsage(AttributeTargets.Field)]
public sealed class FieldAttribute : Attribute {
    public int index;

    public ValueType valueType;

    public FieldAttribute(int index, ValueType valueType) {
        this.index = index;
        this.valueType = valueType;
    }
}
