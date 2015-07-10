#Cuckoo: a full-feathered method interceptor

##Overview
Cuckoo is a Fody add-in for AOP-style method interception. It modifies assemblies so that marked methods delegate immediately to user-provided classes, which are given full control over the call's further execution, and full access to all passed parameters.

Methods are most easily targeted by attribute, although an underlying mechanism is also exposed (via a simple API), whereby interceptions can be declared generally.

And by being built on the Mono.Cecil and Fody libraries, Cuckoo suits itself to .NET development on any platform.

####Features
 - Wraps method calls
 - Triggers arbitrary code
 - Gives access to (and alters) arguments and return values
 - Controlled by attribute declarations, or by arbitrary rules from user code
 - Cross-platform compatible


##Quick Start

####Installation
Install via NuGet

####Create a Cuckoo attribute  
Create a custom CuckooAttribute


####Apply to methods  
Decorate the methods of your choice
