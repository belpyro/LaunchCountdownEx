using UnityEngine;

namespace NASA_CountDown.Extensions
{
    public static class PluginExtensions
    {
        public static GameObject TryToAddComponent<T>(this GameObject obj) where T : Component
        {
            if (!obj.TryGetComponent(typeof(T), out var _))
            {
                obj.AddComponent<T>();
            }

            return obj;
        }       
        
        public static GameObject TryToRemoveComponent<T>(this GameObject obj) where T : Component
        {
            if (obj.TryGetComponent(typeof(T), out var component))
            {
                Object.Destroy(component);
            }

            return obj;
        }

        public static ConfigNode TryAddValue<T>(this ConfigNode node, string key, T value) where T: class
        {
            if (value == null) return node;
            
            node.AddValue(key, value);
            return node;
        }
    }
}