#if NET35
namespace System.Reflection
{
    [AttributeUsage(AttributeTargets.Assembly)]
    internal sealed class AssemblyMetadataAttribute : Attribute
    {
        public AssemblyMetadataAttribute(string key, string value)
        {
        }
    }
}
#endif