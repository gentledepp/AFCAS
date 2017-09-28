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
    using System.Collections.Generic;
    using Objects;

    /// <summary>
    /// The main interface to be used by the clients that need to make authorization decisions.
    /// An instance of this interface is provided by <see cref="Afcas"/> class.
    /// </summary>
    public interface IAuthorizationProvider {
        // the method to justify the existence of this interface
        bool IsAuthorized( string principalId, string operationId, ResourceHandle resource );

        // these method also have uses for authorization purposes
        bool IsMemberOf( string groupId, string memberId );
        bool IsSubOperation( string opId, string subOpId );
        bool IsSubResource( ResourceHandle resource, ResourceHandle subResource );

        // These two methods are for offline support
        IList< ResourceAccessPredicate > GetAuthorizationDigest( string principalId );
        IList< Operation > GetAuthorizedOperations( string principalId, ResourceHandle resource );

        // This can be used to allow the user to browse authorized resources
        IList< ResourceHandle > GetAuthorizedResources( string principalId, string operationId );
    }
}