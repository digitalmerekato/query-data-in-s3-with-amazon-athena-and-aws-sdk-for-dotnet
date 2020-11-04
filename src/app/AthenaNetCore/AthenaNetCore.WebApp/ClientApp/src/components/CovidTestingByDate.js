﻿import React, { Component } from 'react'
import data from '../data/states.json'

export class CovidTestingByDate extends Component {

    constructor(props) {
        super(props);
        var date = new Date();
        date.setDate(date.getDate() - 1);
        this.state = {
            maxDate: date.toISOString().split("T")[0],
            selectedValue: "--",
            covidTesting: [],
            isLoading: true,
            isCheckingStatus: false,
            statusData: {
                atempt: 0,
                queryId: ""
            }
        };

        this.stateSelectChange = this.stateSelectChange.bind(this);
    }

    stateSelectChange(event) {
        const selectedDate = event.target.value;
        //console.log(selectedState);
        if (selectedDate !== "") {
            this.setState({ isLoading: true });
            this.loadCovidData(selectedDate);
        } else {
            this.setState({ covidTesting: [] });
        }
    }


    renderGridTable(covidTestingResults) {
        return (
            <table className='table table-striped' aria-labelledby="tabelLabel">
                <thead>
                    <tr>
                        <th>Date</th>
                        <th>State</th>
                        <th>Positive</th>
                        <th>Negative</th>
                        <th>Pending</th>
                        <th>Hospitalized</th>
                        <th>Death</th>
                        <th>Positive Increase</th>
                    </tr>
                </thead>
                <tbody>
                    {covidTestingResults.map(item =>
                        <tr key={item.date + item.state}>
                            <td>{item.date}</td>
                            <td>{item.state}</td>
                            <td>{item.positive}</td>
                            <td>{item.negative}</td>
                            <td>{item.pending}</td>
                            <td>{item.hospitalized}</td>
                            <td>{item.death}</td>
                            <td>{item.positiveincrease}</td>
                        </tr>
                    )}
                </tbody>
            </table>
        );
    }

    renderStatusCheck(statusData) {
        return (
            <p><em>Running query ID: {statusData.queryId}, Atemp: {statusData.atempt}</em></p>
        );
    }

    render() {
        let contents = this.state.isLoading
            ? (this.state.isCheckingStatus ? this.renderStatusCheck(this.state.statusData) : <p><em>You need to select a date to filter...</em></p>)
            : this.renderGridTable(this.state.covidTesting);

        return (
            <div>
                <h1 id="tabelLabel" >Testing Progress by Date</h1>
                <p>This component demonstrates fetching COVID-19 data from the server that uses Amazon Athena to run SQL Standard query on S3 files from a Data Lake account. This Component run Athena Query, get QueryExecutionId, check status of execution, and list results </p>
                <label>Date:
                    <input type="date" onChange={this.stateSelectChange} min="2020-01-01" max={this.state.maxDate} />
                </label>
                {contents}
                {this.state.ma}
            </div>
        );
    }

    async checkQueryStatus(queryId) {
        const response = await fetch(`covidtracking/query/status/${queryId}`);
        const statusResult = await response.json();

        if (statusResult.isStillRunning) {
            this.setState({
                isCheckingStatus: true,
                isLoading: true,
                statusData: {
                    queryId: queryId,
                    atempt: this.state.statusData.atempt + 1
                }
            });
            this.scheduleStatusCheck(queryId);
        } else {
            clearInterval(this.timer);
            await this.loadResult(queryId);
        }
    }

    async loadResult(queryId) {
        const response = await fetch(`/covidtracking/testing/run/result/${queryId}`);
        const dataResult = await response.json();
        this.setState({ covidTesting: dataResult, isLoading: false });
    }

    async loadCovidData(date) {
        const response = await fetch(`/CovidTracking/testing/run/${date}`);
        const dataResult = await response.json();
        this.setState({
            isCheckingStatus: true,
            statusData: {
                queryId: dataResult.queryId,
                atempt: 1
            }
        });
        await this.scheduleStatusCheck(dataResult.queryId);
    }

    scheduleStatusCheck(queryId) {
        const SECONDS = 3;
        if (this.timer) {
            clearInterval(this.timer);
        }
        this.timer = setInterval(() => {
            this.checkQueryStatus(queryId);
        }, (SECONDS * 1000));
    }
}