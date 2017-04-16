using System;
using FluentAssertions;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Ploeh.AutoFixture;
using ServoLink;

namespace ServoLinkTests
{
    [TestClass]
    public class BinaryHelperTests
    {
        private Fixture _fixture;
        private BinaryHelper _sut;

        [TestInitialize]
        public void InitTest()
        {
            _fixture = new Fixture();
            _sut = new BinaryHelper();
        }

        [TestMethod]
        public void ConvertToByteArray_should_return_binary_of_text()
        {
            byte[] result = _sut.ConvertToByteArray((byte) 8, "abcd","1234");

            result.ShouldBeEquivalentTo(new[] { 8, 'a', 'b', 'c', 'd', '1', '2', '3', '4' });
        }

        [TestMethod]
        public void ConvertToByteArray_should_return_binary_of_single_and_array_16bit_values()
        {
            byte[] result = _sut.ConvertToByteArray((ushort) 3200, new short[] {1, 2, 3, 4});

            result.ShouldBeEquivalentTo(new byte[] {128, 12, 1, 0, 2, 0, 3, 0, 4, 0});
        }

        [TestMethod]
        public void ConvertToByteArray_should_return_binary_of_single_and_array_32bit_values()
        {
            byte[] result = _sut.ConvertToByteArray((uint)3200, new [] { 1, 2, 3, 4 });

            result.ShouldBeEquivalentTo(new byte[] { 128, 12, 0, 0, 1, 0, 0, 0, 2, 0, 0, 0, 3, 0, 0, 0, 4, 0, 0, 0 });
        }

        [TestMethod]
        public void ConvertToByteArray_should_return_binary_of_single_and_array_64bit_values()
        {
            byte[] result = _sut.ConvertToByteArray(new long[] { 1, 2, 3, 4 }, (ulong)3200);

            result.ShouldBeEquivalentTo(new byte[] { 1, 0, 0, 0, 0, 0, 0, 0, 2, 0, 0, 0, 0, 0, 0, 0, 3, 0, 0, 0, 0, 0, 0, 0, 4, 0, 0, 0, 0, 0, 0, 0, 128, 12, 0, 0, 0, 0, 0, 0});
        }
    }
}
