---------------------------------------------------------------------
Rainbow Order Fulfilment System Read Me
---------------------------------------------------------------------

Rainbow OF is a Rainbow Order Fulfilment that is an open source solution to managing order fulfilment from WooCommerce to the customer.

Designed to pull data from Woo, or import from a legacy system it handles the order from placement to fulfilment. Minus the accounting.

SQ Server:
------------------
The project uses a SQLConnection string set in appsettings.json. Once you have updated the string to point to the correct SQL instance in the appsettings.json. The do an add-migration
to build the database using EF Core code first. The project to do the migrations in is RainbowOF.Data.SQL. Currently the migrations are stored there, so you will need to delete them
so that a new one is created.

There is no sample data, you need to pull data from your Woo site.

Coding standards:
------------------

Trying to follow -> https://docs.microsoft.com/en-us/dotnet/csharp/fundamentals/coding-style/coding-conventions

- Local class variables: "_" prefix with Pascal case similar to the class name.  
	- Exceptions:
		* When using a model. then use the word model as prefix to the Pascal case with the. 
		* Reference variables to be used in child components are preceded with Ref.
		* Event reference variable with Event as a suffix
		* Also boolean's are Prefix with Is or Do as Pascal case.
		* Component Parameters PascelCase no "_" with a word prefix Explaining source SourceXXX or ModelXXX etc 
- public interface variable camelCase (no I)
- Local function/routine variables: "_" prefix with camel case.
- Parameters for procedures: where appropriate use a word prefix to explain the parameter action (like delete or update or new) or use the prefix "p" but make sure you add a comment about using ///

enums: to be in camel case.

Table Name Standards:
----------------------
- Tables are named as the plural of the class that is a single record in the table. 
- All Lists in each class will also be plural. 
- All ID fields are the same name as the Single record suffixed with "Id".
- All models / database classes are store in RainbowOF.Models
