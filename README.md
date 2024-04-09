# Expense Tracker App
This is an Expense Tracker App in Asp.Net Core MVC using SyncFusion Components.

## Introduction
Developed the Expense Tracker App, a web application developed in ASP.NET Core 6 MVC utilizing SyncFusion Components and SQL Server database. This application aims to assist users in efficiently managing their expenses by providing features for tracking transactions, organizing expenses into categories, and visualizing spending patterns.

## Technologies
- **ASP.NET Core 6 MVC**: The application's backend and frontend are developed using ASP.NET Core 6 MVC framework.
- **SyncFusion Components**: SyncFusion Components are utilized for creating interactive UI elements such as grids, charts, and sidebars.
- **SQL Server Database**: SQL Server is used as the backend database for storing categories and transactions data.
- **C# Programming Language**: C# is the primary programming language used for server-side logic.
- **Entity Framework Core**: Entity Framework Core is employed for database access and management.

## Features
1. **CRUD Operations**: Implemented CRUD (Create, Read, Update, Delete) operations for both categories and transactions to allow users to manage their data effectively.
2. **Grid with Paging & Sorting**: Utilized SyncFusion grid component with paging and sorting functionalities to enhance data presentation and accessibility.
3. **Dashboard with Chart Elements**: Created a dashboard featuring chart elements to provide users with visual insights into their spending patterns.
4. **Side Menu within Dockable Sidebar**: Integrated a side menu within a dockable sidebar for easy navigation across different sections of the application.
5. **Login and Logout Authentication**: Implemented authentication features to ensure secure access to the application, allowing users to log in to their accounts and log out when done.

## Architecture
The Expense Tracker App follows a Model-View-Controller (MVC) architecture:
- **Model**: Represents the data and business logic of the application. It includes entities such as categories and transactions, as well as services for data manipulation.
- **View**: Displays the user interface of the application to the users. It includes HTML templates rendered by the server and served to the client.
- **Controller**: Handles user requests, processes input, and interacts with the model and view components to generate appropriate responses.

## Modules
1. **Category Management**: Allows users to create, view, update, and delete expense categories to organize their transactions efficiently.
2. **Transaction Tracking**: Enables users to record their transactions, including date, amount, description, and associated category.
3. **Dashboard**: Provides users with a visual representation of their spending habits through charts and graphs, aiding in better financial management.
4. **Navigation Sidebar**: Incorporates a sidebar menu for seamless navigation between different sections of the application.
5. **Authentication**: Provides secure login and logout functionality to authenticate users and protect sensitive data.

## Flow of the App
1. **Authentication**: Users are required to log in to access the application.
2. **Dashboard**: Upon successful login, users are directed to the dashboard, where they can view visualizations of their spending patterns.
3. **Category Management**: Users can navigate to the category management section to create, update, or delete expense categories as needed.
4. **Transaction Tracking**: Users can record their transactions, specifying the date, amount, description, and category for each transaction.
5. **Navigation**: Users can navigate between different sections of the application using the sidebar menu.
6. **Logout**: Users can log out of their accounts to securely end their session and protect their data.

## Users
The Expense Tracker App caters to individuals or businesses looking to manage their expenses effectively. Users can be anyone who wants to track their spending habits, categorize expenses, and gain insights into their financial activities.

## Additional Points
- **Security**: Implement appropriate authentication and authorization mechanisms to ensure data privacy and prevent unauthorized access.
- **Localization**: Consider adding support for multiple languages to make the application accessible to a broader audience.
- **Scalability**: Design the application with scalability in mind to accommodate potential growth in data volume and user base.
- **Performance Optimization**: Optimize database queries, frontend rendering, and server-side processing to enhance application performance and responsiveness.
- **Error Handling**: Implement robust error handling mechanisms to provide users with meaningful error messages and ensure smooth application operation.
- **Documentation**: Provide comprehensive documentation, including installation instructions, usage guidelines, and troubleshooting tips, to assist users in utilizing the application effectively.