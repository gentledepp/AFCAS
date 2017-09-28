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

namespace Afcas.Objects {
    public class NullResource: ResourceHandle {
        private static readonly NullResourceHandleFactory _NullFactory = new NullResourceHandleFactory( );

        public static NullResource Instance = new NullResource( );

        private NullResource( ): base( _NullFactory ) {}

        public override string Key {
            get {
                return "";
            }
        }

        #region Nested type: NullResourceHandleFactory

        private class NullResourceHandleFactory: ResourceHandleFactory {
            public NullResourceHandleFactory( ): base( "" ) {}

            public override ResourceHandle GenerateResourceHandle( object resource ) {
                return Instance;
            }

            protected internal override ResourceHandle GenerateResourceHandleByKey( string resourceId ) {
                return Instance;
            }
        }

        #endregion
    }
}