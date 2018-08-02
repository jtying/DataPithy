﻿using System;
using System.Data;
using System.Linq;
using Ivony.Data.MySqlClient;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Ivony.Data.Queries;
using Microsoft.Extensions.DependencyInjection;

namespace Ivony.Data.Test
{
  [TestClass]
  public class MySqlTest
  {
    private static TestTraceService traceService;



    [TestInitialize]
    public void Enter()
    {

      Db.Enter( builder => builder.UseMySql( "192.168.10.163", "Test", "robot", "mvxy8Bsamc2MkdW" ) );

      Db.T( $"DROP TABLE IF EXISTS testTable" ).ExecuteNonQuery();
      Db.T( $@"
CREATE TABLE testTable(
Id int(11) NOT NULL AUTO_INCREMENT,
Name varchar(50) DEFAULT NULL,
Content text,
PRIMARY KEY (Id)
)" ).ExecuteNonQuery();

    }

    [TestCleanup]
    public void Exit()
    {
      Db.Exit();
    }



    [TestMethod]
    public void StandardTest()
    {
      Assert.IsNull( Db.T( $"SELECT * FROM testTable" ).ExecuteScalar(), "空数据表查询测试失败！" );
      Assert.IsNull( Db.T( $"SELECT * FROM testTable" ).ExecuteFirstRow(), "空数据表查询测试失败！" );

      Assert.AreEqual( Db.T( $"SELECT COUNT(*) FROM testTable" ).ExecuteScalar<int>(), 0, "空数据表查询测试失败" );
      Assert.AreEqual( Db.T( $"INSERT INTO testTable ( Name, Content) VALUES ( {"Ivony"}, {"Test"} )" ).ExecuteNonQuery(), 1, "插入数据测试失败" );
      Assert.AreEqual( Db.T( $"SELECT * FROM testTable" ).ExecuteDynamics().Length, 1, "插入数据后查询测试失败" );
      Assert.IsNotNull( Db.T( $"SELECT ID FROM testTable" ).ExecuteFirstRow(), "插入数据后查询测试失败" );

      var dataItem = Db.T( $"SELECT * FROM testTable" ).ExecuteDynamicObject();
      Assert.AreEqual( dataItem.Name, "Ivony", "插入数据后查询测试失败" );
      Assert.AreEqual( dataItem["Content"], "Test", "插入数据后查询测试失败" );
    }

    [TestMethod]
    public void TransactionTest()
    {
#if false
      using ( var transaction = db.BeginTransaction() )
      {
        Assert.AreEqual( Db.T( $"INSERT INTO testTable ( Name, Content ) VALUES ( {"Ivony"}, {"Test"} )" ).ExecuteNonQuery(), 1, "插入数据测试失败" );
        Assert.AreEqual( Db.T( $"SELECT * FROM testTable" ).ExecuteDynamics().Length, 1, "插入数据后查询测试失败" );
      }

      Assert.AreEqual( Db.T( $"SELECT * FROM testTable" ).ExecuteDynamics().Length, 0, "自动回滚事务测试失败" );

      using ( var transaction = db.BeginTransaction() )
      {
        Assert.AreEqual( Db.T( $"INSERT INTO testTable ( Name, Content) VALUES (  ( {"Ivony"}, {"Test"} ) )" ).ExecuteNonQuery(), 1, "插入数据测试失败" );
        Assert.AreEqual( Db.T( $"SELECT * FROM testTable" ).ExecuteDynamics().Length, 1, "插入数据后查询测试失败" );

        transaction.Rollback();
      }

      Assert.AreEqual( Db.T( $"SELECT * FROM testTable" ).ExecuteDynamics().Length, 0, "手动回滚事务测试失败" );



      using ( var transaction = db.BeginTransaction() )
      {
        Assert.AreEqual( Db.T( $"INSERT INTO testTable ( Name, Content) VALUES (  ( {"Ivony"}, {"Test"} ) )" ).ExecuteNonQuery(), 1, "插入数据测试失败" );
        Assert.AreEqual( Db.T( $"SELECT * FROM testTable" ).ExecuteDynamics().Length, 1, "插入数据后查询测试失败" );

        transaction.Commit();
      }

      Assert.AreEqual( Db.T( $"SELECT * FROM testTable" ).ExecuteDynamics().Length, 1, "手动提交事务测试失败" );



      {
        Exception exception = null;
        var transaction = (MySqlDbTransactionContext) db.BeginTransaction();

        try
        {
          using ( transaction )
          {
            Db.T( $"SELECT * FROM Nothing" ).ExecuteNonQuery();
            transaction.Commit();
          }
        }
        catch ( Exception e )
        {
          exception = e;
        }

        Assert.IsNotNull( exception, "事务中出现异常测试失败" );
        Assert.AreEqual( transaction.Connection.State, ConnectionState.Closed );
      }
#endif
    }

    [TestMethod]
    public void TraceTest()
    {

      Db.T( $"SELECT * FROM testTable" ).ExecuteDataTable();

      var tracing = traceService.Last();

      var logs = tracing.TraceEvents;
      Assert.AreEqual( logs.Length, 3 );

      Assert.AreEqual( logs[0].EventName, "OnExecuting" );
      Assert.AreEqual( logs[1].EventName, "OnLoadingData" );
      Assert.AreEqual( logs[2].EventName, "OnComplete" );


      try
      {
        Db.T( $"SELECT * FROM Nothing" ).ExecuteDynamics();
      }
      catch
      {

      }

      tracing = traceService.Last();

      logs = tracing.TraceEvents;
      Assert.AreEqual( logs.Length, 2 );

      Assert.AreEqual( logs[0].EventName, "OnExecuting" );
      Assert.AreEqual( logs[1].EventName, "OnException" );
    }
  }
}
