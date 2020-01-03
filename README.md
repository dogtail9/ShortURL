# A ASP.NET Core application hosted on different plattforms

This project is a tutorial on how to host an ASP.NET Core application on Kubernetes, Docker Swarm or Azure App Services.
The appllication is a short url application. The application stores long urls behind a shorter key value. 
When the user browse to the key value the browser will be redirected to the long url.

## Prerequisites

* Docker for Windows
* Visual Studio 2019

## The Application

The application consists of two parts, one service for redirection and one service for adding new urls.
We will also write a Gui application for adding urls.
The data will be stored in SQL Server and cached in Redis nere the redirect service. 
The cache will be updated using RabbitMQ when a url is added, deleted or updated.
The application will trace every call using Open Telemetry and W3C Trace Context. 
Zipkin will be used to gather the logs.