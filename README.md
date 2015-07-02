#Cuckoo: a full-feathered method interceptor

##Overview
Cuckoo is a Fody add-in for AOP-style method interception. It modifies assemblies so that marked methods immediately delegate to user-provided ICuckoo classes, which manage each call accordingly, and can trigger arbitrary code as they like.

Another para here for pacing.

By being based on the Mono.Cecil and Fody libraries, Cuckoo suits itself to .NET development on any platform.

####Features
 - Wraps method calls
 - Triggers arbitrary code
 - Gives access to (and alters) arguments and return values
 - Controlled by attribute declarations
 - Cross-platform compatible


##Quick Start

####Installation
Install via NuGet

####Create a Cuckoo attribute  
Create a custom CuckooAttribute

####Apply to methods  
Decorate the methods of your choice
