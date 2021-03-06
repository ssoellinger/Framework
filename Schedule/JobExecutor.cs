﻿/*
  This file is part of CNCLib - A library for stepper motors.

  Copyright (c) Herbert Aitenbichler

  Permission is hereby granted, free of charge, to any person obtaining a copy of this software and associated documentation files (the "Software"), 
  to deal in the Software without restriction, including without limitation the rights to use, copy, modify, merge, publish, distribute, sublicense, 
  and/or sell copies of the Software, and to permit persons to whom the Software is furnished to do so, subject to the following conditions:

  The above copyright notice and this permission notice shall be included in all copies or substantial portions of the Software.

  THE SOFTWARE IS PROVIDED "AS IS", WITHOUT WARRANTY OF ANY KIND, EXPRESS OR IMPLIED, INCLUDING BUT NOT LIMITED TO THE WARRANTIES OF MERCHANTABILITY, 
  FITNESS FOR A PARTICULAR PURPOSE AND NONINFRINGEMENT. IN NO EVENT SHALL THE AUTHORS OR COPYRIGHT HOLDERS BE LIABLE FOR ANY CLAIM, DAMAGES OR OTHER LIABILITY, 
  WHETHER IN AN ACTION OF CONTRACT, TORT OR OTHERWISE, ARISING FROM, OUT OF OR IN CONNECTION WITH THE SOFTWARE OR THE USE OR OTHER DEALINGS IN THE SOFTWARE. 
*/

namespace Framework.Schedule
{
    using System;
    using System.Collections.Generic;
    using System.Threading;
    using System.Threading.Tasks;

    using Framework.Schedule.Abstraction;
    using Framework.Tools.Abstraction;

    using Microsoft.Extensions.DependencyInjection;
    using Microsoft.Extensions.Logging;

    internal abstract class JobExecutor : IJobExecutor
    {
        private readonly IServiceProvider    _serviceProvider;
        private readonly ICurrentDateTime    _currentDateTime;
        private readonly ILogger             _logger;
        private readonly ILoggerFactory      _loggerFactory;
        private readonly Type                _job;
        private readonly object              _state;
        private readonly IList<IJobExecutor> _thenExecutors = new List<IJobExecutor>();

        private volatile bool _disposed;

        protected bool IsDisposed => _disposed;

        public CancellationTokenSource CtSource { get; set; } = new CancellationTokenSource();

        public Timer Timer { get; set; }

        protected JobExecutor(IServiceProvider serviceProvider, ILoggerFactory loggerFactory, ICurrentDateTime currentDateTime, Type job, object state)
        {
            _serviceProvider = serviceProvider;
            _currentDateTime = currentDateTime;
            _logger          = loggerFactory.CreateLogger(job);
            _loggerFactory   = loggerFactory;
            _job             = job;
            _state           = state;
        }

        /// <summary>
        /// Start a timer with "Execute" as callback
        /// </summary>
        public abstract void Start();

        public IJobExecutor Then(Type job, object state)
        {
            var thenJob = new ThenJobExecutor(_serviceProvider, _loggerFactory, _currentDateTime, job, state);
            _thenExecutors.Add(thenJob);
            return thenJob;
        }

        public void Dispose()
        {
            foreach (var thenJobs in _thenExecutors)
            {
                ((JobExecutor)thenJobs).Dispose();
            }

            _disposed = true;
            Timer?.Dispose();
        }

        protected virtual void Executing()
        {
        }

        protected virtual void Executed()
        {
        }

        protected async Task Execute()
        {
            Executing();

            try
            {
                using (var scope = _serviceProvider.CreateScope())
                {
                    var job = (IJob)scope.ServiceProvider.GetService(_job);
                    await job.Execute(_state, CtSource.Token);
                }
            }
            catch (Exception e)
            {
                _logger.LogError(e, "Unhandled exception by job.");
            }

            Executed();

            await ExecuteThen();
        }

        private async Task ExecuteThen()
        {
            foreach (var thenJobs in _thenExecutors)
            {
                await ((JobExecutor)thenJobs).Execute();
            }
        }

        private sealed class ThenJobExecutor : JobExecutor
        {
            public ThenJobExecutor(IServiceProvider serviceProvider, ILoggerFactory loggerFactory, ICurrentDateTime currentDateTime, Type job, object state) :
                base(serviceProvider, loggerFactory, currentDateTime, job, state)
            {
            }

            public override void Start()
            {
            }
        }
    }
}