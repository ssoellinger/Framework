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

using Framework.Repository.Abstraction;

namespace Framework.Repository
{
    using System;
    using System.Collections.Generic;
    using System.Linq;
    using System.Threading.Tasks;

    using Microsoft.EntityFrameworkCore;

    public abstract class CrudRepository<TDbContext, TEntity, TKey> : GetRepository<TDbContext, TEntity, TKey>
        where TDbContext : DbContext where TEntity : class
    {
        protected CrudRepository(TDbContext dbContext)
            : base(dbContext)
        {
        }

        protected IQueryable<TEntity> TrackingQueryWithInclude => AddInclude(TrackingQuery);

        protected IQueryable<TEntity> TrackingQueryWithOptional => AddOptionalWhere(TrackingQuery);

        #region Get Tracking

        public async Task<IList<TEntity>> GetTrackingAll()
        {
            return await GetAll(AddInclude(TrackingQueryWithOptional));
        }

        public async Task<TEntity> GetTracking(TKey key)
        {
            return await Get(TrackingQueryWithInclude, key);
        }

        public async Task<IList<TEntity>> GetTracking(IEnumerable<TKey> keys)
        {
            return await Get(TrackingQueryWithInclude, keys);
        }

        #endregion

        #region CRUD

        public void Add(TEntity entity)
        {
            AddEntity(entity);
        }

        public void AddRange(IEnumerable<TEntity> entities)
        {
            AddEntities(entities);
        }

        public void Delete(TEntity entity)
        {
            DeleteEntity(entity);
        }

        public void DeleteRange(IEnumerable<TEntity> entities)
        {
            DeleteEntities(entities);
        }

        public void SetState(TEntity entity, MyEntityState state)
        {
            SetEntityState(entity, (Microsoft.EntityFrameworkCore.EntityState)state);
        }

        public void SetValue(TEntity entity, TEntity values)
        {
            AssignValues(entity, values);
            base.SetValue(entity, values);
        }

        public void SetValueGraph(TEntity trackingEntity, TEntity values)
        {
            AssignValuesGraph(trackingEntity, values);
        }

        protected virtual void AssignValues(TEntity trackingEntity, TEntity values)
        {
        }

        protected virtual void AssignValuesGraph(TEntity trackingEntity, TEntity values)
        {
            SetValue(trackingEntity, values);
        }

        #endregion

        public void Sync(ICollection<TEntity> inDb, ICollection<TEntity> toDb, Func<TEntity, TEntity, bool> compareEntity, Action<TEntity> prepareForAdd)
        {
            Sync<TEntity>(inDb, toDb, compareEntity, prepareForAdd);
        }
    }
}