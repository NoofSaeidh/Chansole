using Castle.DynamicProxy;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Serilog;
using SerilogTimings.Extensions;
using Serilog.Events;
using Microsoft.Extensions.Options;
using Serilog.Context;
using Serilog.Core.Enrichers;

namespace Chansole.Common;

public record LoggerInterceptorOptions
{
    public LogEventLevel CompletionLevel { get; init; }
    public LogEventLevel AbandonmentLevel { get; init; }
}

internal class LoggerInterceptor : AsyncInterceptorBase, IInterceptor
{
    private readonly ILogger _logger;
    private readonly IOptions<LoggerInterceptorOptions> _options;
    private readonly IInterceptor _interceptor;

    public LoggerInterceptor(ILogger logger, IOptions<LoggerInterceptorOptions> options)
    {
        _logger      = logger;
        _options     = options;
        _interceptor = this.ToInterceptor();
    }

    public void Intercept(IInvocation invocation)
    {
        _interceptor.Intercept(invocation);
    }

    protected override async Task InterceptAsync(IInvocation invocation, IInvocationProceedInfo proceedInfo,
                                                 Func<IInvocation, IInvocationProceedInfo, Task> proceed)
    {
        await InterceptAsyncImpl<bool>(invocation, proceedInfo, logResult: false, async (inv, info) =>
        {
            await proceed(inv, info);
            return true;
        });
    }

    protected override async Task<TResult> InterceptAsync<TResult>(
        IInvocation invocation,
        IInvocationProceedInfo proceedInfo,
        Func<IInvocation, IInvocationProceedInfo, Task<TResult>> proceed)
    {
        return await InterceptAsyncImpl(invocation, proceedInfo, logResult: true, proceed);
    }

    private async Task<TResult> InterceptAsyncImpl<TResult>(
        IInvocation invocation,
        IInvocationProceedInfo proceedInfo,
        bool logResult,
        Func<IInvocation, IInvocationProceedInfo, Task<TResult>> proceed)
    {
        var logger = _logger.ForContext(invocation.TargetType);
        using var operation = logger.OperationAt(_options.Value.CompletionLevel, _options.Value.AbandonmentLevel)
                                    .Begin("Invoking method {Method} with arguments {@Arguments}",
                                           args: invocation.Arguments.Prepend(invocation.Method.Name));
        using var _ = LogContext.Push(
            new PropertyEnricher("Method", invocation.Method.Name),
            new PropertyEnricher("Type", invocation.TargetType.FullName));
        {
            TResult result;
            try
            {
                result = await proceed(invocation, proceedInfo).ConfigureAwait(false);
            }
            catch (Exception ex)
            {
                operation.SetException(ex);
                operation.Abandon();
                throw;
            }

            if (logResult)
            {
                operation.EnrichWith("Result", result!, destructureObjects: true);
            }

            operation.Complete();
            return result;
        }
    }
}