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

namespace Afcas.Base {
    using System;
    using System.Data;
    using System.Runtime.Serialization;
    using System.Security.Permissions;

    /// <summary>
    /// A base class for types that unifies key identity with memory identity. 
    /// i.e. If an instance of a descendent exists with a Key property of value 'X'
    /// there can exist only that specific instance in memory. And it is not possible
    /// to create another one with the same Key value.
    /// 
    /// Instances of this type can only be created using static method <see cref="Create"/>
    /// </summary>
    /// <typeparam name="T">The actual type of the object</typeparam>
    [ Serializable ]
    public abstract class AbstractKeyedNamed< T >
            where T: AbstractKeyedNamed< T > {
        private string _Description;
        private string _Key;
        private string _Name;

        #region cache & instance management 

        /// <summary>
        /// The replacement for a constructor call, which ensures no two objects with the same key is
        /// ever created within the same <see cref="ObjectCache"/>. Concrate subclasses must provide a 
        /// public static method that would replace generic parameters here with a typed one.
        /// </summary>
        /// <param name="key">the key of the new object</param>
        /// <param name="name">a name for the new object.</param>
        /// <param name="initParams">the other parameters that are needed to properly initialize a 
        /// new object of type {T}</param>
        /// <returns></returns>
        [ SecurityPermission( SecurityAction.LinkDemand, Flags = SecurityPermissionFlag.SerializationFormatter ) ]
        protected static T Create( string key, string name, params object[ ] initParams ) {
            ObjectCache cache = ObjectCache.Current;

            T res = cache.Get< T >( key );
            if( res == default( T ) ) {
                res = ( T )FormatterServices.GetUninitializedObject( typeof( T ) );
                res.Init( key, name, initParams );
                cache.Put( res );
                return res;
            }
            throw new ArgumentException( "An object with the same key exists in current object cache. Key: " + key ?? "null" );
        }

        #endregion

        public string Key {
            get {
                return _Key;
            }
        }

        public string Name {
            get {
                return _Name;
            }
            protected internal set {
                if( value == null ) {
                    throw new ArgumentNullException( "value" );
                }
                _Name = value;
            }
        }

        public string Description {
            get {
                return _Description;
            }
            set {
                if( value == null ) {
                    throw new ArgumentNullException( "value" );
                }
                _Description = value;
            }
        }

        internal void Init( string key, string name, params object[ ] initParams ) {
            _Key = key;
            _Name = name;
            _Description = "";
            InitInstance( initParams );
        }

        internal void Init( DataRow row ) {
            _Key = ( string )row[ 0 ];
            _Name = ( string )row[ 1 ];
            _Description = ( string )row[ 2 ];
        }

        /// <summary>
        /// This method is intended to replace a constructor call. As such, the object cannot 
        /// be asssumed to be consistently initialized within. That is the duty of this method. 
        /// All inheriting classes must make a call to the base method at the beginning.
        /// </summary>
        /// <param name="initParams">The parameters that this particular type {T} requires 
        /// for correct initialization.</param>
        protected abstract void InitInstance( params object[ ] initParams );

        /// <summary>
        /// A method to copy data from one instance into another.
        /// 
        /// The inheritors must overrride this method for its own data and make sure to call
        /// the base version.
        /// </summary>
        /// <param name="other"></param>
        public virtual void CopyFrom( T other ) {
            _Key = other.Key;
            _Name = other.Name;
            _Description = other.Description;
        }
            }
}