# Power Position Reporter

This application generates intra-day reports of power positions for power traders. It aggregates trade volumes per hour and outputs the results to a CSV file based on a configurable schedule.
It is prepared based on the requirements described here: https://bitbucket.org/axso-tim/axso_etrm_coding_challenge/src/main/

## Features

- Fetches power trades from PowerService
- Aggregates trade volumes per hour in local time (Europe/London)
- Outputs results to a CSV file with specified format
- Schedules extracts at configurable intervals (from appsettings.json)
- Runs an extract when the application starts

## Technical Implementation

- Developed using .NET 8.0
- Uses Hangfire for reliable background job processing
- SQLite for lightweight, portable database storage
- NodaTime for accurate time zone handling

## Configuration

The application can be configured via the `appsettings.json` file or command-line arguments:

- `AppSettings:OutputPath`: The folder path where CSV files are saved
- `AppSettings:ExtractIntervalMinutes`: The interval (in minutes) between extracts

## Running the Application

```bash
dotnet run --OutputPath="C:\PowerPositionReports" --ExtractIntervalMinutes=30
```

## Output Format

The application generates CSV files with the naming format `PowerPosition_YYYYMMDD_HHMM.csv` (e.g., `PowerPosition_20250316_1030.csv`).

Each CSV file contains:
- Header row with "Local Time" and "Volume" columns
- Local time in 24-hour format (HH:MM) but it is actually converted from int, so minutes are always 00.
- Aggregated volume for each hour

## Dependencies

- Axpo.PowerService.dll (.NET Standard 2.0)
- Hangfire.Core
- Hangfire.SQLite
- NodaTime
- Microsoft.Extensions.Hosting
- Microsoft.Extensions.Configuration
- Microsoft.Extensions.Logging


## Further Development Ideas
- Cloud & Serverless: Migrate scheduling to AWS Lambda with EventBridge.
- Separation of Concerns: Extract job logic into a dedicated layer.
- Enhanced Logging: Integrate centralized logging (e.g., CloudWatch, ELK, also Serilog can be implemented for structural logging).
- Resilience: Improve retry policies and error handling.
- API/UI: Consider adding an API or web UI for on-demand extraction.
- Dashboard for jobs: Hangfire dashboard can be used to monitor and manage jobs. But it requires a web server.