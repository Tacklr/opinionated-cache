opinionated-cache
=================

An opinionated cache

To start using this library, run the following command in the [Package Manager Console](http://docs.nuget.org/docs/start-here/using-the-package-manager-console)

    PM> Install-Package opinionated-cache

[![opinionated-cache MyGet Build Status](https://www.myget.org/BuildSource/Badge/opinionated-cache?identifier=7067f82c-0d7b-4440-aef6-4650fd8e9e04)](https://www.myget.org/)

Release notes:

1.1.2 Fixing Cache Clear of prefixed keys
 * When you do a Clear the key value was _supposed_ to allow passing a prefix string that would clear all entries that start with that prefix. It does this by appending a period (the previous default KeySeparator) to the key given and clearing anything that starts with that prefix. Now that the KeySeparator is settable, we have to ask the IBaseCacheKey for the separator to append so that we get the correct value (currently a forward slash).

1.1.1 Making it faster
 * Made the Get method use ```as T``` instead of a ```(T)``` cast so we don't throw when a cache key calls for the wrong kind of object in the cache (e.g. you stored a IFoo, but the cache key being passed is associated with an IBar)
 * The interal ```Log``` method now passes the _detail_ information as a delegate so we don't bother doing the ```ToString()``` and concatenation unless the ```DebugLog``` flag is true.
 * The internal calls to ```Log``` are now ```Conditional``` on the **DEBUG** constant when building, so we don't even attempt to call the method unless you're building from source in **DEBUG**

1.1 Adding features
 * Added ```Do``` and ```Do<TRet>``` for backing methods that don't return elements that can be cached (```void``` or ```int``` returns for example).
 * **Breaking Change** Added ```IBaseCacheKey.BuildKey``` and ```IBaseCacheKey.BuildPolicyKey``` for use by implementations of cache keys and exposed the standard behavior in ```BaseCacheKey`` so the standard way of doing things can easily be done.
 * **Breaking Change** Removed the N overloads of ```BaseCacheKey.BuildKey``` and ```BaseCacheKey.BuildPolicyKey``` and added a ```params string[]``` version so we can pass any number of strings to be joined.
 * **Breaking Change** ```AddOrUpdate``` does not enroll the new element in the cache  because we really don't have a good method of knowing the _best_ backing method to refill. Rather, we now just ```Clear``` the cache entries.
  
1.0
  * Initial release.
