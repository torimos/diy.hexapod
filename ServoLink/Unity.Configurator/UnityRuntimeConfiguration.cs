using System;
using Microsoft.Practices.Unity;
using ServoLink;
using ServoLink.Contracts;

namespace Unity.Configurator
{
    public class UnityRuntimeConfiguration
    {
        public IUnityContainer SetupContainer()
        {
            IUnityContainer container = CreateContainer();
            Configure(container);
            return container;
        }

        private IUnityContainer CreateContainer()
        {
            bool adapterAlreadyExist;

            try
            {
                // due to fact that ServiceLocator doesn't has API to check that adapter is already registered we have to rely on null reference exception
                adapterAlreadyExist = Microsoft.Practices.ServiceLocation.ServiceLocator.Current != null;
            }
            catch (NullReferenceException)
            {
                adapterAlreadyExist = false;
            }
            catch (InvalidOperationException)
            {
                adapterAlreadyExist = false;
            }

            if (adapterAlreadyExist)
            {
                const string message = "Unity container instance already exist in current application domain. Maybe you are trying to register dependencies twice?";
                throw new InvalidOperationException(message);
            }

            var container = new UnityContainer();

            // set up service locator unity adapter
            Microsoft.Practices.ServiceLocation.ServiceLocator.SetLocatorProvider(() => new UnityServiceLocatorAdapter(container));

            return container;
        }


        public virtual void Configure(IUnityContainer container)
        {
            container.RegisterType<IPort, SerialPort>();
            container.RegisterType<IServoController, ServoController>();
        }
    }
}
