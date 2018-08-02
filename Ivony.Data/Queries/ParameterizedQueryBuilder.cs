﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Ivony.Data.Queries
{

  /// <summary>
  /// 辅助构建 ParameterizedQuery 对象
  /// </summary>
  public class ParameterizedQueryBuilder : IParameterizedQueryBuilder
  {

    private StringBuilder textBuilder = new StringBuilder();

    private List<object> values = new List<object>();




    /// <summary>
    /// 用于同步的对象
    /// </summary>
    public object SyncRoot { get; } = new object();


    /// <summary>
    /// 添加一段查询文本
    /// </summary>
    /// <param name="text">要添加到末尾的查询文本</param>
    public void Append( string text )
    {
      lock ( SyncRoot )
      {
        textBuilder.Append( text.Replace( "#", "##" ) );
      }
    }


    /// <summary>
    /// 添加一个字符到查询文本
    /// </summary>
    /// <param name="ch">要添加到查询文本末尾的字符</param>
    public void Append( char ch )
    {
      lock ( SyncRoot )
      {
        if ( ch == '#' )
          textBuilder.Append( "##" );

        else
          textBuilder.Append( ch );
      }
    }


    /// <summary>
    /// 添加一个查询参数
    /// </summary>
    /// <param name="value">参数值</param>
    public void AppendParameter( object value )
    {
      var partial = value as IParameterizedQueryPartial;
      if ( partial == null )
      {
        var container = value as IDbQueryContainer;
        if ( container != null )
          partial = container.Query as IParameterizedQueryPartial;
      }

      if ( partial != null )
      {
        AppendPartial( partial );
        return;
      }


      lock ( SyncRoot )
      {
        values.Add( value );
        textBuilder.AppendFormat( "#{0}#", values.Count - 1 );
      }
    }


    /// <summary>
    /// 创建参数化查询对象实例
    /// </summary>
    /// <returns>参数化查询对象</returns>
    public ParameterizedQuery BuildQuery()
    {
      lock ( SyncRoot )
      {
        return new ParameterizedQuery( textBuilder.ToString(), values.ToArray() );
      }
    }


    /// <summary>
    /// 在当前位置添加一个部分查询
    /// </summary>
    /// <param name="partial">要添加的部分查询对象</param>
    protected virtual void AppendPartial( IParameterizedQueryPartial partial )
    {
      lock ( SyncRoot )
      {
        if ( textBuilder.Length > 0
          && char.IsWhiteSpace( textBuilder[textBuilder.Length - 1] ) == false
          && Db.DbContext.AutoWhitespaceSperator == true )

          Append( ' ' );

        partial.AppendTo( this );
      }
    }
  }
}
