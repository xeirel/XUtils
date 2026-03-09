using System;

namespace XUtils.Console
{
    [AttributeUsage(AttributeTargets.Method, AllowMultiple = false, Inherited = true)]
    public sealed class DevCommandAttribute : Attribute
    {
        public string Name { get; }
        public string Description { get; set; }

        public DevCommandAttribute()
        {
        }

        public DevCommandAttribute(string name)
        {
            Name = name;
        }
    }
}
