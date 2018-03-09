using System;
using System.Linq;
using System.Linq.Expressions;

namespace NTPAC.Persistence.Generic.Facades.Extensions
{
  public static class ExpressionExtensions {
  
    public static Expression<Func<TTo, Boolean>> ConvertPredicateParameter<TFrom, TTo>(this Expression<Func<TFrom, Boolean>> predicate) where TTo : TFrom
    {
      var param     = Expression.Parameter(typeof(TTo), predicate.Parameters.First().Name);
      var paramConv = Expression.Convert(param, typeof(TFrom));
      var call      = Expression.Invoke(predicate, paramConv);
      var lambda    = Expression.Lambda(call, param);
      return (Expression <Func <TTo, Boolean >>)lambda;
    }
  }
}