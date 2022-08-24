.. _edgedb-net-driver:

Driver
======

The Driver is the core connection between your code and the database, it's responsible for executing 
queries and building result types.

Clients & Client pools
----------------------

To actually communicate and execute queries with the database, you need a client. There are multiple 
types of clients available, but the most common is the ``EdgeDBClient`` class.

The ``EdgeDBClient`` class is a client pool that pools TCP clients by default. It automatically scales 
based off of demand and server configuration. We can initialize one like so:

.. code-block:: csharp

  var client = new EdgeDBClient();

.. note:: 
  Refer to `projects`_ for how clients autoresolve connection parameters.

You can start preforming queries with the ``client`` instance by using the ``QueryAsync`` method like so:

.. code-block:: csharp
  
  var result = await client.QueryAsync<string>("SELECT \"Hello, DotNet!\"");



.. _projects: https://www.edgedb.com/docs/guides/projects#ref-guide-using-projects
