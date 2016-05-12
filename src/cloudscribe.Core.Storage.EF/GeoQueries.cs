﻿// Copyright (c) Source Tree Solutions, LLC. All rights reserved.
// Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.
// Author:					Joe Audette
// Created:					2015-11-16
// Last Modified:			2016-05-09
// 

using cloudscribe.Core.Models.Geography;
using Microsoft.Data.Entity;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;

namespace cloudscribe.Core.Storage.EF
{
    public class GeoQueries : IGeoQueries
    {
        public GeoQueries(CoreDbContext dbContext)
        {
            this.dbContext = dbContext;
        }

        private CoreDbContext dbContext;

        public async Task<IGeoCountry> FetchCountry(Guid guid, CancellationToken cancellationToken = default(CancellationToken))
        {
            ThrowIfDisposed();
            cancellationToken.ThrowIfCancellationRequested();

            var item = await dbContext.Countries.SingleOrDefaultAsync(
                    x => x.Id == guid,
                    cancellationToken)
                    .ConfigureAwait(false);

            return item;
        }

        public async Task<IGeoCountry> FetchCountry(
            string isoCode2,
            CancellationToken cancellationToken = default(CancellationToken))
        {
            ThrowIfDisposed();
            cancellationToken.ThrowIfCancellationRequested();
            return await dbContext.Countries.SingleOrDefaultAsync(
                x => x.ISOCode2 == isoCode2,
                cancellationToken)
                .ConfigureAwait(false);
        }

        public async Task<int> GetCountryCount(CancellationToken cancellationToken = default(CancellationToken))
        {
            ThrowIfDisposed();
            cancellationToken.ThrowIfCancellationRequested();
            return await dbContext.Countries.CountAsync<GeoCountry>(cancellationToken)
                .ConfigureAwait(false);
        }

        public async Task<List<IGeoCountry>> GetAllCountries(CancellationToken cancellationToken = default(CancellationToken))
        {
            ThrowIfDisposed();
            cancellationToken.ThrowIfCancellationRequested();

            var query = from c in dbContext.Countries
                        orderby c.Name ascending
                        select c;

            var items = await query.AsNoTracking()
                .ToListAsync<IGeoCountry>(cancellationToken)
                .ConfigureAwait(false)
                ;

            return items;

        }

        public async Task<List<IGeoCountry>> GetCountriesPage(
            int pageNumber,
            int pageSize,
            CancellationToken cancellationToken = default(CancellationToken))
        {
            ThrowIfDisposed();
            cancellationToken.ThrowIfCancellationRequested();

            int offset = (pageSize * pageNumber) - pageSize;

            var query = dbContext.Countries.OrderBy(x => x.Name)
                .Select(p => p)
                .Skip(offset)
                .Take(pageSize)
                ;


            return await query
                 .AsNoTracking()
                 .ToListAsync<IGeoCountry>(cancellationToken)
                 .ConfigureAwait(false);

        }

        public async Task<IGeoZone> FetchGeoZone(
            Guid guid,
            CancellationToken cancellationToken = default(CancellationToken))
        {
            ThrowIfDisposed();
            cancellationToken.ThrowIfCancellationRequested();

            var item = await dbContext.States.SingleOrDefaultAsync(
                    x => x.Id == guid,
                    cancellationToken)
                    .ConfigureAwait(false);

            return item;
        }

        public Task<int> GetGeoZoneCount(
            Guid countryGuid,
            CancellationToken cancellationToken = default(CancellationToken))
        {
            ThrowIfDisposed();
            cancellationToken.ThrowIfCancellationRequested();
            return dbContext.States.CountAsync<GeoZone>(
                g => g.CountryId == countryGuid, cancellationToken);
        }

        public async Task<List<IGeoZone>> GetGeoZonesByCountry(
            Guid countryGuid,
            CancellationToken cancellationToken = default(CancellationToken))
        {
            ThrowIfDisposed();
            cancellationToken.ThrowIfCancellationRequested();
            //var query = from l in dbContext.States
            //            where l.CountryGuid == countryGuid
            //            orderby l.Name descending
            //            select l;

            var query = dbContext.States
                        .Where(x => x.CountryId == countryGuid)
                        .OrderByDescending(x => x.Name)
                        .Select(x => x);

            var items = await query
                .AsNoTracking()
                .ToListAsync<IGeoZone>(cancellationToken)
                .ConfigureAwait(false);

            return items;


        }

        public async Task<List<IGeoCountry>> CountryAutoComplete(
            string query,
            int maxRows,
            CancellationToken cancellationToken = default(CancellationToken))
        {
            ThrowIfDisposed();
            cancellationToken.ThrowIfCancellationRequested();

            // approximation of a LIKE operator query
            //http://stackoverflow.com/questions/17097764/linq-to-entities-using-the-sql-like-operator

            //var listQuery = from l in dbContext.Countries
            //                .Take(maxRows)
            //                where l.Name.Contains(query) || l.ISOCode2.Contains(query)
            //                orderby l.Name ascending
            //                select l;

            var listQuery = dbContext.Countries
                            .Where(x => x.Name.Contains(query) || x.ISOCode2.Contains(query))
                            .OrderBy(x => x.Name)
                            .Take(maxRows)
                            .Select(x => x);

            var items = await listQuery
                .AsNoTracking()
                .ToListAsync<IGeoCountry>(cancellationToken)
                .ConfigureAwait(false);

            return items;

        }

        public async Task<List<IGeoZone>> StateAutoComplete(
            Guid countryGuid,
            string query,
            int maxRows,
            CancellationToken cancellationToken = default(CancellationToken))
        {
            ThrowIfDisposed();
            cancellationToken.ThrowIfCancellationRequested();

            //var listQuery = from l in dbContext.States
            //                .Take(maxRows)
            //                where (
            //                l.CountryGuid == countryGuid &&
            //                (l.Name.Contains(query) || l.Code.Contains(query))
            //                )
            //                orderby l.Code ascending
            //                select l;

            var listQuery = dbContext.States
                            .Where(x =>
                           x.CountryId == countryGuid &&
                           (x.Name.Contains(query) || x.Code.Contains(query))
                            )
                            .OrderBy(x => x.Code)
                            .Take(maxRows)
                            .Select(x => x);

            return await listQuery
                .AsNoTracking()
                .ToListAsync<IGeoZone>(cancellationToken)
                .ConfigureAwait(false);

        }

        public async Task<List<IGeoZone>> GetGeoZonePage(
            Guid countryGuid,
            int pageNumber,
            int pageSize,
            CancellationToken cancellationToken = default(CancellationToken))
        {
            ThrowIfDisposed();
            cancellationToken.ThrowIfCancellationRequested();

            int offset = (pageSize * pageNumber) - pageSize;

            var query = dbContext.States
               .Where(x => x.CountryId == countryGuid)
               .OrderBy(x => x.Name)
               .Skip(offset)
               .Take(pageSize)
               .Select(p => p)
               ;

            return await query
                .AsNoTracking()
                .ToListAsync<IGeoZone>(cancellationToken)
                .ConfigureAwait(false);

        }

        public async Task<ILanguage> FetchLanguage(
            Guid guid,
            CancellationToken cancellationToken = default(CancellationToken))
        {
            ThrowIfDisposed();
            cancellationToken.ThrowIfCancellationRequested();

            var item = await dbContext.Languages.SingleOrDefaultAsync(
                    x => x.Id == guid,
                    cancellationToken)
                    .ConfigureAwait(false);

            return item;
        }

        public Task<int> GetLanguageCount(CancellationToken cancellationToken = default(CancellationToken))
        {
            ThrowIfDisposed();
            cancellationToken.ThrowIfCancellationRequested();
            return dbContext.Languages.CountAsync<Language>(cancellationToken);

        }

        public async Task<List<ILanguage>> GetAllLanguages(CancellationToken cancellationToken = default(CancellationToken))
        {
            ThrowIfDisposed();
            cancellationToken.ThrowIfCancellationRequested();

            var query = dbContext.Languages
                        .OrderBy(x => x.Name)
                        .Select(x => x);

            var items = await query
                .AsNoTracking()
                .ToListAsync<ILanguage>(cancellationToken)
                .ConfigureAwait(false);

            return items;

        }

        public async Task<List<ILanguage>> GetLanguagePage(
            int pageNumber,
            int pageSize,
            CancellationToken cancellationToken = default(CancellationToken))
        {
            ThrowIfDisposed();
            cancellationToken.ThrowIfCancellationRequested();

            int offset = (pageSize * pageNumber) - pageSize;

            var query = dbContext.Languages
                        .OrderBy(x => x.Name)
                        .Skip(offset)
                        .Take(pageSize)
                        .Select(x => x);

            return await query
                .AsNoTracking()
                .ToListAsync<ILanguage>(cancellationToken)
                .ConfigureAwait(false);

        }

        public async Task<ICurrency> FetchCurrency(
            Guid guid,
            CancellationToken cancellationToken = default(CancellationToken))
        {
            ThrowIfDisposed();
            cancellationToken.ThrowIfCancellationRequested();

            var item = await dbContext.Currencies.AsNoTracking().SingleOrDefaultAsync(
                    x => x.Id == guid,
                    cancellationToken)
                    .ConfigureAwait(false);

            return item;
        }

        public async Task<List<ICurrency>> GetAllCurrencies(CancellationToken cancellationToken = default(CancellationToken))
        {
            ThrowIfDisposed();
            cancellationToken.ThrowIfCancellationRequested();

            var query = dbContext.Currencies
                        .OrderBy(x => x.Title)
                        .Select(x => x);

            var items = await query
                .AsNoTracking()
                .ToListAsync<ICurrency>(cancellationToken)
                .ConfigureAwait(false);

            return items;

        }

        #region IDisposable Support

        private void ThrowIfDisposed()
        {
            if (disposedValue)
            {
                throw new ObjectDisposedException(GetType().Name);
            }
        }

        private bool disposedValue = false; // To detect redundant calls

        void Dispose(bool disposing)
        {
            if (!disposedValue)
            {
                if (disposing)
                {
                    // TODO: dispose managed state (managed objects).
                }

                // TODO: free unmanaged resources (unmanaged objects) and override a finalizer below.
                // TODO: set large fields to null.

                disposedValue = true;
            }
        }

        // TODO: override a finalizer only if Dispose(bool disposing) above has code to free unmanaged resources.
        // ~SiteRoleStore() {
        //   // Do not change this code. Put cleanup code in Dispose(bool disposing) above.
        //   Dispose(false);
        // }

        // This code added to correctly implement the disposable pattern.
        public void Dispose()
        {
            // Do not change this code. Put cleanup code in Dispose(bool disposing) above.
            Dispose(true);
            // TODO: uncomment the following line if the finalizer is overridden above.
            // GC.SuppressFinalize(this);
        }

        #endregion

    }
}
