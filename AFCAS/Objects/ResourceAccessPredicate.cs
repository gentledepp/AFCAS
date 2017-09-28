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

    [ Serializable ]
    public struct ResourceAccessPredicate {
        private readonly ResourceAccessPredicateType _AccessPredicateType;
        private readonly string _OperationId;
        private readonly string _PrincipalId;
        private readonly ResourceHandle _Resource;

        public ResourceAccessPredicate( string principalId,
                                        string operationId,
                                        ResourceHandle resource,
                                        ResourceAccessPredicateType accessPredicateType ) {
            _AccessPredicateType = accessPredicateType;
            _PrincipalId = principalId;
            _Resource = resource;
            _OperationId = operationId;
        }


        public string PrincipalId {
            get {
                return _PrincipalId;
            }
        }

        public string OperationId {
            get {
                return _OperationId;
            }
        }

        public ResourceHandle Resource {
            get {
                return _Resource;
            }
        }

        public ResourceAccessPredicateType AccessPredicateType {
            get {
                return _AccessPredicateType;
            }
        }

        public override bool Equals( object obj ) {
            if( !( obj is ResourceAccessPredicate ) ) {
                return false;
            }
            ResourceAccessPredicate other = ( ResourceAccessPredicate )obj;

            return string.Equals( _Resource.Key, other.Resource.Key ) && string.Equals( PrincipalId, other.PrincipalId )
                   && string.Equals( OperationId, other.OperationId ) && AccessPredicateType == other.AccessPredicateType;
        }

        public override int GetHashCode( ) {
            unchecked {
                int res = _Resource.Key.GetHashCode( );
                res = res*17 + PrincipalId.GetHashCode( );
                res = res*17 + OperationId.GetHashCode( );
                res = res*17 + AccessPredicateType.GetHashCode( );
                return res;
            }
        }

        public static bool operator ==( ResourceAccessPredicate left, ResourceAccessPredicate right ) {
            return left.Equals( right );
        }

        public static bool operator !=( ResourceAccessPredicate left, ResourceAccessPredicate right ) {
            return !left.Equals( right );
        }
    }
}