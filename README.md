Autocore
========

Autocore is a pre-configured application core designed for building declarative modular applications.

Autocore is built on top of Autofac providing automatic dependency configuration through marker interfaces.

Some of Autocore's features include:
- Zero configuration dependency injection (no XML config file, no custom initializer code to register types, etc.)
- Declarative component registration by implementing marker interfaces
- Scoped dependencies including Singleton (shared instance), Instance (always create a new instance), Volatile (per-scope instance, e.g. per-request).

Installation
------------

TODO

Hello World
-----------

Below is a simple console application with an injectable service.

```C#
using System;
using Autocore;

namespace HelloWorld
{
	public interface IGreeting : ISingletonDependency
	{
		string Message { get; }
	}

	public class Greeting : IGreeting
	{
		public string Message
		{
			get { return "Hello World!"; }
		}
	}

	public static class Program
	{
		public static void Main(string[] args)
		{
			using (var container = Autocore.Factory.Create())
			{
				var greeting = container.Resolve<IGreeting>();
				System.Console.WriteLine(greeting.Message);
			}
		}
	}
}
```

Basic concepts
--------------

Autocore hides most of the complexity required to setup an IoC container for dependency injection. This section introduces the most important concepts of Autocore and how to take advantage of its declarative design.

**Disclaimer**: throughout this text I refer to *service* in the sense of a contract that a component implements. In other words, you should read *service* as a synonym of *interface* and avoid the confusion with the other type of services (e.g. REST, SOAP, WCF, etc.).

#### Interfaces
The magic behind Autocore's automatic component registration are three marker interfaces, namely:
* ```ISingletonDependency``` are services with a single shared instance per container. Calls on the same container to ```Resove<T>``` with ```T : ISingletonDependency``` will always return the same instance.
* ```IInstanceDependency``` are services that must be instantiated every time a service instance is needed. These services are never shared and calls to ```Resove<T>``` with ```T : IInstanceDependency``` will always return a new instance of the service.
* ```IVolatileDependency``` are special services that behave like singletons inside a volatile scope. For more information on volatiles see the [Volatiles](#volatiles) section below. Typical examples of volatile services include per-request state wrappers such as ```HttpContext```, services that use ```ThreadLocal``` storage, ```CallContext``` etc.

Note that by using these interfaces you are not only marking your services for dependency injection, but you are also explicitly stating the scope of those services.

#### Bootstrapping
Autocore wraps Autofac IoC containers behind the minimal ```IContainer``` interface. Containers represent nested scopes of component resolution. The initial (usually static) container is the *root* container. This container is created through the ```Autocore.Factory.Create``` method:

```C#
var container = Autocore.Factory.Create();
```
or
```C#
using (var container = Autocore.Factory.Create())
{
  // your code goes here: container.Resolve<ISomething>().CallSomeMethod();
}
```

In simple applications you could create the *root* container inside a ```using``` to ensure that the container is disposed after running your code (as in the [Hello World](#hello-world) example above). More complex applications might cache the *root* container and use it to construct nested containers.

Advanced concepts
-----------------

This section introduces some advanced concepts of Autocore such as *volatiles* and how to hack the implementation API without using the ```Factory``` façade.

#### Volatiles
When building services you usually have to choose the lifetime scope of those services. Should this service be a singleton? Can it be shared by all threads, calls, requests, etc.?

Even if you don't care about the scope of your services, you are making an implicit choice (i.e. create a new instance of the service every time).

Ideally this decision should be pretty straight forward: the service should be singleton unless it uses per-call information. 

But things tend to get a lot messier when you start using implicit contextual state that looks like static but is in fact per-call. Typical examples include ```HttpContext.Current```, anything that uses ```[ThreadStatic]```, ```CallContext.LogicalGetData```, etc.

But what about this (broken) snippet:
```C#
// Single instance shared by every request
class MySingletonService : ISomeService
{
  public MySingletonService()
  {
    UserAgent = HttpContext.Current.Request.UserAgent;
  }
  public string UserAgent { get; protected set; }
}
```
As you can guess, every request that accesses ```mySingletonService.UserAgent``` will see the user-agent of the request that triggered the construction of service instance. Of course, you could fix *this* code by moving the ```HttpContext.Current.Request.UserAgent``` to the property getter, but what prevents new occurrences of the issue in other services?

Volatiles are an abstraction that let you make explicit the implicit contextual state. In a sense, volatiles can be thought as per-call singletons. That is:
* Within a volatile (per-call) scope, every call to ```Resove<T>``` with ```T : IVolatileDependency``` will return the same instance.
* Calls to ```Resove<T>``` with ```T : IVolatileDependency``` on two distinct volatile scope will return different instances.
* Calls to ```Resove<T>``` with ```T : IVolatileDependency``` outside a volatile scope fail.

The previous example, this time using Autocore's interfaces:
```C#
public interface IMyPerRequestContext : IVolatileDependency
{
  string UserAgent { get; }
}
public class MyPerRequestContext : IMyPerRequestContext
{
  public MyPerRequestContext()
  {
    UserAgent = HttpContext.Current.Request.UserAgent;
  }
  public string UserAgent { get; protected set; }
}

public interface ISomeSingletonService : ISingletonDependency { ... }
class MySingletonService : ISomeSingletonService
{
  Volatile<IMyPerRequestContext> _ctx;
  public MySingletonService(Volatile<IMyPerRequestContext> ctx)
  {
    _ctx = ctx;
  }
  public string UserAgent { get { return _ctx.Value.UserAgent; } }
}
```

Admittedly, this snippet is a bit more complex that the previous one, but the addition of the *volatile* ```IMyPerRequestContext``` has a number of advantages:
* It makes explicit that the ```MySingletonService``` requires some per-call (i.e. volatile) service.
* Any singleton service that takes a direct dependency on a volatile service will throw a runtime exception in its constructor. For example:

  ```c#
  class BrokenService : ISomeSingletonService
  {
    public BrokenService(IMyPerRequestContext ctx) // Runtime exception
    {
    }
  }
  ```

* Any call to ```Volatile<T>.Value``` outside a volatile scope fails with a runtime exception. For example:

  ```c#
  class HackishService : ISomeSingletonService
  {
    string _userAgent;
    public HackishService(Volatile<IMyPerRequestContext> ctx)
    {
      _userAgent = ctx.Value.UserAgent // Runtime exception (outside a volatile scope)
    }
  }
  // ...
  // global scope
  var svc = container.Resolve<ISomeSingletonService>(); // throws!
  container.ExecuteInVolatileScope((scope) => {
    // volatile scope
    svc.DoSomething();
  });
  
  ```

So far we've seen that volatiles will help you avoid scope mismatch issues in your services. To accomplish this, *volatiles* can only be resolved inside a volatile scope. To execute code inside a volatile scope you could use:

```c#
using (var container = Autocore.Factory.Create())
{
  // root scope
	var op = container.Resolve<IOperation>(); // ctor cannot call Volatile<T>.Value
	
	container.ExecuteInVolatileScope(scope => {
	  // volatile scope
		op.Work(); // Work() can call Volatile<T>.Value
	});
}
```

Putting it all together:
```c#
public interface IMyVolatile : IVolatileDependency { void DoVolatile(); }

public interface IMySingleton : ISingletonDependency { void Work(); }

public class MySingleton : IMySingleton
{
  Volatile<IMyVolatile> _v;
  public MySingleton(Volatile<IMyVolatile> v) { _v = v; }
  public void Work() { _v.Value.DoVolatile(); }
}

public class BrokenSingleton : IMySingleton
{
  IMyVolatile _v;
  public BrokenSingleton(IMyVolatile v) { _v = v; } // throws
  public void Work() { _v.DoVolatile(); }
}

public class HackishSingleton : IMySingleton
{
  IMyVolatile _v;
  public HackishSingleton(Volatile<IMyVolatile> v) { _v = v.Value; } // throws
  public void Work() { _v.DoVolatile(); }
}

using (var container = Autocore.Factory.Create())
{
	var op = container.Resolve<IMySingleton>(); // ctor cannot call Volatile<T>.Value
	var v  = container.Resolve<IMyVolatile>();  // compile-time error
	
	container.ExecuteInVolatileScope(scope => {
	  // volatile scope
  	var v  = scope.Resolve<IMyVolatile>(); // OK!
		op.Work(); // Work() can call Volatile<T>.Value
	});
}
```

#### The ```Factory``` façade
Autocore relies heavily on the façade pattern to provide simple abstractions to complex operations without completely hiding the low-level API. 

In other words, while the ```Factory``` class might provide useful helpers to quickly construct a container, in some cases you might need more flexibility and/or power. Autocore is designed to help you navigate through these scenarios without getting in the way.

This section shows some examples of façade unwinding to gradually provide more flexibility.

##### The happy path
```C#
var container = Autocore.Factory.Create();
```
Registers dependencies in every assembly loaded in the current AppDomain.

##### Create with an assembly filter
```C#
var container = Autocore.Factory.Create(asm => asm.FullName.StartsWith("MyApp"));
```
Registers dependencies in every assembly loaded in the current AppDomain for which the filter lambda returns true (i.e. whose full name starts with *MyApp*).
 
##### Create with a list of assemblies
```C#
var assemblies = new Assembly[] { typeof(MyApp).Assembly, typeof(MyPlugin).Assembly };
var container = Autocore.Factory.Create(assemblies);
```
Registers dependencies in specified assemblies.
 
##### Create from an ```Autofac.ContainerBuilder```
```C#
var assemblies = new Assembly[] { ... }; // AppDomain.CurrentDomain.GetAssemblies();

var builder = new ContainerBuilder();

// do advanced Autofac builder configuration

builder.RegisterDependencyAssemblies(assemblies); // Autocore extension
var container = new Autocore.Implementation.Container(builder.Build());
```
Constructs an ```Autocore.IContainer``` from a custom ```Autofac.ContainerBuilder``` and registers dependencies in the specified assemblies.

##### Create from a specific list of types
```C#
var types = new Type[] { ... };

var builder = new ContainerBuilder();

// do advanced Autofac builder configuration

builder.RegisterDependencyTypes(types); // Autocore extension
var container = new Autocore.Implementation.Container(builder.Build());
```
Constructs an ```Autocore.IContainer``` from a custom ```Autofac.ContainerBuilder``` and registers the specified dependency types.

