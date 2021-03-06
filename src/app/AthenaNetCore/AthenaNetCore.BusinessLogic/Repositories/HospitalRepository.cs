﻿/*
 * Copyright Amazon.com, Inc. or its affiliates. All Rights Reserved.
 * SPDX-License-Identifier: MIT-0
 *
 * Permission is hereby granted, free of charge, to any person obtaining a copy of this
 * software and associated documentation files (the "Software"), to deal in the Software
 * without restriction, including without limitation the rights to use, copy, modify,
 * merge, publish, distribute, sublicense, and/or sell copies of the Software, and to
 * permit persons to whom the Software is furnished to do so.
 *
 * THE SOFTWARE IS PROVIDED "AS IS", WITHOUT WARRANTY OF ANY KIND, EXPRESS OR IMPLIED,
 * INCLUDING BUT NOT LIMITED TO THE WARRANTIES OF MERCHANTABILITY, FITNESS FOR A
 * PARTICULAR PURPOSE AND NONINFRINGEMENT. IN NO EVENT SHALL THE AUTHORS OR COPYRIGHT
 * HOLDERS BE LIABLE FOR ANY CLAIM, DAMAGES OR OTHER LIABILITY, WHETHER IN AN ACTION
 * OF CONTRACT, TORT OR OTHERWISE, ARISING FROM, OUT OF OR IN CONNECTION WITH THE
 * SOFTWARE OR THE USE OR OTHER DEALINGS IN THE SOFTWARE.
 */
using Amazon;
using Amazon.Athena;
using Amazon.Athena.Model;
using Amazon.Runtime;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Text;
using System.Threading.Tasks;
using System.Linq;
using AthenaNetCore.BusinessLogic.Extentions;
using AthenaNetCore.BusinessLogic.Entities;

namespace AthenaNetCore.BusinessLogic.Repositories
{
    public class HospitalRepository : BaseRepository, IHospitalRepository
    {
        const string quote = "\"";
        private readonly string QUERY_HOSPITALS_MOST_AFFECTED = $"SELECT * FROM  {quote}covid-19{quote}.{quote}hospital_beds{quote} ORDER BY potential_increase_in_bed_capac LIMIT 500;";

        public Task<IEnumerable<HospitalBeds>> HospitalsBedsWaitResultAsync()
        {
            return AmazonAthenaClient.QueryAsync<HospitalBeds>(QUERY_HOSPITALS_MOST_AFFECTED);
        }

        public Task<string> HospitalsBedsAsync()
        {
            return AmazonAthenaClient.QueryAndGoAsync(QUERY_HOSPITALS_MOST_AFFECTED);
        }


        public Task<IEnumerable<HospitalBeds>> HospitalsBedsAsync(string queryId)
        {
            if (string.IsNullOrWhiteSpace(queryId))
            {
                return default;
            }

            return AmazonAthenaClient.ProcessQueryResultsAsync<HospitalBeds>(queryId);
        }
    }
}

