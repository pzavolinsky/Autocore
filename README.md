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

Grab the **Autocore** package from NuGet by typing this in the Visual Studio package manager powershell console:

`PM> install-package Autocore`

or by running:

```Shell
cd your-project-directory # the one with the .csproj
mono NuGet.exe install Autocore
```

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

Why should I use this?
----------------------

In my experience, designing applications to enforce inversion of control through dependency injection has many practical advantages such as promoting modularity, loose-coupling, maintainability and testability. These days, building an enterprise app without a dependency injection container feels like leaving my house without my pants on.

If you are unsure whether dependency injection is for you, you should google around a bit and then make an informed decision. From this point on, I'll assume you have decided to use DI in your project.

The question remains, why should I use Autocore when my DI container works perfectly fine?

While I think DI containers are a *must* in middle/big applications, their configuration can become a burden to the point of discouraging modular decomposition. I'm talking about the bootstrapping and container configuration code. This code (sometimes expressed as XML configuration) is usually a never-ending list of service-to-component (i.e. interface-to-implementation) mappings, such as:

```C#
using Autofac;

var builder = new ContainerBuilder();

builder.RegisterType<MyComponent>().AsSelf();
builder.RegisterType<MyOtherComponent>().As<ISomeService>().As<ISomeOtherService>();
builder.RegisterGeneric(typeof(MyGenericComponent<>)).As(typeof(IMyGenericService<>));
// ... this list can go on forever

var container = builder.Build();
```

The list can indeed go on forever, but that (annoying as it is to maintain) is not the main issue. Every time you create a new service/component you have to remember to go to the bootstrapping code (or XML config) and register it for DI, otherwise you'll get obscure runtime errors about classes lacking default constructors and injectors failing.

Initially this might not look like a real problem but, in my experience, it creates a psychological resistance against building new components and separating concerns. Even if that is not the case, the need for additional component/service registration forces you to divert your attention form the task at hand in order to focus on the non-functional task of registering your component in a code that, sooner or later, starts looking like spaghetti.

Autocore helps you solve this problem by eliminating bootstrapping. Using marker interfaces and conventions you get all your services and components registered with a single line of code:

```C#
var container = Autocore.Factory.Create()
```

Granted, there are some complex scenarios where manual wiring of service-to-component mappings might be the only way to go. But even in edge cases you can still leverage Autocore by using automatic registration for the simple mappings and manually registering the exceptions (see [The Factory façade](#the-factory-fa%C3%A7ade) for more details).

If you are considering adding Autocore to an existing project, see [Autocore for existing projects](#autocore-for-existing-projects) for some ideas on where to start.

Integrations
------------

Autocore provides pre-configured integrations for ASP.NET MVC and ASP.NET WebAPI. These integrations are provided as separate packages.

#### MVC Integration
Install with

`PM> install-package Autocore.Mvc`

or

```Shell
cd your-project-directory # the one with the .csproj
mono NuGet.exe install Autocore.Mvc
```

Open ```Global.asax.cs``` and add the following line to the ```Application_Start()``` method:

```C#
public class MvcApplication : System.Web.HttpApplication
{
  protected void Application_Start()
  {
    Autocore.Mvc.Factory.Create(typeof(MvcApplication).Assembly);

    // the rest of your Application_Start() method follows ...
  }
}
```

After this setup you can start injecting dependencies to your regular MVC controllers. Check [Samples/Autocore.Samples.Mvc5](https://github.com/pzavolinsky/Autocore/tree/master/Samples/Autocore.Samples.Mvc5) for a complete example.

#### WebAPI Integration
Install with

`PM> install-package Autocore.WebApi`

or

```Shell
cd your-project-directory # the one with the .csproj
mono NuGet.exe install Autocore.WebApi
```

Open ```App_Start/WebApiConfig.cs``` and add the *using* and *RegisterAutocore* lines below:

```C#
using Autocore.WebApi; // <-- don't forget this!

public static class WebApiConfig
{
  public static void Register(HttpConfiguration config)
  {
    config.RegisterAutocore(typeof(WebApiConfig).Assembly);

    // the rest of your Register() method follows ...
  }
}
```

After this setup you can start injecting dependencies to your regular Api controllers. Check [Samples/Autocore.Samples.WebApi](https://github.com/pzavolinsky/Autocore/tree/master/Samples/Autocore.Samples.WebApi) for a complete example.

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
  IVolatile<IMyPerRequestContext> _ctx;
  public MySingletonService(IVolatile<IMyPerRequestContext> ctx)
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

* Any call to ```IVolatile<T>.Value``` outside a volatile scope fails with a runtime exception. For example:

  ```c#
  class HackishService : ISomeSingletonService
  {
    string _userAgent;
    public HackishService(IVolatile<IMyPerRequestContext> ctx)
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
	var op = container.Resolve<IOperation>(); // ctor cannot call IVolatile<T>.Value
	
	container.ExecuteInVolatileScope(scope => {
	  // volatile scope
		op.Work(); // Work() can call IVolatile<T>.Value
	});
}
```

Putting it all together:
```c#
public interface IMyVolatile : IVolatileDependency { void DoVolatile(); }

public interface IMySingleton : ISingletonDependency { void Work(); }

public class MySingleton : IMySingleton
{
  IVolatile<IMyVolatile> _v;
  public MySingleton(IVolatile<IMyVolatile> v) { _v = v; }
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
  public HackishSingleton(IVolatile<IMyVolatile> v) { _v = v.Value; } // throws
  public void Work() { _v.DoVolatile(); }
}

using (var container = Autocore.Factory.Create())
{
	var op = container.Resolve<IMySingleton>(); // ctor cannot call IVolatile<T>.Value
	var v  = container.Resolve<IMyVolatile>();  // compile-time error
	
	container.ExecuteInVolatileScope(scope => {
	  // volatile scope
  	var v2 = scope.Resolve<IMyVolatile>(); // OK!
		op.Work(); // Work() can call IVolatile<T>.Value
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
using Autocore.Implementation; // pull in container builder extensions
// ...

var assemblies = new Assembly[] { ... }; // AppDomain.CurrentDomain.GetAssemblies();

var builder = new ContainerBuilder();

// do advanced Autofac builder configuration

builder.RegisterDependencyAssemblies(assemblies); // Autocore extension
var container = new Autocore.Implementation.Container(builder.Build());
```
Constructs an ```Autocore.IContainer``` from a custom ```Autofac.ContainerBuilder``` and registers dependencies in the specified assemblies.

##### Create from a specific list of types
```C#
using Autocore.Implementation; // pull in container builder extensions
// ...

var types = new Type[] { ... };

var builder = new ContainerBuilder();

// do advanced Autofac builder configuration

builder.RegisterDependencyTypes(types); // Autocore extension
var container = new Autocore.Implementation.Container(builder.Build());
```
Constructs an ```Autocore.IContainer``` from a custom ```Autofac.ContainerBuilder``` and registers the specified dependency types.

Autocore for existing projects
------------------------------

If you are trying to integrate Autocore to an existing project, this section might give you some ideas on how to approach the integration.

Whenever possible, using a pre-configured Autocore integration module for [ASP.NET MVC](#mvc-integration) or [WebAPI](#webapi-integration) will save you some headache.

If no suitable pre-configured module exists, your first task is to choose an integration strategy:

* Top-down integration: start from your project's highest abstraction (e.g. the entry-point class) and work your way down to its component classes.
* Bottom-up integration: start from small isolated/decoupled components and work your way up to its client components.
* Hybrid integration: do top-down and bottom-up at the same time.

##### Top-down integration

You can start this integration by wrapping your project entry-point classes behind an ```ISingletonDependency``` and then resolve them using an Autocore factory.

For example:

```C#
// before:
public static class MyEntryPointClass
{
  public static void DoStuff()
  {
    new SomeClass().DoSomeStuff();
    new OtherClass().DoMoreStuff();
  }
}
MyEntryPointClass.DoStuff();

// after:
public interface IEntryPoint : ISingletonDependency { void DoStuff();     }
public interface ISomeClass  : ISingletonDependency { void DoSomeStuff(); }
public class MyEntryPointClass : IEntryPoint
{
  ISomeClass  _some;
  public MyEntryPointClass(ISomeClass some)
  {
    _some  = some;
  }
  public void DoStuff()
  {
    _some.DoSomeStuff();
    new OtherClass().DoMoreStuff(); // still pending integration
  }
}

using (var container = Autocore.Factory.Create())
{
  container.Resolve<IEntryPoint>().DoStuff();
}
```

After the entry-point class is resolving dependencies, you can move to its constituent components (e.g. ```SomeClass```, ```OtherClass```, etc.).

Two important things to notice are:

* The starting point of a per-operation (e.g. per-request) call.
* The ending point of a per-operation (e.g. per-request) call.

If both points are inside the same method, consider wrapping the per-operation call in a ```container.ExecuteInVolatileScope``` call:

```C#
class SomeClass : ISomeInterface // : where ISomeInterface : ISingletonDependency
{
  IContainer _container;
  public SomeClass(IContainer container)
  {
    _container = container;
  }
  public void HandleRequest(RequestContext ctx)
  {
    _container.ExecuteInVolatileScope((scope) => {
      // per-request code goes here
      // use scope.Resolve<> to set/access per-request volatiles
      // ...
    });
  }
}
```

Otherwise, consider creating a volatile scope when the per-operation call begins and dispose it when it ends.

##### Bottom-up integration

You can start by identifying a more-or-less isolated portion of your project (e.g. a third-party service client, a small rule or validation engine, etc.) and migrating that whole sub-system to an injected dependency tree.

Update your project's entry-point to create and cache the *root* container.

Finally, update the client code for your sub-system to resolve the sub-system through the cached *root* container.

For example:
```C#
// before:
public interface IEmailValdation { bool IsValid(string email); }
public class EmailValidation : IEmailValdation
{
  IRegexEmailValidation _regex;
  IConfirmationEmailValidation _confirmation;

  public EmailValidation()
  {
    _regex = new RegexEmailValidation();
    _confirmation = new ConfirmationEmailValidation();
  }

  public bool IsValid(string email)
  {
    return _regex.Validate(email) && _confirmation.Validate(email);
  }
}

public class PersonValidation
{
  IEmailValdation _emailValidation;
  public PersonValidation()
  {
    _emailValidation = new EmailValidation();
  }
  // ...
}

// after
public interface IEmailValdation : ISingletonDependency { bool IsValid(string email); }
public class EmailValidation : IEmailValdation
{
  IRegexEmailValidation _regex;
  IConfirmationEmailValidation _confirmation;

  public EmailValidation(IRegexEmailValidation regex, IConfirmationEmailValidation confirmation)
  {
    _regex = regex;
    _confirmation = confirmation;
  }

  public bool IsValid(string email)
  {
    return _regex.Validate(email) && _confirmation.Validate(email);
  }
}

public class PersonValidation
{
  IEmailValdation _emailValidation;
  public PersonValidation()
  {
    _emailValidation = MyEntryPointClass.Container.Resolve<IEmailValdation>();
  }
  // ...
}

public static class MyEntryPointClass
{
  public static void DoStuff()
  {
    Container = Autocore.Factory.Create();
    new SomeClass().DoSomeStuff(); // this call will eventually create a PersonValidation
    // ...
  }

  // root container cache
  public static Autocore.IContainer Container { get; private set; }
}
```

Troubleshooting
---------------

#### VolatileResolutionException: Attempted to access a volatile dependency outside a volatile scope
If you are getting this error you are either hacking or leaking a volatile dependency.

###### Hacking a volatile dependency

This scenario happens when a singleton class takes a dependency on a ```IVolatile<T>``` but tries to
get the actual volatile value in the singleton's constructor. This is wrong because the lifetime
scope of the singleton is longer than the scope of the volatile value (i.e. ```IVolatileDependency```)
and sooner or later the singleton will be holding a disposed reference of the volatile.

For example:

```C#
  public interface IVolatileService : IVolatileDependency {}
  public class VolatileService : IVolatileService {}
  public class TriesToHackAVolatile : ISingletonDependency
  {
    private IVolatileService _svc; // wrong!
    public TriesToHackAVolatile(IVolatile<IVolatileService> svc)
    {
      _svc = svc.Value; // throws
    }
  }
  ...
  var container = Autocore.Factory.Create();
  container.Resolve<TriesToHackAVolatile>(); // throws
```

To fix this error, simply store the ```IVolatile<T>``` in a field and call its ```Value``` property
whenever you need to access the actual volatile value:

```C#
  public interface IVolatileService : IVolatileDependency {}
  public class VolatileService : IVolatileService {}
  public class TriesToHackAVolatile : ISingletonDependency
  {
    private IVolatile<IVolatileService> _svc; // wrong!
    public TriesToHackAVolatile(IVolatile<IVolatileService> svc)
    {
      _svc = svc;
    }
    public void MethodCalledInsideAVolatileScope()
    {
      // you can safely use _svc.Value here
    }
  }
```

###### Leaking a volatile dependency

This scenario happens when methods of a singleton class return delegates that defer access to a volatile
value. These delegates are effectively leaking the volatile outside its scope and, when called outside a volatile scope, throw the exception.

For example:

```C#
  public interface IVolatileService : IVolatileDependency
  {
    string Get(int num);
  }
  public class VolatileService : IVolatileService
  {
    public string Get(int num) { return num.ToString(); }
  }
  public class LeaksVolatile : ISingletonDependency
  {
    private IVolatile<IVolatileService> _svc;
    public LeaksVolatile(IVolatile<IVolatileService> svc) { _svc = svc; }
    public IEnumerable<string> Access()
    {
      // The lambda expression in Select is evaluated after
      // the method returns (and potentially, after the
      // volatile scope was disposed).
      return (new[] { 1 }).Select(i => _svc.Value.Get(i));
    }
    public Func<string> Delayed(int i)
    {
      return () => _svc.Value.Get(i); // wrong, leaks _svc.Value
    }
  }
  ...
  var container = Autocore.Factory.Create();
  var client = container.Resolve<LeaksVolatile>();
  
  var list = container.ExecuteInVolatileScope((scope) => client.Access());
  list.ToArray(); // throws (evaluates lambda outside the volatile scope)
  
  var callback = container.ExecuteInVolatileScope((scope) => client.Delayed(10));
  callback(); // throws (evaluates lambda outside the volatile scope)
```

To fix this error, if the leak was triggered by a Linq expression force the expression evaluation by calling ```ToArray``` or ```ToList``` before returning from the method. If the leak was triggered by an explicit ```Action``` or ```Func``` returned by the method, make sure you call ```IVolatile<T>.Value``` outside the returned delegate.

```C#
  public class LeaksVolatile : ISingletonDependency
  {
    private IVolatile<IVolatileService> _svc;
    public LeaksVolatile(IVolatile<IVolatileService> svc) { _svc = svc; }
    public IEnumerable<string> Access()
    {
      return (new[] { 1 }).Select(i => _svc.Value.Get(i))
        .ToArray(); // force expression evaluation here!
    }
    public Func<string>Delayed(int i)
    {
      var s = _svc.Value.Get(i);
      return () => s;
    }
  }
```

#### DependencyResolutionException: No scope with a Tag matching '__autocore_volatile__' is visible from the scope in which the instance was requested

If you are getting this error you are breaking dependency scopes.

###### Breaking dependency scopes

This scenario happens when a singleton class takes a direct dependency on a volatile.

For example:

```C#
  public interface IVolatileService : IVolatileDependency {}
  public class VolatileService : IVolatileService {}
  public class BreaksVolatile : ISingletonDependency
  {
    public BreaksVolatile(IVolatileService svc) // throws
    {
    }
  }
  ...
  var container = Autocore.Factory.Create();
  container.Resolve<BreaksVolatile>(); // throws
```

To fix this error, simply wrap the dependency with ```IVolatile<T>``` and call ```IVolatile<T>.Value``` in the singleton methods that need to access the volatile. Keep in mind that you SHOULD NOT call ```Value``` directly in the constructor since that would be [hacking a volatile dependency](#hacking-a-volatile-dependency) and will not work.

```C#
  public class BreaksVolatile : ISingletonDependency
  {
    private IVolatile<IVolatileService> _svc;
    public BreaksVolatile(IVolatile<IVolatileService> svc) { _svc = svc; }
    public void DoSomething()
    {
      // you can safely use _svc.Value here
    }
  }
```

Mono compatibility
------------------

Autocore should work fine in Mono projects but there are two considerations you should keep in mind:
- Mono does not support ASP.NET WebAPI so the [WebApi integration](#webapi-integration) won't work in Mono.
- Mono 3.10 has a bug in the implementation of ```CallContext.LogicalGetData``` when executing ```async``` code. Until the fix for bug [#24757](https://bugzilla.xamarin.com/show_bug.cgi?id=24757) makes it into a stable release, Autocore will not work for async projects. A workaround for this issue (the one I'm using for Autocore development) is building Mono from source (e.g. ```git clone https://github.com/mono/mono.git```).
