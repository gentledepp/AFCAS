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

using System.Diagnostics.CodeAnalysis;

[ assembly:
        SuppressMessage( "Microsoft.Naming", "CA1704:IdentifiersShouldBeSpelledCorrectly", MessageId = "Afcas", Scope = "namespace",
                Target = "Afcas" ) ]
[ assembly:
        SuppressMessage( "Microsoft.Naming", "CA1704:IdentifiersShouldBeSpelledCorrectly", MessageId = "Afcas", Scope = "namespace",
                Target = "Afcas.Base" ) ]
[ assembly:
        SuppressMessage( "Microsoft.Naming", "CA1704:IdentifiersShouldBeSpelledCorrectly", MessageId = "Afcas", Scope = "namespace",
                Target = "Afcas.Objects" ) ]
[ assembly:
        SuppressMessage( "Microsoft.Naming", "CA1704:IdentifiersShouldBeSpelledCorrectly", MessageId = "Afcas", Scope = "namespace",
                Target = "Afcas.Test" ) ]
[ assembly:
        SuppressMessage( "Microsoft.Naming", "CA1704:IdentifiersShouldBeSpelledCorrectly", MessageId = "Utils", Scope = "namespace",
                Target = "Afcas.Utils" ) ]
[ assembly: SuppressMessage( "Microsoft.Design", "CA2210:AssembliesShouldHaveValidStrongNames" ) ]
[ assembly:
        SuppressMessage( "Microsoft.Naming", "CA1704:IdentifiersShouldBeSpelledCorrectly", MessageId = "Afcas", Scope = "namespace",
                Target = "Afcas.Utils" ) ]
[ assembly: SuppressMessage( "Microsoft.Design", "CA1014:MarkAssembliesWithClsCompliant" ) ]
[ assembly:
        SuppressMessage( "Microsoft.Naming", "CA1704:IdentifiersShouldBeSpelledCorrectly", MessageId = "Afcas", Scope = "type",
                Target = "Afcas.Afcas" ) ]
[ assembly: SuppressMessage( "Microsoft.Naming", "CA1704:IdentifiersShouldBeSpelledCorrectly", MessageId = "Afcas" ) ]
[ assembly:
        SuppressMessage( "Microsoft.Design", "CA1024:UsePropertiesWhereAppropriate", Scope = "member",
                Target = "Afcas.Afcas.#GetAuthorizationManager()" ) ]
[ assembly:
        SuppressMessage( "Microsoft.Design", "CA1024:UsePropertiesWhereAppropriate", Scope = "member",
                Target = "Afcas.Afcas.#GetAuthorizationProvider()" ) ]
[ assembly:
        SuppressMessage( "Microsoft.Design", "CA1024:UsePropertiesWhereAppropriate", Scope = "member",
                Target = "Afcas.IAuthorizationManager.#GetOperationList()" ) ]
[ assembly:
        SuppressMessage( "Microsoft.Naming", "CA1709:IdentifiersShouldBeCasedCorrectly", MessageId = "Op", Scope = "member",
                Target = "Afcas.IAuthorizationProvider.#IsSubOperation(System.String,System.String)" ) ]
[ assembly:
        SuppressMessage( "Microsoft.Naming", "CA1704:IdentifiersShouldBeSpelledCorrectly", MessageId = "Params", Scope = "member",
                Target = "Afcas.Base.AbstractKeyedNamed`1.#Create(System.String,System.String,System.Object[])" ) ]
[ assembly:
        SuppressMessage( "Microsoft.Naming", "CA1704:IdentifiersShouldBeSpelledCorrectly", MessageId = "Params", Scope = "member",
                Target = "Afcas.Base.AbstractKeyedNamed`1.#InitInstance(System.Object[])" ) ]
[ assembly:
        SuppressMessage( "Microsoft.Performance", "CA1822:MarkMembersAsStatic", Scope = "member",
                Target = "Afcas.Base.ObjectCache.#Complete()" ) ]
[ assembly:
        SuppressMessage( "Microsoft.Design", "CA1004:GenericMethodsShouldProvideTypeParameter", Scope = "member",
                Target = "Afcas.Base.ObjectCache.#Get`1(System.String)" ) ]
[ assembly:
        SuppressMessage( "Microsoft.Design", "CA1004:GenericMethodsShouldProvideTypeParameter", Scope = "member",
                Target = "Afcas.Base.ObjectCache.#GetAll`1()" ) ]
[ assembly:
        SuppressMessage( "Microsoft.Design", "CA1004:GenericMethodsShouldProvideTypeParameter", Scope = "member",
                Target = "Afcas.Base.ObjectCache.#GetEnumerator`1()" ) ]
[ assembly:
        SuppressMessage( "Microsoft.Design", "CA1004:GenericMethodsShouldProvideTypeParameter", Scope = "member",
                Target = "Afcas.Base.ObjectCache.#Remove`1(System.String)" ) ]
[ assembly:
        SuppressMessage( "Microsoft.Performance", "CA1811:AvoidUncalledPrivateCode", Scope = "member",
                Target = "Afcas.Impl.AuthorizationProvider.#CacheDurationInSeconds" ) ]
[ assembly:
        SuppressMessage( "Microsoft.Usage", "CA2211:NonConstantFieldsShouldNotBeVisible", Scope = "member",
                Target = "Afcas.Objects.NullResource.#Instance" ) ]
[ assembly:
        SuppressMessage( "Microsoft.Naming", "CA1702:CompoundWordsShouldBeCasedCorrectly", MessageId = "EMail", Scope = "member",
                Target = "Afcas.Objects.Principal.#EMail" ) ]
[ assembly:
        SuppressMessage( "Microsoft.Usage", "CA2208:InstantiateArgumentExceptionsCorrectly", Scope = "member",
                Target = "Afcas.Objects.Principal.#EMail" ) ]
[ assembly:
        SuppressMessage( "Microsoft.Naming", "CA1704:IdentifiersShouldBeSpelledCorrectly", MessageId = "fac", Scope = "member",
                Target = "Afcas.Test.SampleResourceHandle.#.ctor(Afcas.Objects.ResourceHandleFactory,Afcas.Test.SampleResource)" ) ]
[ assembly:
        SuppressMessage( "Microsoft.Naming", "CA1704:IdentifiersShouldBeSpelledCorrectly", MessageId = "fac", Scope = "member",
                Target = "Afcas.Test.SampleResourceHandle.#.ctor(Afcas.Objects.ResourceHandleFactory,System.String)" ) ]
[ assembly:
        SuppressMessage( "Microsoft.Performance", "CA1822:MarkMembersAsStatic", Scope = "member",
                Target = "Afcas.Test.TestAuthorizationManager.#Setup()" ) ]
[ assembly:
        SuppressMessage( "Microsoft.Performance", "CA1822:MarkMembersAsStatic", Scope = "member",
                Target = "Afcas.Test.TestAuthorizationManager.#TestCreateDag()" ) ]
[ assembly:
        SuppressMessage( "Microsoft.Performance", "CA1822:MarkMembersAsStatic", Scope = "member",
                Target = "Afcas.Test.TestAuthorizationManager.#TestCreateOperation()" ) ]
[ assembly:
        SuppressMessage( "Microsoft.Performance", "CA1822:MarkMembersAsStatic", Scope = "member",
                Target = "Afcas.Test.TestAuthorizationManager.#TestCreatePrincipal()" ) ]
[ assembly:
        SuppressMessage( "Microsoft.Performance", "CA1822:MarkMembersAsStatic", Scope = "member",
                Target = "Afcas.Test.TestAuthorizationManager.#TestDeleteOperation()" ) ]
[ assembly:
        SuppressMessage( "Microsoft.Performance", "CA1822:MarkMembersAsStatic", Scope = "member",
                Target = "Afcas.Test.TestAuthorizationManager.#TestDeletePrincipal()" ) ]
[ assembly:
        SuppressMessage( "Microsoft.Performance", "CA1822:MarkMembersAsStatic", Scope = "member",
                Target = "Afcas.Test.TestAuthorizationManager.#TestUpdateOperation()" ) ]
[ assembly:
        SuppressMessage( "Microsoft.Performance", "CA1822:MarkMembersAsStatic", Scope = "member",
                Target = "Afcas.Test.TestAuthorizationManager.#TestUpdatePrincipal()" ) ]
[ assembly:
        SuppressMessage( "Microsoft.Performance", "CA1822:MarkMembersAsStatic", Scope = "member",
                Target = "Afcas.Test.TestAuthorizationProvider.#Setup()" ) ]
[ assembly:
        SuppressMessage( "Microsoft.Performance", "CA1822:MarkMembersAsStatic", Scope = "member",
                Target = "Afcas.Test.TestAuthorizationProvider.#TestComplexAuthorization()" ) ]
[ assembly:
        SuppressMessage( "Microsoft.Design", "CA1024:UsePropertiesWhereAppropriate", Scope = "member",
                Target = "Afcas.Utils.DBHelper.#GetRequiredTransactionScope()" ) ]
[ assembly:
        SuppressMessage( "Microsoft.Performance", "CA1822:MarkMembersAsStatic", Scope = "member",
                Target = "Afcas.Test.TestReflection.#TestAccessors()" ) ]
[ assembly:
        SuppressMessage( "Microsoft.Performance", "CA1822:MarkMembersAsStatic", Scope = "member",
                Target = "Afcas.Test.TestAuthorizationProvider.#TestSimpleHierarchy()" ) ]
[ assembly:
        SuppressMessage( "Microsoft.Performance", "CA1822:MarkMembersAsStatic", Scope = "member",
                Target = "Afcas.Test.TestAuthorizationProvider.#TestSimpleAuthorization()" ) ]
[ assembly:
        SuppressMessage( "Microsoft.Performance", "CA1822:MarkMembersAsStatic", Scope = "member",
                Target = "Afcas.Test.TestAuthorizationProvider.#TestComplexHierarchy()" ) ]
[ assembly: SuppressMessage( "Microsoft.Naming", "CA1724:TypeNamesShouldNotMatchNamespaces", Scope = "type", Target = "Afcas.Afcas" ) ]
[ assembly: SuppressMessage( "Microsoft.Design", "CA1020:AvoidNamespacesWithFewTypes", Scope = "namespace", Target = "Afcas.Utils" ) ]
[ assembly: SuppressMessage( "Microsoft.Design", "CA1020:AvoidNamespacesWithFewTypes", Scope = "namespace", Target = "Afcas" ) ]
[ assembly:
        SuppressMessage( "Microsoft.Naming", "CA1724:TypeNamesShouldNotMatchNamespaces", Scope = "type",
                Target = "Afcas.Objects.Principal" ) ]
[ assembly:
        SuppressMessage( "Microsoft.Security", "CA2122:DoNotIndirectlyExposeMethodsWithLinkDemands", Scope = "member",
                Target = "Afcas.Test.SampleResourceHandleFactory.#GenerateResourceHandle(System.Object)" ) ]
[ assembly:
        SuppressMessage( "Microsoft.Security", "CA2122:DoNotIndirectlyExposeMethodsWithLinkDemands", Scope = "member",
                Target = "Afcas.Test.SampleResourceHandleFactory.#GenerateResourceHandleByKey(System.String)" ) ]
[ assembly:
        SuppressMessage( "Microsoft.Security", "CA2122:DoNotIndirectlyExposeMethodsWithLinkDemands", Scope = "member",
                Target = "Afcas.Test.TestAuthorizationManager.#CreateTestOperations(Afcas.Base.ObjectCache)" ) ]
[ assembly:
        SuppressMessage( "Microsoft.Security", "CA2122:DoNotIndirectlyExposeMethodsWithLinkDemands", Scope = "member",
                Target = "Afcas.Test.TestAuthorizationManager.#CreateTestPrincipals(Afcas.Base.ObjectCache)" ) ]
[ assembly:
        SuppressMessage( "Microsoft.Security", "CA2122:DoNotIndirectlyExposeMethodsWithLinkDemands", Scope = "member",
                Target = "Afcas.Test.TestAuthorizationManager.#TestDeleteOperation()" ) ]
[ assembly:
        SuppressMessage( "Microsoft.Security", "CA2122:DoNotIndirectlyExposeMethodsWithLinkDemands", Scope = "member",
                Target = "Afcas.Test.TestAuthorizationProvider.#SetupComplexData()" ) ]
[ assembly:
        SuppressMessage( "Microsoft.Security", "CA2122:DoNotIndirectlyExposeMethodsWithLinkDemands", Scope = "member",
                Target = "Afcas.Test.TestAuthorizationProvider.#SetupSimpleData()" ) ]
[ assembly:
        SuppressMessage( "Microsoft.Security", "CA2122:DoNotIndirectlyExposeMethodsWithLinkDemands", Scope = "member",
                Target = "Afcas.Test.TestAuthorizationProvider.#TestSimpleAuthorization()" ) ]
[ assembly:
        SuppressMessage( "Microsoft.Security", "CA2122:DoNotIndirectlyExposeMethodsWithLinkDemands", Scope = "member",
                Target = "Afcas.Test.TestAuthorizationProvider.#TestSimpleHierarchy()" ) ]
[ assembly:
        SuppressMessage( "Microsoft.Security", "CA2122:DoNotIndirectlyExposeMethodsWithLinkDemands", Scope = "member",
                Target = "Afcas.Test.TestReflection.#TestAccessors()" ) ]
[ assembly:
        SuppressMessage( "Microsoft.Security", "CA2122:DoNotIndirectlyExposeMethodsWithLinkDemands", Scope = "member",
                Target = "Afcas.Test.TestAuthorizationProvider.#TestOffline()" ) ]
[ assembly:
        SuppressMessage( "Microsoft.Performance", "CA1822:MarkMembersAsStatic", Scope = "member",
                Target = "Afcas.Test.TestAuthorizationProvider.#TestOffline()" ) ]