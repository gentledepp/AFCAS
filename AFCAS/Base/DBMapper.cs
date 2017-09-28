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

    public abstract class DBMapper< T >
            where T: AbstractKeyedNamed< T > {
        [ SecurityPermission( SecurityAction.LinkDemand, Flags = SecurityPermissionFlag.SerializationFormatter ) ]
        public virtual T Construct( DataRow row ) {
            T res = ( T )FormatterServices.GetUninitializedObject( typeof( T ) );
            object[ ] ia = row.ItemArray;
            object[ ] pl = new object[ia.Length - 2];
            Array.Copy( ia, 2, pl, 0, pl.Length );
            res.Init( ( string )row[ 0 ], ( string )row[ 1 ], pl );
            return res;
        }

        public abstract void AddOrUpdateDB( T element );
        public abstract void RemoveFromDB( string key );
            }
}