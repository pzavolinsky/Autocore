using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Autocore.Test.Integration
{
    public interface IGenericInterface<T> : ISingletonDependency { }
    public class NonGenericClass : IGenericInterface<bool> { }
}
