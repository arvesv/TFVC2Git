# TFVC2Git

A tool for migrating TFS TFVC history to Git. NB. There might be better tools out there for a
"general migration". This tool is made for me(!) for the case of migration multiple TFS paths into
one Git repo.


The dependencies:
*   Windows and .NET 4.8 as we use the "old" TFS API
*   SQL Server database. Works with a "serverless" Azure SQL (basically it reties for 1 minute in case the database must start. I did this to save cost)
*	Visual Studio 2019 (or later?)

