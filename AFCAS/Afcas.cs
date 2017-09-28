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

namespace Afcas {
    using System;
    using System.Collections.Generic;
    using System.Threading;
    using Impl;
    using Objects;

    public static class Afcas {
        private static readonly Dictionary< string, ResourceHandleFactory > _FactoryList =
                new Dictionary< string, ResourceHandleFactory >( );

        private static IAuthorizationManager _AuthManager;

        private static object _SyncRoot;

        private static object SyncRoot {
            get {
                Interlocked.CompareExchange( ref _SyncRoot, new object( ), null );
                return _SyncRoot;
            }
        }

        public static IAuthorizationManager GetAuthorizationManager( ) {
            lock( SyncRoot ) {
                if( _AuthManager == null ) {
                    _AuthManager = new AuthorizationManager( );
                }
                return _AuthManager;
            }
        }

        public static IAuthorizationProvider GetAuthorizationProvider( ) {
            return GetAuthorizationManager( );
        }

        public static void RegisterHandleFactory( ResourceHandleFactory factory ) {
            lock( SyncRoot ) {
                if( _FactoryList.ContainsKey( factory.ResourceType ) ) {
                    throw new ArgumentException( factory.ResourceType + " already have a registered ResourceHandleFactory" );
                }
                _FactoryList.Add( factory.ResourceType, factory );
            }
        }

        public static ResourceHandleFactory GetHandleFactory( string resourceType ) {
            ResourceHandleFactory gen;
            lock( SyncRoot ) {
                if( !_FactoryList.TryGetValue( resourceType, out gen ) ) {
                    throw new ArgumentException( "No ResourceHandleFactory registered for resource type: " + resourceType );
                }
            }
            return gen;
        }

        internal static bool IsHandleFactoryRegistered( string resourceType ) {
            lock( SyncRoot ) {
                return _FactoryList.ContainsKey( resourceType );
            }
        }
    }
}