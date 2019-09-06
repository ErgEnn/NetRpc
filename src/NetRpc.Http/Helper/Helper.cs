﻿using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Http;
using NetRpc.Http.Client;
using Newtonsoft.Json;

namespace NetRpc.Http
{
    internal static class Helper
    {
        public static object ToObjectForHttp(string str, Type t)
        {
            object dataObj;
            try
            {
                dataObj = str.ToObject(t);
            }
            catch (Exception e)
            {
                throw new HttpFailedException($"{e.Message}, str:\r\n{str}");
            }

            return dataObj;
        }

        public static string FormatPath(string path)
        {
            path = path.Replace('\\', '/');
            path = path.Replace("//", "/");
            path = path.TrimStart('/');
            return path;
        }

        public static object[] GetArgsFromDataObj(Type dataObjType, object dataObj)
        {
            List<object> ret = new List<object>();
            if (dataObjType == null)
                return ret.ToArray();
            foreach (var p in dataObjType.GetProperties())
                ret.Add(ClassHelper.GetPropertyValue(dataObj, p.Name));
            return ret.ToArray();
        }

        public static object[] GetArgsFromQuery(IQueryCollection query, Type dataObjType)
        {
            if (dataObjType == null)
                return new object[0];

            var ret = new List<object>();
            foreach (var p in dataObjType.GetProperties())
            {
                if (query.TryGetValue(p.Name, out var values))
                {
                    try
                    {
                        if (p.PropertyType == typeof(string))
                        {
                            ret.Add(values[0]);
                            continue;
                        }

                        var v = p.PropertyType.GetMethod("Parse", new[] { typeof(string) }).Invoke(null, new object[] { values[0] });
                        ret.Add(v);
                    }
                    catch (Exception ex)
                    {
                        throw new HttpNotMatchedException($"'{p.Name}' is not valid value, {ex.Message}");
                    }
                }
                else
                {
                    ret.Add(ClassHelper.GetDefault(p.PropertyType));
                }
            }
            return ret.ToArray();
        }

        public static bool IsEqualsOrSubclassOf(this Type t, Type c)
        {
            return t == c || t.IsSubclassOf(c);
        }

        public static List<string> GetCommentsXmlPaths()
        {
            var ret = new List<string>();
            var root = GetAssemblyPath();
            foreach (var file in Directory.GetFiles(root))
            {
                if (string.Equals(Path.GetExtension(file), ".xml", StringComparison.OrdinalIgnoreCase))
                {
                    var dll = GetFullPathWithoutExtension(file) + ".dll";
                    var exe = GetFullPathWithoutExtension(file) + ".exe";
                    if (File.Exists(dll) || File.Exists(exe))
                        ret.Add(file);
                }
            }

            return ret;
        }

        public static string GetFullPathWithoutExtension(string s)
        {
            var dir = Path.GetDirectoryName(s);
            if (dir == null)
                dir = "";
            var name = Path.GetFileNameWithoutExtension(s);
            if (name == null)
                name = "";
            return Path.Combine(dir, name);
        }

        public static string GetAssemblyPath()
        {
            var assembly = Assembly.GetEntryAssembly();
            if (assembly == null)
                assembly = Assembly.GetExecutingAssembly();

            var path = assembly.CodeBase;
            path = path.Substring(8, path.Length - 8);
            path = Path.GetDirectoryName(path);
            return path;
        }

        public static string GetExceptionContent(this Exception ex)
        {
            return $"{ex.GetType()}, {ex.Message}";
        }
    }
}