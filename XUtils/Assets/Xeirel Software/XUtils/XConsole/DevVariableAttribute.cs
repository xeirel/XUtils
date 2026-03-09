using System;

namespace XUtils.Console
{
    [AttributeUsage(AttributeTargets.Field | AttributeTargets.Property, AllowMultiple = false, Inherited = true)]
    public sealed class DevVariableAttribute : Attribute
    {
        public string Name { get; }
        public string Description { get; set; }

        public DevVariableAttribute()
        {
        }

        public DevVariableAttribute(string name)
        {
            Name = name;
        }
    }
}
