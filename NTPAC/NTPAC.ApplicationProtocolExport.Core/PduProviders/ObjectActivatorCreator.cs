using System;
using System.Linq;
using System.Linq.Expressions;

namespace NTPAC.ApplicationProtocolExport.Core.PduProviders
{
  internal delegate T ObjectActivator<T>(params Object[] args);
  
  // Generic object creation which allow use of a parametrized constructor (not possible with new T())
  // Faster then Activator.CreateInstance
  // Based on https://rogerjohansson.blog/2008/02/28/linq-expressions-creating-objects/
  internal static class ObjectActivatorCreator<T> where T:class
  {
    public static readonly ObjectActivator<T> ObjectActivator = CompileObjectActivator(); 

    private static ObjectActivator<T> CompileObjectActivator()
    {
      var ctor = typeof(T).GetConstructors().First();
      var paramsInfo = ctor.GetParameters();                  

      //create a single param of type object[]
      var param = Expression.Parameter(typeof(Object[]), "args");
 
      var argsExp = new Expression[paramsInfo.Length];            

      //pick each arg from the params array 
      //and create a typed expression of them
      for (var i = 0; i < paramsInfo.Length; i++)
      {
        var index     = Expression.Constant(i);
        var paramType = paramsInfo[i].ParameterType;              

        var paramAccessorExp = Expression.ArrayIndex(param, index);              

        var paramCastExp = Expression.Convert (paramAccessorExp, paramType);              

        argsExp[i] = paramCastExp;
      }                  

      //make a NewExpression that calls the
      //ctor with the args we just created
      var newExp = Expression.New(ctor,argsExp);                  

      //create a lambda with the New
      //Expression as body and our param object[] as arg
      var lambda = Expression.Lambda(typeof(ObjectActivator<T>), newExp, param);              

      //compile it
      var compiled = (ObjectActivator<T>)lambda.Compile();
      return compiled;
    }
  }
}
