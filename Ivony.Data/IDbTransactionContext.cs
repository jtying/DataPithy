﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Ivony.Data
{


  /// <summary>
  /// 定义数据库事务上下文
  /// </summary>
  public interface IDbTransactionContext : IDisposable
  {
    /// <summary>
    /// 提交事务
    /// </summary>
    void Commit();

    /// <summary>
    /// 回滚事务
    /// </summary>
    void Rollback();


    /// <summary>
    /// 开启事务，若事务创建时已经开启，则调用该方法没有副作用
    /// </summary>
    void BeginTransaction();



    /// <summary>
    /// 获取数据库查询执行器
    /// </summary>
    /// <returns>数据库查询执行器</returns>
    IDbExecutor GetExecutor();

  }


  /// <summary>
  /// 定义异步数据库事务上下文
  /// </summary>
  public interface IAsyncDbTransactionContext 
  {


    /// <summary>
    /// 异步提交事务
    /// </summary>
    Task CommitAsync();

    /// <summary>
    /// 异步回滚事务
    /// </summary>
    Task RollbackAsync();

    /// <summary>
    /// 异步开启事务，若事务创建时已经开启，则调用该方法没有副作用
    /// </summary>
    Task BeginTransactionAsync();

    /// <summary>
    /// 获取异步数据库查询执行器
    /// </summary>
    /// <returns>异步数据库查询执行器</returns>
    IAsyncDbExecutor GetDbExecutor();
  }

}

