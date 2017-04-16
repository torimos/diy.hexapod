using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Microsoft.Practices.ServiceLocation;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using NSubstitute;
using Ploeh.AutoFixture;

namespace ServoLinkTests
{
    public class TestBase
    {
        [TestInitialize]
        public void InitTestBase()
        {
            ConfigureServiceLocator();
        }

        protected virtual void ConfigureServiceLocator()
        {
            ServiceLocator.SetLocatorProvider(() => Substitute.For<IServiceLocator>());
        }
    }
}
