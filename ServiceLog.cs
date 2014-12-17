using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Linq.Expressions;
using System.Reflection;
using System.Text;


namespace Utilities
{
    using Contexts;
    public static class LogUtility
    {
        private static readonly IDictionary<string, object> _parametterDictionary = new Dictionary<string, object>();

        public static IEnumerable<ParameterInfo> Method()
        {
            var st = new StackTrace();
            var sf = st.GetFrame(1);
            var currentMethodName = sf.GetMethod();
            return currentMethodName.GetParameters();

        }


        public static void AddLog(this IEnumerable<ParameterInfo> parameter,
            string methodName,
            DateTime logTime,
            string ipAdres,
            string name,
            params Expression<Func<object>>[] providedExpressions
            )
        {
            try
            {
                const string formatLogParams = "{0}={1} - Type={2}";

                foreach (var providedExpression in from providedExpression in providedExpressions
                                                   let bodyType = providedExpression.Body
                                                   where bodyType is MemberExpression
                                                   select providedExpression)
                {
                    GetParameterValue((MemberExpression)providedExpression.Body);
                }
               
                var LogDbContext = new LogContext();
                var parameterInfos = parameter as ParameterInfo[] ?? parameter.ToArray();
                var description = String.Join(";",
                    parameterInfos.Select(s =>
                        string.Format(formatLogParams,
                            s.Name,
                            (_parametterDictionary.FirstOrDefault(n => n.Key == s.Name).Value),
                            s.ParameterType)));


                serviceLogDbContext.AddServiceLog(methodName, logTime, ipAdres, name, description);
            }
            catch (Exception exception)
            {
                ExceptionLog(Guid.NewGuid().ToString(), DateTime.Now, exception);
            }

        }

        public static void ExceptionLog(
            string errorId, DateTime logTime,
            Exception exception)
        {
            var sb = new StringBuilder();
            sb.AppendFormat("Exception Found:-Type: {0}", exception.GetType().FullName);
            sb.AppendLine();
            sb.AppendFormat("-Message: {0}", exception.Message);
            sb.AppendLine();
            sb.AppendFormat("-Source: {0}", exception.Source);
            sb.AppendLine();
            sb.AppendFormat("-Stacktrace: {0}", exception.StackTrace);
            sb.AppendLine();

            var servicePath = AppDomain.CurrentDomain.BaseDirectory;
            var dir = Path.GetDirectoryName(servicePath);
            dir += "\\ServiceLog";
            var filename = dir + "\\" + errorId + ".log";
            if (!Directory.Exists(dir))
                Directory.CreateDirectory(dir);
            var fs = new FileStream(filename, FileMode.OpenOrCreate, FileAccess.ReadWrite);
            //var reader = new StreamReader(fs);
            var writer = new StreamWriter(fs);
            writer.Write(sb.AppendLine());
            writer.Write("\n");
            writer.Close();
        }

        private static void GetParameterValue(MemberExpression body)
        {

            var constantExpression = (ConstantExpression)body.Expression;


            var value = ((FieldInfo)body.Member).GetValue(constantExpression.Value);
            var name = body.Member.Name;
            _parametterDictionary.Add(name, value);
        }


    }
}
