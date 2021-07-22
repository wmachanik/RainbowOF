# RainbowOF
Rainbow Order Fulfilment Project. A to be released open source project that integrates with Woo Products and Items to create a Order Fulfilment front end. Written in EF core and Blazor
---------------------------------------------------------------------
Rainbow Order Fulfilment System Read Me
---------------------------------------------------------------------

Rainbow OF is a Rainbow Order Fulfilment that is an open source solution to managing order fulfilment from WooCommerce to the customer.

Designed to pull data from Woo, or import from a legacy system it handles the order from placement to fulfilment. Minus the accounting.

Main Web Project is RainboeOf.Web.FrontEnd

SQL Server:
------------------
The project uses a SQLConnection string set in appsettings.json. Once you have updated the string to point to the correct SQL instance in the appsettings.json. The do an add-migration to build the database using EF Core code first. The best project to do the migrations in, is RainbowOF.Data.SQL. Currently the migrations are stored there, so you will need to delete them so that a new one is created.

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


Design Ideas
--------------
A draft design document call RainbowOfDesignAndDocumentation.doc is in the root folder.

Done
----
- Basics Settings
- Woo Settings integration
- Initial Woo import including:
	- Categories
	- Attributes
	- Attribute Varieties
	- Items (only parent and essentially data)
- Tools, including:
	- Basic modal support
	- Colour piciking tool
	- Toast messages (may migrate to blazorise solution)
- Items:
	- Category Lookup Component, including fitler, sort and paging support. Also CRUD with Woo integration.
	- Attributes Lookup Component, including fitler, sort and paging support. Also CRUD with Woo integration.
	- Attributes Varieties Lookup Component, used in Attributes Grid, and also in Attribute Varieties. Fitler, sort and paging done. CRUD done with Woo Support
	- Items Grid View componenet. With fitler, sort and paging support. Editting moved to seperate component
	- All done using an interface that addes a view then has similar CRUD / Fitler / Sort / Paging calls

Busy
-----
- Complete the Items edit:
	- Integrate categories, specifically predicable / trackable categories
	- Integreate attributes, specifically variation attributes so that Item variations (or children) can be created and dispalyed.
	- Add a Woo Sync to pull the Item Variations (to be done via an interface that allows Woo Import to use it
	- CRUD support to and from Database and also with Woo integration

To Do
------
- Additonal item types - GroupBy / virtual / collections
- Contacts all of it, including:
	- Data
	- Woo
	- POPIA
	- Accounts? 
- Orders all of it, including:
	- Data
	- Woo
	- Delivery sheet management
	- Predications ?	
- Third Party manager / Repairs
- Reminder emails and notifications including
	- Prediciton (do we store in a table or calculate?
	- Mobile?
- CSV Import (of the old data, but also support of the new stuff)
- Security:
	- google / ms single sign on 
- Auto Creation of sample data and table
- Move to Cloud database.

Next Phase
----------
- Open source the project?
- Mobile app to allow orders and tracking
- Sage / Accounting integration
- Item Templates > creation of items based on templates
- SAS version and model (free for x and then price brackets).
 
