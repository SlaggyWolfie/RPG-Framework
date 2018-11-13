﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using UnityEditor;
using UnityEngine;

namespace RPG.Nodes
{
    public class NodeReflection
    {
        public static bool IsOfType(Type derivedType, Type baseType)
        {
            return derivedType.IsSubclassOf(baseType) || derivedType.IsAssignableFrom(baseType);
        }

        public static bool IsOfType(object obj, Type baseType)
        {
            return IsOfType(obj.GetType(), baseType);
        }

        public static bool IsOfType<TDerived, TBase>()
        {
            return IsOfType(typeof(TDerived), typeof(TBase));
        }

        public static bool IsOfType<TBase>(object obj)
        {
            return obj is TBase;
        }

        public static Type[] GetDerivedTypes<T>()
        {
            return GetDerivedTypes(typeof(T));
        }

        public static Type[] GetDerivedTypes(Type baseType)
        {
            List<Type> types = new List<Type>();
            Assembly[] assemblies = AppDomain.CurrentDomain.GetAssemblies();

            foreach (Assembly assembly in assemblies)
            {
                types.AddRange(assembly.GetTypes().
                    Where(t => !t.IsAbstract && baseType.IsAssignableFrom(t)).ToArray());
            }

            return types.ToArray();
        }

        public static Type[] GetNodeTypes()
        {
            return GetDerivedTypes<Node>();
        }

        public static KeyValuePair<TAttribute, MethodInfo>[] GetAttributeMethods<TAttribute>(object o) where TAttribute : Attribute
        {
            Type type = o.GetType();
            MethodInfo[] methods = type.GetMethods(BindingFlags.Instance | BindingFlags.Static | BindingFlags.Public | BindingFlags.NonPublic);
            List<KeyValuePair<TAttribute, MethodInfo>> kvp = new List<KeyValuePair<TAttribute, MethodInfo>>();
            foreach (MethodInfo method in methods)
            {
                TAttribute[] attributes = (TAttribute[])method.GetCustomAttributes(typeof(TAttribute), true);
                if (attributes.Length == 0) continue;

                if (method.GetParameters().Length != 0)
                {
                    Debug.LogWarning("Method " + method.DeclaringType.Name + "." + method.Name +
                                     " has parameters and cannot be used for commands.");
                    continue;
                }
                if (method.IsStatic)
                {
                    Debug.LogWarning("Method " + method.DeclaringType.Name + "." + method.Name +
                                     " is static and cannot be used for commands.");
                    continue;
                }

                kvp.AddRange(attributes.Select(
                    attribute => new KeyValuePair<TAttribute, MethodInfo>(attribute, method)));
            }

            return kvp.ToArray();
        }
    }
}
