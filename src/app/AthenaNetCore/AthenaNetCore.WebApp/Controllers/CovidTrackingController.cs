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
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using AthenaNetCore.BusinessLogic.Entities;
using AthenaNetCore.BusinessLogic.Repositories;
using AthenaNetCore.WebApp.Models;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;

namespace AthenaNetCore.WebApp.Controllers
{
    [ApiController]
    [Route("[controller]")]
    public class CovidTrackingController : Controller
    {
        const string ERROR_MSG = "Oops... It was not possible to run this query, please check server log ref";
        private readonly ILogger<CovidTrackingController> _logger;

        public CovidTrackingController(ICovidTestingRepository testingRepository,
            IHospitalRepository hospitalRepository,
            ILogger<CovidTrackingController> logger)
        {
            TestingRepository = testingRepository;
            HospitalRepository = hospitalRepository;
            _logger = logger;
        }

        public ICovidTestingRepository TestingRepository { get; }
        public IHospitalRepository HospitalRepository { get; }


        [HttpGet("hospitals")]
        public async Task<IEnumerable<HospitalBeds>> Hospitals()
        {
            return await HospitalRepository.HospitalsBedsWaitResultAsync();
        }

        [HttpGet("hospitals/run")]
        [ProducesResponseType(typeof(AthenaQueryResultModel), StatusCodes.Status200OK)]
        [ProducesResponseType(typeof(AppErrorModel), StatusCodes.Status400BadRequest)]
        public async Task<ActionResult<AthenaQueryResultModel>> HospitalsRunAndGo()
        {
            try
            {
                var result = await HospitalRepository.HospitalsBedsAsync();
                return Ok(new AthenaQueryResultModel
                {
                    QueryId = result,
                    IsStillRunning = true
                });
            }
            catch (Exception ex)
            {
                var msg = $"{ERROR_MSG}:{Guid.NewGuid()}";
                _logger.LogError(ex, msg);
                return BadRequest(new AppErrorModel
                {
                    ErrorMessage = msg
                });
            }
        }

        [HttpGet("hospitals/run/result/{queryId}")]
        public async Task<IEnumerable<HospitalBeds>> HospitalsResult(string queryId)
        {
            return await HospitalRepository.HospitalsBedsAsync(queryId);
        }


        [HttpGet("query/status/{queryId}")]
        [ProducesResponseType(typeof(AthenaQueryResultModel), StatusCodes.Status200OK)]
        [ProducesResponseType(typeof(AppErrorModel), StatusCodes.Status400BadRequest)]
        public async Task<IActionResult> HospitalsStatus(string queryId)
        {
            try
            {
                var result = await HospitalRepository.IsTheQueryStillRunning(queryId);
                return Ok(new AthenaQueryResultModel
                {
                    QueryId = queryId,
                    IsStillRunning = result
                });
            }
            catch (Exception ex)
            {
                var msg = $"Something went wrong for queryId: {queryId}, please check the server log";
                _logger.LogError(ex, msg);
                return BadRequest(new AppErrorModel
                {
                    ErrorMessage = msg
                });
            }
        }


        [HttpGet("testing/run/{stateAbbreviation}")]
        [ProducesResponseType(typeof(AthenaQueryResultModel), StatusCodes.Status200OK)]
        [ProducesResponseType(typeof(AppErrorModel), StatusCodes.Status400BadRequest)]
        public async Task<ActionResult<AthenaQueryResultModel>> TestingByStateRunAndGo(string stateAbbreviation)
        {
            try
            {
                var result = await TestingRepository.ProgressAsync(stateAbbreviation);
                return Ok(new AthenaQueryResultModel
                {
                    QueryId = result,
                    IsStillRunning = true
                });
            }
            catch (Exception ex)
            {
                var msg = $"{ERROR_MSG}:{Guid.NewGuid()}";
                _logger.LogError(ex, msg);
                return BadRequest(new AppErrorModel
                {
                    ErrorMessage = msg
                });
            }
        }

        [HttpGet("testing/run/{date:DateTime}")]
        [ProducesResponseType(typeof(AthenaQueryResultModel), StatusCodes.Status200OK)]
        [ProducesResponseType(typeof(AppErrorModel), StatusCodes.Status400BadRequest)]
        public async Task<ActionResult<AthenaQueryResultModel>> TestingByDateRunAndGo(DateTime date)
        {
            try
            {
                var result = await TestingRepository.ProgressAsync(date);
                return Ok(new AthenaQueryResultModel
                {
                    QueryId = result,
                    IsStillRunning = true
                });
            }
            catch (Exception ex)
            {
                var msg = $"{ERROR_MSG}:{Guid.NewGuid()}";
                _logger.LogError(ex, msg);
                return BadRequest(new AppErrorModel
                {
                    ErrorMessage = msg
                });
            }
        }

        [HttpGet("testing/run/result/{queryId}")]
        public Task<IEnumerable<CovidTestingStatesDaily>> TestingResult(string queryId)
        {
            return TestingRepository.GetTestingQueryResultAsync(queryId);
        }

    }
}
