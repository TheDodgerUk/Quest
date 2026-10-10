using System;
using System.Reflection;

namespace FullSerializer
{
    public class fsPrePostProcessor : fsObjectProcessor
    {
        public override bool CanProcess(Type type)
        {
            return true;
        }

        public override void OnBeforeSerialize(Type storageType, object instance)
        {
            InvokeMethodsWithAttribute(storageType, instance, typeof(PreSerialiseAttribute));
        }

        public override void OnAfterDeserialize(Type storageType, object instance)
        {
            InvokeMethodsWithAttribute(storageType, instance, typeof(PostDeserialiseAttribute));
        }

        private static void InvokeMethodsWithAttribute(Type type, object instance, Type attribute)
        {
            var flags = BindingFlags.NonPublic | BindingFlags.Instance;
            var methods = type.GetMethods(flags);
            for (int i = 0; i < methods.Length; ++i)
            {
                if (methods[i].GetParameters().Length == 0)
                {
                    if (methods[i].IsDefined(attribute, true) == true)
                    {
                        methods[i].Invoke(instance, null);
                    }
                }
            }
        }
    }

    public class PostDeserialiseAttribute : Attribute { }
    public class PreSerialiseAttribute : Attribute { }
}
