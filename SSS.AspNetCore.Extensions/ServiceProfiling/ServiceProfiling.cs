using Microsoft.Extensions.Logging;
using Newtonsoft.Json;
using SSS.AspNetCore.Extensions.Handlers;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Reflection;

namespace SSS.AspNetCore.Extensions.ServiceProfiling
{
    public class ServiceProfiling<TDecorated, TService> : DispatchProxy
        where TService : class
    {
        private TDecorated _decorated;

        private ILogger<TService> _logger;

        private IValidatorHandler _validatorHandler;

        public static TDecorated CreateProxy(IServiceProvider provider)
        {
            object proxy = Create<TDecorated, ServiceProfiling<TDecorated, TService>>();

            ((ServiceProfiling<TDecorated, TService>)proxy).SetParameters(provider);

            return (TDecorated)proxy;
        }

        protected override object Invoke(MethodInfo targetMethod, object[] args)
        {
            string declaringType = $"{targetMethod.DeclaringType.Name}.{targetMethod.Name}";

            try
            {
                var newArgs = new List<object>(args);

                _logger.LogInformation("Start call to: @{DeclareType} with parameter @{Parameters}", declaringType, newArgs is not null ? JsonConvert.SerializeObject(newArgs) : string.Empty);
            }
            catch
            {
            }

            var time = new Stopwatch();

            time.Start();

            Validate(args);

            var result = targetMethod.Invoke(_decorated, args);

            time.Stop();

            _logger.LogInformation("End call to: @{Name} (@{Time} milliseconds)", targetMethod.Name, time.ElapsedMilliseconds);

            return result;
        }

        private void SetParameters(IServiceProvider provider)
        {
            Type type = typeof(TService);

            var args = GetArgs(provider, type.GetConstructors()[0]);

            _decorated = (TDecorated)Activator.CreateInstance(type, args);

            _logger = (ILogger<TService>)provider.GetService(typeof(ILogger<TService>));

            _validatorHandler = (IValidatorHandler)provider.GetService(typeof(IValidatorHandler));
        }

        private static object[] GetArgs(IServiceProvider provider, ConstructorInfo constructor)
        {
            var args = new List<object>();

            var parameters = constructor?.GetParameters();

            if (parameters != null)
            {
                foreach (var parameter in parameters)
                {
                    args.Add(provider.GetService(Type.GetType(parameter.ParameterType.AssemblyQualifiedName)));
                }
            }

            return args.ToArray();
        }

        private void Validate(object[] args)
        {
            foreach (var arg in args)
            {
                _validatorHandler.Validate(arg);
            }
        }
    }
}
