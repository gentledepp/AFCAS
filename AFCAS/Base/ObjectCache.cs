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
    using System.Collections.Generic;
    using System.Threading;
    using Utils;

    /// <summary>
    /// A cache for objects to store them for the duration of a logical transaction.
    /// This class is NOT thread-safe!
    /// </summary>
    public class ObjectCache: IDisposable {
        private readonly IDictionary< Type, IDictionary< string, object > > _CacheCache;

        private object _SyncRoot;

        public ObjectCache( ) {
            _CacheCache = new Dictionary< Type, IDictionary< string, object > >( );
        }

        private object SyncRoot {
            get {
                Interlocked.CompareExchange( ref _SyncRoot, new object( ), null );
                return _SyncRoot;
            }
        }


        internal static Stack< ObjectCache > CacheStack {
            get {
                LocalDataStoreSlot ds = Thread.GetNamedDataSlot( "ObjectCache.CacheStack" );
                Stack< ObjectCache > stack = Thread.GetData( ds ) as Stack< ObjectCache >;
                if( stack == null ) {
                    stack = new Stack< ObjectCache >( );
                    Thread.SetData( ds, stack );
                }
                return stack;
            }
        }

        public static ObjectCache Current {
            get {
                Stack< ObjectCache > stack = CacheStack;
                if( stack.Count == 0 ) {
                    ObjectCache res = new ObjectCache( );
                    stack.Push( res );
                }

                return stack.Peek( );
            }
        }

        #region IDisposable Members

        public void Dispose( ) {
            Dispose( true );
            GC.SuppressFinalize( this );
        }

        #endregion

        protected virtual void Dispose( bool shouldClean ) {
            _CacheCache.Clear( );
        }

        private IDictionary< string, object > GetCache< T >( ) {
            lock( SyncRoot ) {
                IDictionary< string, object > cache;
                if( !_CacheCache.TryGetValue( typeof( T ), out cache ) ) {
                    cache = new Dictionary< string, object >( );
                    _CacheCache.Add( typeof( T ), cache );
                }
                return cache;
            }
        }

        private T PutOrUpdateExisting< T >( T element, bool throwIfExists ) where T: AbstractKeyedNamed< T > {
            IDictionary< string, object > cache;
            lock( cache = GetCache< T >( ) ) {
                T elem = element;
                object res;
                if( cache.TryGetValue( element.Key, out res ) ) {
                    if( !ReferenceEquals( res, element ) ) {
                        if( throwIfExists ) {
                            throw new ArgumentException( "Identical objects with different memory references deteched" );
                        }
                        elem = ( T )res;
                        elem.CopyFrom( element );
                    }
                } else {
                    cache.Add( element.Key, element );
                }
                return elem;
            }
        }


        public static void PushCurrent( ObjectCache cache ) {
            if( cache == null ) {
                throw new ArgumentNullException( "cache" );
            }
            CacheStack.Push( cache );
        }

        public static void PopCurrent( ) {
            if( CacheStack.Count == 0 ) {
                throw new InvalidProgramException( "No current ObjectCache set" );
            }
            CacheStack.Pop( );
        }

        public void Complete( ) {
            //currently noop
        }

        public void Put< T >( T element ) where T: AbstractKeyedNamed< T > {
            PutOrUpdateExisting( element, true );
        }

        public T PutOrUpdate< T >( T element ) where T: AbstractKeyedNamed< T > {
            return PutOrUpdateExisting( element, false );
        }

        public T Get< T >( string key ) where T: AbstractKeyedNamed< T > {
            IDictionary< string, object > cache;
            lock( cache = GetCache< T >( ) ) {
                object res;
                if( cache.TryGetValue( key, out res ) ) {
                    return ( T )res;
                }
                return default( T );
            }
        }

        public bool Remove< T >( string id ) where T: AbstractKeyedNamed< T > {
            IDictionary< string, object > cache;
            lock( cache = GetCache< T >( ) ) {
                return cache.Remove( id );
            }
        }

        public IList< T > GetAll< T >( ) {
            IDictionary< string, object > cache;
            lock( cache = GetCache< T >( ) ) {
                List< T > res = new List< T >( cache.Count );
                foreach( T elem in cache.Values ) {
                    res.Add( elem );
                }
                return res;
            }
        }

        public IEnumerator< T > GetEnumerator< T >( ) where T: AbstractKeyedNamed< T > {
            return new EnumeratorWrapper< T >( GetCache< T >( ).Values.GetEnumerator( ) );
        }
    }
}