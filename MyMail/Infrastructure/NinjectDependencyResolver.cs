using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Mvc;
using MyMail.Models.DBmanager;
using Ninject;

namespace MyMail.Infrastructure
{
    public class NinjectDependencyResolver:IDependencyResolver
    {
        private IKernel _kernel;

        public NinjectDependencyResolver(IKernel kernel_param)
        {
            _kernel = kernel_param;

            _bind();
        }

        public object GetService(Type serviceType)
        {
            return _kernel.TryGet(serviceType);
        }

        public IEnumerable<object> GetServices(Type serviceType)
        {
            return _kernel.GetAll(serviceType);
        }

        private void _bind()
        {
            _kernel.Bind<IDBprovider>().To<DBprovider>();
        }
    }
}