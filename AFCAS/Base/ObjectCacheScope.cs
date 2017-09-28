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

    public enum ObjectCacheScopeOption {
        Required,
        RequiresNew
    }

    public class ObjectCacheScope: IDisposable {
        private readonly ObjectCache _Cache;
        private readonly bool _IsOwnCache;
        private bool _Disposed;

        public ObjectCacheScope( ): this( ObjectCacheScopeOption.Required ) {}

        public ObjectCacheScope( ObjectCache cacheToUse ) {
            if( cacheToUse == null ) {
                throw new ArgumentNullException( "cacheToUse" );
            }
            _Cache = cacheToUse;
            ObjectCache.CacheStack.Push( cacheToUse );
        }

        public ObjectCacheScope( ObjectCacheScopeOption option ) {
            switch( option ) {
                case ObjectCacheScopeOption.Required:
                    if( ObjectCache.CacheStack.Count == 0 ) {
                        _Cache = new ObjectCache( );
                        ObjectCache.CacheStack.Push( _Cache );
                        _IsOwnCache = true;
                    } else {
                        _Cache = ObjectCache.CacheStack.Peek( );
                    }
                    break;
                case ObjectCacheScopeOption.RequiresNew:
                    _Cache = new ObjectCache( );
                    _IsOwnCache = true;
                    ObjectCache.CacheStack.Push( _Cache );
                    break;
                default:
                    throw new InvalidProgramException( "Unsupported ObjectCacheScopeOption" );
            }
        }

        #region IDisposable Members

        public void Dispose( ) {
            Dispose( true );
            GC.SuppressFinalize( this );
        }

        #endregion

        ~ObjectCacheScope( ) {
            Dispose( false );
        }

        public void Complete( ) {
            if( _IsOwnCache ) {
                _Cache.Complete( );
            }
        }

        protected virtual void Dispose( bool disposeManagedResources ) {
            if( _Disposed ) {
                return;
            }

            // remove topmost cache
            //ObjectCache ctx = null;
            //if( ObjectCache.CacheStack.Count > 0 ) {
            //    ctx = ObjectCache.CacheStack.Peek( );
            //}
            //if( ctx != _Cache ) {
            //    throw new InvalidProgramException( "Object cache is not topmost" );
            //}
            ObjectCache.CacheStack.Pop( );

            if( disposeManagedResources && _IsOwnCache ) {
                ( ( IDisposable )_Cache ).Dispose( );
            }
            _Disposed = true;
        }
    }
}