---------------------------------------------------------------------
Rainbow Order Fulfilment System Read Me
---------------------------------------------------------------------

Rainbow OF is a Rainbow Order Fulfilment that is an open source solution to managing order fulfilment from WooCommerce to the customer.

Designed to pull data from Woo, or import from a legacy system it handles the order from placement to fulfilment. Minus the accounting.


Coding standards:
------------------

Trying to follow -> https://docs.microsoft.com/en-us/dotnet/csharp/fundamentals/coding-style/coding-conventions

- Local class variables: "_" prefix with Pascal case similar to the class name.  
	- Exceptions:
		* When using a model. then use the word model as prefix to the Pascal case with the. 
		* Reference variables to be used in child components are preceded with Ref.
		* Event reference variable with Event as a suffix
		* Also boolean's are Prefix with Is or Do as Pascal case.
		* Component Parameters PascelCase no "_" with a word prefix Explaining soure SourceXXX or ModelXXX etc 
- public interface variable camelCase (no I)
- Local function/routine variables: "_" prefix with camel case.
- Parameters for procedures: where appropriate use a word prefix to explain the parameter action (like delete or update or new) or use the prefix "p" but make sure you add a comment about using ///

enums: to be in camel case.

Table Name Standards:
----------------------

