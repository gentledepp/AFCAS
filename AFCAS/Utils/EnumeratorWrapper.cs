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

namespace Afcas.Utils {
    using System;
    using System.Collections;
    using System.Collections.Generic;

    public sealed class EnumeratorWrapper< T >: IEnumerator< T > {
        private readonly IEnumerator _Enumerator;

        public EnumeratorWrapper( IEnumerator enumerator ) {
            _Enumerator = enumerator;
        }

        #region IEnumerator<T> Members

        T IEnumerator< T >.Current {
            get {
                return ( T )_Enumerator.Current;
            }
        }

        public void Dispose( ) {
            IDisposable endisp = _Enumerator as IDisposable;
            if( endisp != null ) {
                endisp.Dispose( );
            }
            GC.SuppressFinalize( this );
        }

        object IEnumerator.Current {
            get {
                return _Enumerator.Current;
            }
        }

        bool IEnumerator.MoveNext( ) {
            return _Enumerator.MoveNext( );
        }

        void IEnumerator.Reset( ) {
            _Enumerator.Reset( );
        }

        #endregion
    }
}