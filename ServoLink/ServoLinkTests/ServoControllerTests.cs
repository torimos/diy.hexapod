using FluentAssertions;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using NSubstitute;
using Ploeh.AutoFixture;
using ServoLink;
using ServoLink.Contracts;

namespace ServoLinkTests
{
    [TestClass]
    public class ServoControllerTests : TestBase
    {
        private Fixture _fixture;
        private IServoController _sut;

        [TestInitialize]
        public void InitTest()
        {
            _fixture = new Fixture();
            _sut = new ServoController(32, new BinaryHelper());
        }

        [TestMethod]
        public void Connect_should_not_open_port_when_it_was_opened()
        {
            var port = Substitute.For<IPort>();
            port.IsOpen.Returns(true);

            bool result = _sut.Connect(port);

            port.DidNotReceive().Open();
            result.Should().BeTrue();
        }

        [TestMethod]
        public void Connect_should_open_port_when_it_is_closed_or_not_opened()
        {
            var port = Substitute.For<IPort>();
            port.IsOpen.Returns(false);
            port.Open().Returns(true);

            bool result = _sut.Connect(port);

            port.Received().Open();
            result.Should().BeTrue();
        }

        [TestMethod]
        public void Disconnect_should_close_port_when_port_was_opened()
        {
            var port = Substitute.For<IPort>();
            port.IsOpen.Returns(true);
            
            _sut.Connect(port);
            _sut.Disconnect();

            port.Received().Close();
        }

        [TestMethod]
        public void Disconnect_should_not_close_port_when_port_is_closed_or_not_opened()
        {
            var port = Substitute.For<IPort>();
            port.IsOpen.Returns(false);

            _sut.Disconnect();

            port.DidNotReceive().Close();
        }
    }
}
