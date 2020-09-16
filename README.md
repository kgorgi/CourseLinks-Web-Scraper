# CourseLinks Web Scaper

Course Links is a website that displays dependencies for University of Victoria's courses in an interactive graph. This projects contains the web scraper which was used to gather information about the University of Victoria's available courses from their online academic calendar.

## How It Works

The web scraper iterates and downloads all the relevant course information from the University of Victoria's online academic calender. The web scraper then takes this course information and creates a set of output files. These output files are then statically hosted with and consumed by the CourseLinks UI. This method to store information was chosen to avoid the need of setting up a database.

This project was built using C# and the .NET Framework. 

## Website 
The corresponding website used to display course information gathered by the web scraper is available here: [CourseLinks UI](https://github.com/kgorgi/CourseLinks-UI). A UI demo is also available in the linked GitHub repository. 
