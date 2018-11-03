# Cuckoo: the many-feathered method interceptor

## Overview
Cuckoo is a Fody add-in for AOP-style method interception. It modifies assemblies so that marked methods delegate immediately to user-provided classes, which are given full control over the call's further execution, and full access to all passed parameters.

Methods are most easily targeted by marking them directly by attribute, though an underlying mechanism is also exposed (via a simple API), whereby interceptions can be declared in general, either by the user or by intermediate libraries.

By being built on the Mono.Cecil and Fody libraries, Cuckoo aims to cater to cross-platform .NET development.

#### Features
 - Wraps method calls
 - Triggers arbitrary code
 - Gives access to (and alters) arguments and return values
 - Controlled by attribute declarations, or by arbitrary rules from user code
 - Cross-platform compatible

## Quick Start

#### Installation
Install via NuGet

#### Create a Cuckoo attribute  
Create a custom CuckooAttribute

#### Apply to methods  
Decorate the methods of your choice

### TO DO...
