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

using System;
using System.Collections.Generic;
using System.Net.Http;
using System.Threading.Tasks;

namespace Framework.Service.WebAPI
{
    public abstract class CRUDServiceBase<T, TKey> : ServiceBase where T : class where TKey : IComparable
    {
        protected abstract TKey GetKey(T value);

        protected CRUDServiceBase(HttpClient httpClient) : base(httpClient)
        {

        }

        public async Task<T> Get(TKey id)
        {
            var client = GetHttpClient();

            HttpResponseMessage response = await client.GetAsync(CreatePathBuilder().AddPath(id.ToString()).Build());
            if (response.IsSuccessStatusCode)
            {
                T value = await response.Content.ReadAsAsync<T>();
                return value;
            }

            return null;
        }

        public Task<IEnumerable<T>> Get(IEnumerable<TKey> keys)
        {
            throw new NotImplementedException();
        }

        public async Task<IEnumerable<T>> GetAll()
        {
            var client = GetHttpClient();

            HttpResponseMessage response = await client.GetAsync(CreatePathBuilder().Build());
            if (response.IsSuccessStatusCode)
            {
                IEnumerable<T> dtos = await response.Content.ReadAsAsync<IEnumerable<T>>();
                return dtos;
            }

            return null;
        }

        public async Task<TKey> Add(T value)
        {
            var client = GetHttpClient();

            HttpResponseMessage response = await client.PostAsJsonAsync(CreatePathBuilder().Build(), value);

            if (!response.IsSuccessStatusCode)
            {
                return default(TKey);
            }

            return GetKey(await response.Content.ReadAsAsync<T>());
        }

        public Task<IEnumerable<TKey>> Add(IEnumerable<T> values)
        {
            throw new NotImplementedException();
        }

        public async Task Delete(T value)
        {
            await Delete(GetKey(value));
        }

        public Task Delete(IEnumerable<T> values)
        {
            throw new NotImplementedException();
        }

        public async Task Delete(TKey key)
        {
            var client = GetHttpClient();

            HttpResponseMessage response = await client.DeleteAsync(CreatePathBuilder().AddPath(key.ToString()).Build());

            if (response.IsSuccessStatusCode)
            {
                //return RedirectToAction("Index");
            }

            //return HttpNotFound();
        }

        public Task Delete(IEnumerable<TKey> keys)
        {
            throw new NotImplementedException();
        }

        public async Task Update(T value)
        {
            var client = GetHttpClient();

            HttpResponseMessage response = await client.PutAsJsonAsync(CreatePathBuilder().AddPath(GetKey(value).ToString()).Build(), value);
        }

        public Task Update(IEnumerable<T> values)
        {
            throw new NotImplementedException();
        }
    }
}