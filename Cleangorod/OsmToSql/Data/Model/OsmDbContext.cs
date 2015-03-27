namespace OsmToSql.Data.Model
{
    using System;
    using System.Data.Entity;
    using System.Linq;

    public class OsmDbContext : DbContext
    {
        // Your context has been configured to use a 'OsmDbContext' connection string from your application's 
        // configuration file (App.config or Web.config). By default, this connection string targets the 
        // 'OsmToSql.Data.Model.OsmDbContext' database on your LocalDb instance. 
        // 
        // If you wish to target a different database and/or database provider, modify the 'OsmDbContext' 
        // connection string in the application configuration file.
        public OsmDbContext()
            : base("name=OsmDbContext")
        {
        }

        // Add a DbSet for each entity type that you want to include in your model. For more information 
        // on configuring and using a Code First model, see http://go.microsoft.com/fwlink/?LinkId=390109.

        // public virtual DbSet<MyEntity> MyEntities { get; set; }
    }

    //public class MyEntity
    //{
    //    public int Id { get; set; }
    //    public string Name { get; set; }
    //}

    CREATE TABLE #Node(Id BIGINT not null primary key,coord BIGINT not null,intersection BIGINT null);
CREATE TABLE #Way (Id BIGINT not null primary key,type INT, oneway BIT);
CREATE TABLE #WayPoint ( WayID BIGINT not null, NodeID BIGINT not null, position integer not null );
CREATE TABLE #Intersection(Id BIGINT PRIMARY KEY,NodeID BIGINT not null);
CREATE INDEX Intersection_NodeID on #Intersection(NodeID);
}