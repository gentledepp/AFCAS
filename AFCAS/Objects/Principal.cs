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
    using System;
    using System.Security.Permissions;
    using Base;

    [ Serializable ]
    public class Principal: AbstractKeyedNamed< Principal > {
        private string _EMail;
        private PrincipalType _PrincipalType;

        private Principal( ) {}

        public PrincipalType PrincipalType {
            get {
                return _PrincipalType;
            }
            protected internal set {
                _PrincipalType = value;
            }
        }

        public string EMail {
            get {
                return _EMail;
            }
            protected internal set {
                if( null == value ) {
                    throw new ArgumentNullException( "EMail" );
                }
                _EMail = value;
            }
        }

        [ SecurityPermission( SecurityAction.LinkDemand, Flags = SecurityPermissionFlag.SerializationFormatter ) ]
        public static Principal CreatePrincipal( string id, string name, PrincipalType principalType, string email ) {
            return Create( id, name, principalType, email );
        }

        protected override void InitInstance( object[ ] initParams ) {
            _PrincipalType = ( PrincipalType )initParams[ 0 ];
            _EMail = ( string )initParams[ 1 ];
        }

        public override void CopyFrom( Principal other ) {
            base.CopyFrom( other );
            _PrincipalType = other.PrincipalType;
            _EMail = other.EMail;
        }
    }
}