using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations.Schema;
using System.Data;
using System.Reflection;
using System.Reflection.Emit;
using Kinetix.ComponentModel;

namespace Kinetix.DataAccess.Sql
{
    /// <summary>
    /// Factory d'adapter.
    /// </summary>
    internal class DataRecordAdapterFactory
    {
        private const string _assemblyName = "Kinetix.Broker.DataRecordAdapters";

        private static readonly Type[] _abstractReadMethodParams = new Type[] { typeof(IDataRecord), typeof(int) };
        private static readonly Type[] _constructorParams = new Type[0];
        private static readonly Type[] _readMethodParams = new Type[] { typeof(object), typeof(IDataRecord) };

        private readonly AssemblyBuilder _assemblyBuilder;
        private readonly ModuleBuilder _dynamicModule;
        private readonly MethodInfo _readBooleanMethodInfo;
        private readonly MethodInfo _readByteArrayMethodInfo;
        private readonly MethodInfo _readByteMethodInfo;
        private readonly MethodInfo _readCharArrayMethodInfo;
        private readonly MethodInfo _readCharMethodInfo;
        private readonly MethodInfo _readDateTimeMethodInfo;
        private readonly MethodInfo _readDecimalMethodInfo;
        private readonly MethodInfo _readDoubleMethodInfo;
        private readonly MethodInfo _readFloatMethodInfo;
        private readonly MethodInfo _readGuidMethodInfo;
        private readonly MethodInfo _readIntMethodInfo;
        private readonly MethodInfo _readLongMethodInfo;
        private readonly MethodInfo _readNonNullableBooleanMethodIndo;
        private readonly MethodInfo _readShortMethodInfo;
        private readonly MethodInfo _readStringMethodInfo;
        private readonly MethodInfo _readTimeSpanMethodInfo;

        private int _adapterNum = 0;

        /// <summary>
        /// Crée une nouvelle instance.
        /// </summary>
        private DataRecordAdapterFactory()
        {
            // Création du module.
            var name = new AssemblyName { Name = _assemblyName };
            _assemblyBuilder = AssemblyBuilder.DefineDynamicAssembly(name, AssemblyBuilderAccess.Run);
            _dynamicModule = _assemblyBuilder.DefineDynamicModule(_assemblyName);
            _readBooleanMethodInfo = typeof(AbstractDataReaderAdapter).GetMethod("ReadBoolean", BindingFlags.Static | BindingFlags.NonPublic, null, _abstractReadMethodParams, null);
            _readNonNullableBooleanMethodIndo = typeof(AbstractDataReaderAdapter).GetMethod("ReadNonNullableBoolean", BindingFlags.Static | BindingFlags.NonPublic, null, _abstractReadMethodParams, null);
            _readByteMethodInfo = typeof(AbstractDataReaderAdapter).GetMethod("ReadByte", BindingFlags.Static | BindingFlags.NonPublic, null, _abstractReadMethodParams, null);
            _readDateTimeMethodInfo = typeof(AbstractDataReaderAdapter).GetMethod("ReadDateTime", BindingFlags.Static | BindingFlags.NonPublic, null, _abstractReadMethodParams, null);
            _readTimeSpanMethodInfo = typeof(AbstractDataReaderAdapter).GetMethod("ReadTimeSpan", BindingFlags.Static | BindingFlags.NonPublic, null, _abstractReadMethodParams, null);
            _readDecimalMethodInfo = typeof(AbstractDataReaderAdapter).GetMethod("ReadDecimal", BindingFlags.Static | BindingFlags.NonPublic, null, _abstractReadMethodParams, null);
            _readIntMethodInfo = typeof(AbstractDataReaderAdapter).GetMethod("ReadInt", BindingFlags.Static | BindingFlags.NonPublic, null, _abstractReadMethodParams, null);
            _readShortMethodInfo = typeof(AbstractDataReaderAdapter).GetMethod("ReadShort", BindingFlags.Static | BindingFlags.NonPublic, null, _abstractReadMethodParams, null);
            _readLongMethodInfo = typeof(AbstractDataReaderAdapter).GetMethod("ReadLong", BindingFlags.Static | BindingFlags.NonPublic, null, _abstractReadMethodParams, null);
            _readFloatMethodInfo = typeof(AbstractDataReaderAdapter).GetMethod("ReadFloat", BindingFlags.Static | BindingFlags.NonPublic, null, _abstractReadMethodParams, null);
            _readDoubleMethodInfo = typeof(AbstractDataReaderAdapter).GetMethod("ReadDouble", BindingFlags.Static | BindingFlags.NonPublic, null, _abstractReadMethodParams, null);
            _readStringMethodInfo = typeof(AbstractDataReaderAdapter).GetMethod("ReadString", BindingFlags.Static | BindingFlags.NonPublic, null, _abstractReadMethodParams, null);
            _readCharMethodInfo = typeof(AbstractDataReaderAdapter).GetMethod("ReadChar", BindingFlags.Static | BindingFlags.NonPublic, null, _abstractReadMethodParams, null);
            _readCharArrayMethodInfo = typeof(AbstractDataReaderAdapter).GetMethod("ReadCharArray", BindingFlags.Static | BindingFlags.NonPublic, null, _abstractReadMethodParams, null);
            _readGuidMethodInfo = typeof(AbstractDataReaderAdapter).GetMethod("ReadGuid", BindingFlags.Static | BindingFlags.NonPublic, null, _abstractReadMethodParams, null);
            _readByteArrayMethodInfo = typeof(AbstractDataReaderAdapter).GetMethod("ReadByteArray", BindingFlags.Static | BindingFlags.NonPublic, null, _abstractReadMethodParams, null);
        }

        /// <summary>
        /// Retourne une instance de la factory.
        /// </summary>
        public static DataRecordAdapterFactory Instance { get; } = new DataRecordAdapterFactory();

        /// <summary>
        /// Crée un adapteur.
        /// </summary>
        /// <param name="record">Record.</param>
        /// <param name="t">Type du bean.</param>
        /// <returns>Adapter.</returns>
        public object CreateAdapter(IDataRecord record, Type t)
        {
            var adapterContractType = typeof(IDataRecordAdapter<>).MakeGenericType(t);
            var tb = _dynamicModule.DefineType(
                "Kinetix.Broker.DataRecordAdapters.Adapter" + ++_adapterNum,
                TypeAttributes.Public,
                typeof(AbstractDataReaderAdapter),
                new Type[] { adapterContractType });

            CreateReadMethod(tb, record, t);

            var adapterType = tb.CreateTypeInfo();

            return Activator.CreateInstance(adapterType);
        }

        /// <summary>
        /// Crée la méthode de lecture.
        /// </summary>
        /// <param name="tb">Type builder.</param>
        /// <param name="record">Record.</param>
        /// <param name="t">Type du bean.</param>
        private void CreateReadMethod(TypeBuilder tb, IDataRecord record, Type t)
        {
            var mb = tb.DefineMethod(
                "Read",
                MethodAttributes.Public | MethodAttributes.HideBySig |
                MethodAttributes.NewSlot | MethodAttributes.Final | MethodAttributes.Virtual,
                CallingConventions.Standard,
                t,
                _readMethodParams);

            var mthdIL = mb.GetILGenerator();

            var bean = mthdIL.DeclareLocal(t);
            var lbl = mthdIL.DefineLabel();

            var localMap = new Dictionary<string, LocalBuilder>();

            mthdIL.Emit(OpCodes.Ldarg_1);
            mthdIL.Emit(OpCodes.Stloc, bean);
            mthdIL.Emit(OpCodes.Ldloc, bean);

            // Si le bean passé en paramètre est différent de null, on ne crée pas une nouvelle instance.
            mthdIL.Emit(OpCodes.Brtrue_S, lbl);

            mthdIL.Emit(OpCodes.Newobj, t.GetConstructor(_constructorParams));
            mthdIL.Emit(OpCodes.Stloc, bean);

            mthdIL.MarkLabel(lbl);

            // Création d'un dictionnaire d'accès aux propriétés du bean.
            PropertyInfo prop;
            var definition = BeanDescriptor.GetDefinition(t, true);
            var dic = new Dictionary<string, PropertyInfo>();
            foreach (var property in definition.Properties)
            {
                prop = t.GetProperty(property.PropertyName);
                dic.Add(property.PropertyName.ToLowerInvariant(), prop);
                if (property.MemberName != null)
                {
                    dic.Add(property.MemberName.ToLowerInvariant(), prop);
                }
            }

            LocalBuilder obj;
            for (var i = 0; i < record.FieldCount; ++i)
            {
                var fieldPathItems = record.GetName(i).Split('.');

                if (dic.TryGetValue(fieldPathItems[0].ToLowerInvariant(), out prop))
                {
                    obj = bean;
                    if (fieldPathItems.Length > 1)
                    {
                        // Déclaration récursive des variables si nécessaire.
                        var objName = string.Join(".", fieldPathItems, 0, fieldPathItems.Length - 1);
                        if (!localMap.TryGetValue(objName, out obj))
                        {
                            for (var j = 0; j < fieldPathItems.Length - 1; ++j)
                            {
                                objName = string.Join(".", fieldPathItems, 0, j + 1);
                                if (!localMap.TryGetValue(objName, out obj))
                                {
                                    // Chargement de la variable.
                                    obj = j > 0
                                        ? localMap[string.Join(".", fieldPathItems, 0, j)]
                                        : bean;

                                    mthdIL.Emit(OpCodes.Ldloc, obj);

                                    // Appel de la propriété.
                                    prop = obj.LocalType.GetProperty(fieldPathItems[j]);
                                    if (prop == null)
                                    {
                                        foreach (var p in obj.LocalType.GetProperties())
                                        {
                                            var attr = p.GetCustomAttributes(typeof(ColumnAttribute), false);
                                            if (attr.Length > 0 && ((ColumnAttribute)attr[0]).Name == fieldPathItems[j])
                                            {
                                                prop = p;
                                                break;
                                            }
                                        }
                                    }

                                    mthdIL.Emit(OpCodes.Callvirt, prop.GetGetMethod());

                                    // Affectation de la variable.
                                    obj = mthdIL.DeclareLocal(prop.PropertyType);
                                    mthdIL.Emit(OpCodes.Stloc, obj);

                                    localMap.Add(objName, obj);
                                }
                            }
                        }

                        var propertyName = fieldPathItems[^1];
                        prop = obj.LocalType.GetProperty(propertyName);
                        if (prop == null)
                        {
                            foreach (var p in obj.LocalType.GetProperties())
                            {
                                var attr = p.GetCustomAttributes(typeof(ColumnAttribute), false);
                                if (attr.Length > 0 && ((ColumnAttribute)attr[0]).Name == propertyName)
                                {
                                    prop = p;
                                    break;
                                }
                            }
                        }

                        if (prop == null)
                        {
                            throw new NotSupportedException("Unable to find property " + propertyName + " in " + obj.LocalType.FullName + " while constructing DataRecordAdapter for type " + t.FullName);
                        }
                    }

                    mthdIL.Emit(OpCodes.Ldloc, obj);
                    mthdIL.Emit(OpCodes.Ldarg_2);
                    mthdIL.Emit(OpCodes.Ldc_I4, i);
                    mthdIL.Emit(OpCodes.Call, GetReadMethodByType(prop.PropertyType));
                    mthdIL.Emit(OpCodes.Callvirt, prop.GetSetMethod(true));
                }
            }

            mthdIL.Emit(OpCodes.Ldloc, bean);
            mthdIL.Emit(OpCodes.Ret);
        }

        /// <summary>
        /// Retourne la méthode de lecture adapté au type.
        /// </summary>
        /// <param name="type">Type cible.</param>
        /// <returns>Méthode de lecture.</returns>
        private MethodInfo GetReadMethodByType(Type type)
        {
            if (type == typeof(int?))
            {
                return _readIntMethodInfo;
            }
            else if (type == typeof(string))
            {
                return _readStringMethodInfo;
            }
            else if (type == typeof(decimal?))
            {
                return _readDecimalMethodInfo;
            }
            else if (type == typeof(DateTime?))
            {
                return _readDateTimeMethodInfo;
            }
            else if (type == typeof(DateTime))
            {
                return _readDateTimeMethodInfo;
            }
            else if (type == typeof(short?))
            {
                return _readShortMethodInfo;
            }
            else if (type == typeof(bool?))
            {
                return _readBooleanMethodInfo;
            }
            else if (type == typeof(bool))
            {
                return _readNonNullableBooleanMethodIndo;
            }
            else if (type == typeof(byte?))
            {
                return _readByteMethodInfo;
            }
            else if (type == typeof(int))
            {
                return _readIntMethodInfo;
            }
            else if (type == typeof(long?))
            {
                return _readLongMethodInfo;
            }
            else if (type == typeof(float?))
            {
                return _readFloatMethodInfo;
            }
            else if (type == typeof(double?))
            {
                return _readDoubleMethodInfo;
            }
            else if (type == typeof(char[]))
            {
                return _readCharArrayMethodInfo;
            }
            else if (type == typeof(char?))
            {
                return _readCharMethodInfo;
            }
            else if (type == typeof(Guid?))
            {
                return _readGuidMethodInfo;
            }
            else if (type == typeof(byte[]))
            {
                return _readByteArrayMethodInfo;
            }
            else if (type == typeof(TimeSpan?))
            {
                return _readTimeSpanMethodInfo;
            }
            else if (type == typeof(TimeSpan))
            {
                return _readTimeSpanMethodInfo;
            }
            else
            {
                throw new NotSupportedException("No read method defined in DataRecordFactory for type : " + type.FullName);
            }
        }
    }
}
