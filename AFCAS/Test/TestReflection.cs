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
    using NUnit.Framework;
    using Utils;

    [ TestFixture ]
    public class TestReflection {
        [ Test ]
        public void TestAccessors( ) {
            SampleResource sample = SampleResource.GetOrCreateSampleResource( "1" );
            sample.StringTest = "123";
            sample.Int32Test = 123;

            ReflectionHelper.FastMemberGetter stringFieldGetter = ReflectionHelper.GetCachedFieldGetter( typeof( SampleResource ),
                                                                                                         "_StringTest" );
            ReflectionHelper.FastMemberSetter stringFieldSetter = ReflectionHelper.GetCachedFieldSetter( typeof( SampleResource ),
                                                                                                         "_StringTest" );
            ReflectionHelper.FastMemberGetter intFieldGetter = ReflectionHelper.GetCachedFieldGetter( typeof( SampleResource ),
                                                                                                      "_Int32Test" );
            ReflectionHelper.FastMemberSetter intFieldSetter = ReflectionHelper.GetCachedFieldSetter( typeof( SampleResource ),
                                                                                                      "_Int32Test" );

            Assert.That( stringFieldGetter.Invoke( sample ).Equals( sample.StringTest ) );
            Assert.That( intFieldGetter.Invoke( sample ).Equals( sample.Int32Test ) );

            stringFieldSetter.Invoke( sample, "456" );
            intFieldSetter.Invoke( sample, 456 );
            Assert.That( sample.StringTest == "456" );
            Assert.That( sample.Int32Test == 456 );

            ReflectionHelper.FastMemberGetter stringPropGetter = ReflectionHelper.GetCachedPropertyGetter( typeof( SampleResource ),
                                                                                                           "StringTest" );
            ReflectionHelper.FastMemberSetter stringPropSetter = ReflectionHelper.GetCachedPropertySetter( typeof( SampleResource ),
                                                                                                           "StringTest" );
            ReflectionHelper.FastMemberGetter intPropGetter = ReflectionHelper.GetCachedPropertyGetter( typeof( SampleResource ),
                                                                                                        "Int32Test" );
            ReflectionHelper.FastMemberSetter intPropSetter = ReflectionHelper.GetCachedPropertySetter( typeof( SampleResource ),
                                                                                                        "Int32Test" );


            Assert.That( stringPropGetter.Invoke( sample ).Equals( sample.StringTest ) );
            Assert.That( intPropGetter.Invoke( sample ).Equals( sample.Int32Test ) );
            stringPropSetter.Invoke( sample, "789" );
            intPropSetter.Invoke( sample, 789 );
            Assert.That( sample.StringTest == "789" );
            Assert.That( sample.Int32Test == 789 );

            ReflectionHelper.FastMemberGetter keyPropGetter = ReflectionHelper.GetCachedPropertyGetter( typeof( SampleResource ), "Key" );
            ReflectionHelper.FastMemberSetter keyFieldSetter = ReflectionHelper.GetCachedFieldSetter( typeof( SampleResource ), "_Key" );

            Assert.That( sample.Key.Equals( keyPropGetter.Invoke( sample ) ) );
            keyFieldSetter.Invoke( sample, "xxx" );
            Assert.That( sample.Key.Equals( "xxx" ) );
        }
    }
}

#endif