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


#if DEBUG

namespace Afcas.Test {
    using System;
    using System.Security.Permissions;
    using Base;
    using Objects;

    [ Serializable ]
    public class SampleResource: AbstractKeyedNamed< SampleResource > {
        private int _Int32Test;
        private string _StringTest;
        private SampleResource( ) {}

        public string StringTest {
            get {
                return _StringTest;
            }
            set {
                _StringTest = value;
            }
        }

        public int Int32Test {
            get {
                return _Int32Test;
            }
            set {
                _Int32Test = value;
            }
        }

        protected override void InitInstance( object[ ] initParams ) {}

        [ SecurityPermission( SecurityAction.LinkDemand, Flags = SecurityPermissionFlag.SerializationFormatter ) ]
        public static SampleResource GetOrCreateSampleResource( string key ) {
            return ObjectCache.Current.Get< SampleResource >( key ) ?? Create( key, "" );
        }
    }

    public class SampleResourceHandle: ResourceHandle {
        private readonly string _Key;

        public SampleResourceHandle( ResourceHandleFactory fac, SampleResource res ): base( fac ) {
            _Key = res.Key;
        }

        public SampleResourceHandle( ResourceHandleFactory fac, string key ): base( fac ) {
            _Key = key;
        }

        public override string Key {
            get {
                return _Key;
            }
        }
    }

    public class SampleResourceHandleFactory: ResourceHandleFactory {
        public SampleResourceHandleFactory( ): base( "SampleResource" ) {}

        protected internal override ResourceHandle GenerateResourceHandleByKey( string resourceId ) {
            return new SampleResourceHandle( this, SampleResource.GetOrCreateSampleResource( resourceId ) );
        }

        public override ResourceHandle GenerateResourceHandle( object resource ) {
            return new SampleResourceHandle( this, SampleResource.GetOrCreateSampleResource( ( ( SampleResource )resource ).Key ) );
        }
    }
}

#endif