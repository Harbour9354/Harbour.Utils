﻿using System;
using System.Collections.Generic;
using System.Data;
using System.Linq.Expressions;
using System.Reflection;

namespace Harbour.Utils
{
    /// <summary>
    /// 实体转换类（默认有缓存）
    /// </summary>
    public class EntityConverter
    {
        /// <summary>
        /// 将DataRow转为实体
        /// </summary>
        /// <typeparam name="T">实体类（必须有默认构造参数）</typeparam>
        /// <param name="dr">DataRow</param>
        /// <returns></returns>
        public static T ToEntity<T>(DataRow dr) where T : new()
        {
            if (dr == null)
                return default(T);

            T t = new T();
            foreach (PropertyInfo prop in typeof(T).GetProperties())
            {
                if (dr.Table.Columns.Contains(prop.Name))
                {
                    if (dr[prop.Name] != DBNull.Value)
                        GetSetter<T>(prop)(t, dr[prop.Name]);
                }
            }
            return t;
        }
        /// <summary>
        /// 将IDataReader转换为实体
        /// </summary>
        /// <typeparam name="T">实体类（必须有默认构造参数）</typeparam>
        /// <param name="dr">IDataReader</param>
        /// <returns></returns>
        public static T ToEntity<T>(IDataReader dr) where T : new()
        {
            T t = default(T);
            if (dr.Read())
            {
                t = new T();
                foreach (PropertyInfo prop in typeof(T).GetProperties())
                {
                    if (dr[prop.Name] != DBNull.Value)
                        GetSetter<T>(prop)(t, dr[prop.Name]);
                }
            }
            return t;
        }

        /// <summary>
        /// 将DataTable转为List
        /// </summary>
        /// <typeparam name="T">实体类（必须有默认构造参数）</typeparam>
        /// <param name="dt">DataTable</param>
        /// <returns></returns>
        public static List<T> ToList<T>(DataTable dt) where T : new()
        {
            List<T> list = new List<T>();
            if (dt == null || dt.Rows.Count == 0)
            {
                return list;
            }

            foreach (DataRow dr in dt.Rows)
            {
                T t = new T();
                foreach (PropertyInfo prop in typeof(T).GetProperties())
                {
                    if (dr.Table.Columns.Contains(prop.Name))
                    {
                        if (dr[prop.Name] != DBNull.Value)
                            GetSetter<T>(prop)(t, dr[prop.Name]);
                    }
                }
                list.Add(t);
            }

            return list;
        }
        /// <summary>
        /// 将IDataReader转为实体
        /// </summary>
        /// <typeparam name="T">实体类（必须有默认构造参数）</typeparam>
        /// <param name="dr">IDataReader</param>
        /// <returns></returns>
        public static List<T> ToList<T>(IDataReader dr) where T : new()
        {
            List<T> list = new List<T>();
            while (dr.Read())
            {
                T t = new T();
                foreach (PropertyInfo prop in typeof(T).GetProperties())
                {
                    if (dr[prop.Name] != DBNull.Value)
                        GetSetter<T>(prop)(t, dr[prop.Name]);
                }
                list.Add(t);
            }
            return list;
        }
        private static Action<T, object> GetSetter<T>(PropertyInfo property)
        {
            Action<T, object> result = null;
            Type type = typeof(T);
            string key = type.AssemblyQualifiedName + "_set_" + property.Name;

            //创建 对实体 属性赋值的expression
            ParameterExpression parameter = Expression.Parameter(type, "t");
            ParameterExpression value = Expression.Parameter(typeof(object), "propertyValue");
            MethodInfo setter = type.GetMethod("set_" + property.Name);
            MethodCallExpression call = Expression.Call(parameter, setter, Expression.Convert(value, property.PropertyType));
            var lambda = Expression.Lambda<Action<T, object>>(call, parameter, value);
            result = lambda.Compile();
            return result;
        }

        //private static Action<T, object> GetSetter<T>(PropertyInfo property)
        //{
        //    Action<T, object> result = null;
        //    Type type = typeof(T);
        //    string key = type.AssemblyQualifiedName + "_set_" + property.Name;
        //    if (CacheHelper.GetCache(key) == null)
        //    {
        //        //创建 对实体 属性赋值的expression
        //        ParameterExpression parameter = Expression.Parameter(type, "t");
        //        ParameterExpression value = Expression.Parameter(typeof(object), "propertyValue");
        //        MethodInfo setter = type.GetMethod("set_" + property.Name);
        //        MethodCallExpression call = Expression.Call(parameter, setter, Expression.Convert(value, property.PropertyType));
        //        var lambda = Expression.Lambda<Action<T, object>>(call, parameter, value);
        //        result = lambda.Compile();
        //        CacheHelper.SetCache(key, result, TimeSpan.FromMinutes(60));
        //    }
        //    else
        //    {
        //        result = CacheHelper.GetCache<Action<T, object>>(key);
        //    }
        //    return result;
        //}
        /*
        private static Action<T, object> GetSetter<T>(PropertyInfo property)
        {
            Action<T, object> result = null;
            Type type = typeof(T);
            string key = type.AssemblyQualifiedName + "_set_" + property.Name;
            if (HttpRuntime.Cache.Get(key) == null)
            {
                创建 对实体 属性赋值的expression
                ParameterExpression parameter = Expression.Parameter(type, "t");
                ParameterExpression value = Expression.Parameter(typeof(object), "propertyValue");
                MethodInfo setter = type.GetMethod("set_" + property.Name);
                MethodCallExpression call = Expression.Call(parameter, setter, Expression.Convert(value, property.PropertyType));
                var lambda = Expression.Lambda<Action<T, object>>(call, parameter, value);
                result = lambda.Compile();
                HttpRuntime.Cache[key] = result;
            }
            else
            {
                result = HttpRuntime.Cache[key] as Action<T, object>;
            }
            return result;
        }
        */
    }
}
