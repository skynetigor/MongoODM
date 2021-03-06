﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;

namespace DbdocFramework.Extensions
{
    internal static class  ObjectExtensions
    {
        public static IEnumerable<PropertyInfo> GetProperties(this object obj)
        {
            return obj.GetType().GetProperties();
        }

        public static PropertyInfo GetProperty(this object obj, string propertyName)
        {
            return obj.GetType().GetProperty(propertyName);
        }

        public static PropertyInfo GetPropertyIgnoreCase(this object obj, string propertyName)
        {
            return obj.GetType().GetProperties().FirstOrDefault(p => p.Name.Equals(propertyName, StringComparison.CurrentCultureIgnoreCase));
        }

        public static PropertyInfo GetPrivateProperty(this object obj, string propertyName)
        {
            return obj.GetType().GetProperty(propertyName, BindingFlags.NonPublic | BindingFlags.Instance);
        }

        public static FieldInfo GetPrivateField(this object obj, string propertyName)
        {
            return obj.GetType().GetField(propertyName, BindingFlags.NonPublic | BindingFlags.Instance);
        }

        public static IEnumerable<MethodInfo> GetMethods(this object obj)
        {
            return obj.GetType().GetMethods();
        }

        public static MethodInfo GetMethod(this object obj, string methodName)
        {
            return obj.GetType().GetMethod(methodName);
        }

        public static MethodInfo GetMethod(this object obj, string methodName, BindingFlags bindingFlags)
        {
            return obj.GetType().GetMethod(methodName, bindingFlags);
        }

        public static MethodInfo GetPrivateMethod(this object obj, string methodName)
        {
            return obj.GetType().GetMethod(methodName, BindingFlags.NonPublic | BindingFlags.Instance);
        }
    }
}             
