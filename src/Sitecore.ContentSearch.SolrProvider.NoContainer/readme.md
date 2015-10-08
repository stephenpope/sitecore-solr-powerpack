#NoContainer

Currently Sitecore's Solr provider requires you chose an IOC container.

This is mainly to support the `SolrNet` library that handles all communication with the `Solr` server.

When I originally wrote this the idea was that you can use DI to swap out any parts of `SolrNet` you wanted to modify.

The reality is that the IOC/DI sometimes adds unwanted confusion or complexity to an initial set up. For these cases I wanted to show how you dont actually need an IOC to get Sitecore running with `Solr`.

This class provides a basic wire-up of the default implementation of the services and allows to to swap some in a limited fashion.

##Setup

1. Build !
2. Drop the `Sitecore.ContentSearch.SolrProvider.NoContainer.dll` into your `/bin` directory.

##Configuration

Inside your `Global.asax.cs` you now startup Sitecore with:

	public class DefaultApplication : Sitecore.Web.Application
	{
		public virtual void Application_Start()
		{
			var startup = new Sitecore.ContentSearch.SolrProvider.NoContainer.DefaultSolrStartUp();
            startup.Initialize();
		}
	}
	
*NB - I didn't include this as a class because I'd need to take a deopendency on `Sitecore.Kernel` just for this one class and it seemed a wasteful dependency. *

##Caveats

This has only been lightly testsed, there are probably a lot of checks and other things missing, there are no tests for this yet and is supplied completely `as-is` unless this is something people would like to develop more.

Currently ive not encountered any erros but that doesnt mean there arent any, please feel free to fork and push back changes or fix bugs etc.

##Thoughts

This class supports swapping out most of the components of `SolrNet` but it is limited in scope. If you need to go crazy and make some very custom changes then you would fall back to one of the supported IOC containers such as `CastleWindsor`.


 