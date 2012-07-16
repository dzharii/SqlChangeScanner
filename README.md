SQL Change Scanner
==================
Software testing tool for tracking SQL Server Database changes without affecting the Database itself.

This tool gives you a very high level view on what happened in the database after you’ve performed some action in on the application UI. 

Use case:
---------
You are testing an application that has a user registration page. You fill the user from with correct data and run the SQL Change Scanner for the first time. 
The tool will print all existing tables into the output window. 

Then you are pressing the button “Register” on the application UI. After operation is completed you are pressing the Scan the database again, and for example it gives the following output:

> [dbo].[USERS]
> 
> [dbo].[USER_PROFILES]

Now you know that the new user information is being stored into the two tables. 

After you delete the user and scan the database again, you might get the following Output:

> [dbo].[USERS]

That means the USERS table was affected and the USER_PROFILES is not. So now you might raise a question: why the table USER_PROFILES was not changed? Does it collect a garbage information about already deleted users? 


Please, see also a short [usage video]


[Overview in Russian]

  [usage video]: http://www.youtube.com/watch?feature=player_embedded&v=DBHAlUJAut8
  [Overview in Russian]: http://blog.zhariy.com/2012/07/sql-change-scanner-sql-server.html



