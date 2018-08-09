﻿using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Text;

namespace Ivony.Data
{



  public partial class DbContext
  {

    /// <summary>
    /// 辅助构建 DbContext 对象
    /// </summary>
    public class Builder
    {

      internal Builder( IServiceProvider serviceProvider )
      {
        ServiceProvider = serviceProvider;
      }


      internal Builder( DbContext parent )
      {
        Parent = parent;
        ServiceProvider = Parent.ServiceProvider;
      }



      /// <summary>
      /// 获取父级 DbContext 对象
      /// </summary>
      protected DbContext Parent { get; }

      /// <summary>
      /// 获取服务提供程序
      /// </summary>
      protected IServiceProvider ServiceProvider { get; }



      private IDictionary<string, IDbProvider> providers = new Dictionary<string, IDbProvider>();


      /// <summary>
      /// 注册一个数据访问提供程序
      /// </summary>
      /// <param name="database"></param>
      /// <param name="provider"></param>
      /// <returns></returns>
      public Builder RegisterDbProvider( string database, IDbProvider provider )
      {
        providers.Add( database, provider );
        return this;
      }




      bool? autoWhiteSpace;
      /// <summary>
      /// 设置自动添加空白分隔符设定
      /// </summary>
      public Builder SetAutoWhitespaceSeparator( bool value )
      {
        autoWhiteSpace = value;
        return this;
      }


      string database;

      /// <summary>
      /// 设置默认数据库
      /// </summary>
      public Builder SetDefaultDatabase( string database )
      {
        this.database = database;
        return this;
      }



      private IDictionary<Type, object> services = new Dictionary<Type, object>();


      public Builder RegisterService( Type serviceType, object serviceInstance )
      {
        if ( serviceType == null )
          throw new ArgumentNullException( nameof( serviceType ) );
        if ( serviceInstance == null )
          throw new ArgumentNullException( nameof( serviceInstance ) );


        var instanceType = serviceInstance.GetType();
        if ( serviceType.IsAssignableFrom( instanceType ) == false )
          throw new InvalidOperationException( $"Type of instance is \"{instanceType}\", it's cannot register for a service type of \"{serviceType}\"" );

        services[serviceType] = serviceInstance;

        return this;
      }

      public Builder RegisterService<T>( T serviceInstance )
      {
        if ( serviceInstance == null )
          throw new ArgumentNullException( nameof( serviceInstance ) );

        services[typeof( T )] = serviceInstance;

        return this;
      }

      public Builder RegisterService<T>( Func<T> serviceFactory )
      {
        if ( serviceFactory == null )
          throw new ArgumentNullException( nameof( serviceFactory ) );

        services[typeof( T )] = serviceFactory;

        return this;
      }



      internal DbContext Build()
      {
        DbContext context = new DbContext();

        context.Parent = Parent;
        context.ServiceProvider = ServiceProvider;

        context.DefaultDatabase = database ?? Parent?.DefaultDatabase ?? Db.DefaultDatabaseName;
        context.AutoWhitespaceSeparator = autoWhiteSpace ?? Parent?.AutoWhitespaceSeparator ?? false;

        context.services = new ReadOnlyDictionary<Type, object>( services );
        context.providers = new ReadOnlyDictionary<string, IDbProvider>( providers );

        return context;


      }
    }
  }
}