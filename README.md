opinionated-cache
=================

An opinionated cache

To start using this library, run the following command in the [Package Manager Console](http://docs.nuget.org/docs/start-here/using-the-package-manager-console)

    PM> Install-Package opinionated-cache

[![opinionated-cache MyGet Build Status](https://www.myget.org/BuildSource/Badge/opinionated-cache?identifier=7067f82c-0d7b-4440-aef6-4650fd8e9e04)](https://www.myget.org/)

Release notes:

1.1 Adding features
 * Added ```Do``` and ```Do<TRet>``` for backing methods that don't return elements that can be cached (```void``` or ```int``` returns for example).
 * **Breaking Change** Added ```IBaseCacheKey.BuildKey``` and ```IBaseCacheKey.BuildPolicyKey``` for use by implementations of cache keys and exposed the standard behavior in ```BaseCacheKey`` so the standard way of doing things can easily be done.
 * **Breaking Change** Removed the N overloads of ```BaseCacheKey.BuildKey``` and ```BaseCacheKey.BuildPolicyKey``` and added a ```params string[]``` version so we can pass any number of strings to be joined.
 * **Breaking Change** ```AddOrUpdate``` does not enroll the new element in the cache  because we really don't have a good method of knowing the _best_ backing method to refill. Rather, we now just ```Clear``` the cache entries.
  
1.0
  * Initial release.
