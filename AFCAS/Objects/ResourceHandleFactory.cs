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
    public abstract class ResourceHandleFactory {
        private readonly string _Type;

        protected ResourceHandleFactory( string resourceType ) {
            _Type = resourceType;
        }

        public string ResourceType {
            get {
                return _Type;
            }
        }

        /// <summary>
        /// This method is used by AFCAS to return resources. 
        /// Implementors must be able to associate an actual resource with the Key property
        /// </summary>
        /// <param name="resourceId"></param>
        /// <returns></returns>
        protected internal abstract ResourceHandle GenerateResourceHandleByKey( string resourceId );

        public abstract ResourceHandle GenerateResourceHandle( object resource );
    }
}