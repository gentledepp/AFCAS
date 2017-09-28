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
    /// This is the interface to be used by a management tool. 
    /// An instance of this interface is provided by <see cref="Afcas"/> static class.
    /// Currently, there is no management tool implemented for AFCAS with
    /// the required UI and so forth. However, a full implementation of this interface
    /// is provided. I am planning to provide a web based UI for this
    /// in the future.
    /// 
    /// </summary>
    public interface IAuthorizationManager: IAuthorizationProvider {
        //CRUD for principals
        void AddOrUpdate( Principal pr, string source );
        void RemovePrincipal( string id );

        //CRUD for operations
        void AddOrUpdate( Operation pr );
        void RemoveOperation( string id );

        // These two methods are for maintaining the ACL
        void AddAccessPredicate( string principalId, string operationId, ResourceHandle resource, ResourceAccessPredicateType type );
        void RemoveAccessPredicate( string principalId, string operationId, ResourceHandle resource, ResourceAccessPredicateType type );

        // These are to maintain the hierarchy of principal, operation, and resources
        void AddGroupMember( Principal group, Principal member );
        void RemoveGroupMember( Principal group, Principal member );
        void AddSubOperation( Operation parent, Operation subOperation );
        void RemoveSubOperation( Operation parent, Operation subOperation );
        void AddSubResource( ResourceHandle resource, ResourceHandle subResource );
        void RemoveSubResource( ResourceHandle resource, ResourceHandle subResource );

        // these are for listing purposes
        IList< Principal > GetPrincipalList( );
        IList< Principal > GetPrincipalList( PrincipalType type );
        IList< Principal > GetMembersList( Principal pr );
        IList< Principal > GetFlatMembersList( Principal pr );

        IList< Operation > GetOperationList( );
        IList< Operation > GetSubOperationsList( Operation op );
        IList< Operation > GetFlatSubOperationsList( Operation op );
    }
}