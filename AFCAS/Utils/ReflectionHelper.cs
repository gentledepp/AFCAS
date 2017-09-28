#region copyright

// Copyright (C) 2008 Kemal ERDOGAN
// 
// This program is free software: you can redistribute it and/or modify
// it under the terms of the GNU Lesser General Public License as published by
// the Free Software Foundation, version 3 of the License.
// 
// This program is distributed in the hope that it will be useful,
// but WITHOUT ANY WARRANTY; without even the implied warranty of
// MERCHANTABILITY or FITNESS FOR A PARTICULAR PURPOSE.  See the
// GNU Lesser General Public License for more details.
// 
// You should have received a copy of the GNU Lesser General Public License
// along with this program.  If not, see <http://www.gnu.org/licenses/>.

#endregion

namespace Afcas.Utils {
    using System;
    using System.Collections.Generic;
    using System.Globalization;
    using System.Reflection;
    using System.Reflection.Emit;
    using System.Threading;

    internal static class ReflectionHelper {
        #region Delegates

        public delegate object FastMemberGetter( object obj );

        public delegate void FastMemberSetter( object obj, object value );

        #endregion

        private static readonly Dictionary< string, Delegate > _MemberDelegates = new Dictionary< string, Delegate >( );
        private static object _SyncRoot;

        private static object SyncRoot {
            get {
                Interlocked.CompareExchange( ref _SyncRoot, new object( ), null );
                return _SyncRoot;
            }
        }

        private static FastMemberGetter GetFieldGetter( Type objectType, string fieldName ) {
            FieldInfo fi;
            Type st = objectType;
            do {
                fi = st.GetField( fieldName, BindingFlags.NonPublic|BindingFlags.Public|BindingFlags.Instance|BindingFlags.GetField );
                st = st.BaseType;
            } while( fi == null && st != typeof( object ) );

            if( fi == null ) {
                throw new ArgumentException( String.Format( CultureInfo.CurrentCulture,
                                                            "'{0}' is not a field of type: '{1}'",
                                                            fieldName,
                                                            objectType.Name ) );
            }

            DynamicMethod dm = new DynamicMethod( "Get" + fieldName, typeof( object ), new[ ] { typeof( object ) }, objectType, true );
            ILGenerator il = dm.GetILGenerator( );
            il.Emit( OpCodes.Ldarg_0 );
            il.Emit( OpCodes.Ldfld, fi );
            if( fi.FieldType.IsValueType ) {
                il.Emit( OpCodes.Box, fi.FieldType );
            }
            il.Emit( OpCodes.Ret );

            return ( FastMemberGetter )dm.CreateDelegate( typeof( FastMemberGetter ) );
        }

        private static FastMemberSetter GetFieldSetter( Type objectType, string fieldName ) {
            FieldInfo fi;
            Type st = objectType;
            do {
                fi = st.GetField( fieldName, BindingFlags.NonPublic|BindingFlags.Public|BindingFlags.Instance|BindingFlags.SetField );
                st = st.BaseType;
            } while( fi == null && st != typeof( object ) );

            if( fi == null ) {
                throw new ArgumentException( String.Format( CultureInfo.CurrentCulture,
                                                            "'{0}' is not a field of type: '{1}'",
                                                            fieldName,
                                                            objectType.Name ) );
            }

            DynamicMethod dm = new DynamicMethod( "Set" + fieldName,
                                                  typeof( void ),
                                                  new[ ] { typeof( object ), typeof( object ) },
                                                  fi.Module,
                                                  true );
            ILGenerator il = dm.GetILGenerator( );
            il.Emit( OpCodes.Ldarg_0 );
            il.Emit( OpCodes.Ldarg_1 );
            if( fi.FieldType.IsValueType ) {
                il.Emit( OpCodes.Unbox_Any, fi.FieldType );
            }
            il.Emit( OpCodes.Stfld, fi );
            il.Emit( OpCodes.Ret );

            return ( FastMemberSetter )dm.CreateDelegate( typeof( FastMemberSetter ) );
        }

        private static FastMemberGetter GetPropertyGetter( Type objectType, string propertyName ) {
            PropertyInfo pi = objectType.GetProperty( propertyName,
                                                      BindingFlags.NonPublic|BindingFlags.Public|BindingFlags.Instance
                                                      |BindingFlags.GetProperty );
            if( pi == null ) {
                throw new ArgumentException( String.Format( CultureInfo.CurrentCulture,
                                                            "There exists no property '{0}' of '{1}'",
                                                            propertyName,
                                                            objectType.Name ) );
            }

            MethodInfo mi = pi.GetGetMethod( true );
            if( mi == null ) {
                throw new ArgumentException( String.Format( CultureInfo.CurrentCulture,
                                                            "Property '{0}' of '{1}' does not have a getter",
                                                            propertyName,
                                                            objectType.Name ) );
            }
            //return ( FastMemberGetter )Delegate.CreateDelegate( typeof( FastMemberGetter ), mi, true );

            DynamicMethod dm = new DynamicMethod( "Get" + propertyName, typeof( object ), new[ ] { typeof( object ) }, objectType, true );
            ILGenerator il = dm.GetILGenerator( );

            il.Emit( OpCodes.Ldarg_0 );
            il.EmitCall( OpCodes.Callvirt, pi.GetGetMethod( ), null );
            if( pi.PropertyType.IsValueType ) {
                il.Emit( OpCodes.Box, pi.PropertyType );
            }
            il.Emit( OpCodes.Ret );
            return ( FastMemberGetter )dm.CreateDelegate( typeof( FastMemberGetter ) );
        }

        private static FastMemberSetter GetPropertySetter( Type objectType, string propertyName ) {
            PropertyInfo pi = objectType.GetProperty( propertyName,
                                                      BindingFlags.NonPublic|BindingFlags.Public|BindingFlags.Instance
                                                      |BindingFlags.SetProperty );
            if( pi == null ) {
                throw new ArgumentException( String.Format( CultureInfo.CurrentCulture,
                                                            "There exists no property '{0}' of '{1}'",
                                                            propertyName,
                                                            objectType.Name ) );
            }

            MethodInfo mi = pi.GetSetMethod( true );
            if( mi == null ) {
                throw new ArgumentException( String.Format( CultureInfo.CurrentCulture,
                                                            "Property '{0}' of '{1}' does not have a setter",
                                                            propertyName,
                                                            objectType.Name ) );
            }
            //return ( FastMemberSetter )Delegate.CreateDelegate( typeof( FastMemberSetter ), mi );

            DynamicMethod dm = new DynamicMethod( "Set" + propertyName,
                                                  typeof( void ),
                                                  new[ ] { typeof( object ), typeof( object ) },
                                                  objectType,
                                                  true );
            ILGenerator il = dm.GetILGenerator( );
            il.Emit( OpCodes.Ldarg_0 );
            il.Emit( OpCodes.Ldarg_1 );
            if( pi.PropertyType.IsValueType ) {
                il.Emit( OpCodes.Unbox_Any, pi.PropertyType );
            }
            il.EmitCall( OpCodes.Callvirt, pi.GetSetMethod( true ), null );

            il.Emit( OpCodes.Ret );
            return ( FastMemberSetter )dm.CreateDelegate( typeof( FastMemberSetter ) );
        }

        private static Delegate GetDelegateWithCacheLookup( CreateGetterSetterDelegate gsDelegate, string key ) {
            lock( SyncRoot ) {
                Delegate del;
                if( !_MemberDelegates.TryGetValue( key, out del ) ) {
                    del = gsDelegate( );
                    _MemberDelegates.Add( key, del );
                }
                return del;
            }
        }

        public static FastMemberGetter GetCachedPropertyGetter( Type objectType, string memberName ) {
            string key = objectType.FullName + ".Get" + memberName;
            return ( FastMemberGetter )GetDelegateWithCacheLookup( delegate {
                                                                       return GetPropertyGetter( objectType, memberName );
                                                                   },
                                                                   key );
        }


        public static FastMemberGetter GetCachedFieldGetter( Type objectType, string memberName ) {
            string key = objectType.FullName + ".Get" + memberName;
            return ( FastMemberGetter )GetDelegateWithCacheLookup( delegate {
                                                                       return GetFieldGetter( objectType, memberName );
                                                                   },
                                                                   key );
        }

        public static FastMemberSetter GetCachedPropertySetter( Type objectType, string memberName ) {
            string key = objectType.FullName + ".Set" + memberName;
            return ( FastMemberSetter )GetDelegateWithCacheLookup( delegate {
                                                                       return GetPropertySetter( objectType, memberName );
                                                                   },
                                                                   key );
        }

        public static FastMemberSetter GetCachedFieldSetter( Type objectType, string memberName ) {
            string key = objectType.FullName + ".Set" + memberName;
            return ( FastMemberSetter )GetDelegateWithCacheLookup( delegate {
                                                                       return GetFieldSetter( objectType, memberName );
                                                                   },
                                                                   key );
        }

        #region Nested type: CreateGetterSetterDelegate

        private delegate Delegate CreateGetterSetterDelegate( );

        #endregion
    }
}