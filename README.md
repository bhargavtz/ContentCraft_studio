# ContentCraft Studio ✨

ContentCraft Studio is a comprehensive web application designed to assist users in generating various forms of content, including blog posts, stories, image descriptions, Instagram captions, and business names. Leveraging advanced AI capabilities (likely Google Gemini), it provides a suite of tools to streamline content creation, making it easier for individuals and businesses to produce high-quality, engaging material.

## Table of Contents 📚

- [Features](#features)
- [Prerequisites](#prerequisites)
- [Installation](#installation)
- [Configuration](#configuration)
- [Usage](#usage)
- [API Documentation](#api-documentation)
- [Contributing](#contributing)
- [Contact](#contact)

## Features 🚀

*   **Blog Post Generation**: Create engaging blog posts on various topics.
*   **Story Generation**: Generate creative stories based on user input.
*   **Image Description**: Automatically generate descriptive text for images.
*   **Instagram Caption**: Craft catchy and relevant captions for Instagram posts.
*   **Business Name Generator**: Brainstorm unique and memorable business names.
*   **User Dashboard**: Manage generated content and track activity.
*   **Authentication**: Secure user login and profile management.
*   **Payment Integration**: (If applicable, based on `PaymentModel.cs` and `PricingController.cs`) Handle subscription or one-time payments for premium features.

## Prerequisites 🛠️

Before you begin, ensure you have met the following requirements:

*   **.NET 9.0 SDK**: Download and install the .NET 9.0 SDK from the official Microsoft website.
    *   [Download .NET](https://dotnet.microsoft.com/download/dotnet/9.0)
*   **MongoDB**: A running instance of MongoDB. You can install it locally or use a cloud-hosted service like MongoDB Atlas.
    *   [Install MongoDB Community Edition](https://docs.mongodb.com/manual/installation/)
    *   [MongoDB Atlas](https://www.mongodb.com/cloud/atlas)
*   **Google Cloud Project & Gemini API Key**: Access to the Google Gemini API is required for content generation features.
    *   [Google Cloud Console](https://console.cloud.google.com/)
    *   [Gemini API Documentation](https://ai.google.dev/docs/gemini_api_overview)

## Installation 💻

Follow these steps to get ContentCraft Studio up and running on your local machine.

1.  **Clone the repository**:

    ```bash
    git clone https://github.com/bhargavtz/ContentCraft_studio.git
    cd ContentCraft_studio
    ```


2.  **Restore NuGet packages**:

    ```bash
    dotnet restore
    ```

3.  **Build the project**:

    ```bash
    dotnet build
    ```

## Configuration ⚙️

The application uses `appsettings.json` and `appsettings.Development.json` for configuration. You will need to configure your MongoDB connection string and Gemini API key.

1.  **Open `appsettings.json` or `appsettings.Development.json`**:

    ```json
    {
      "Logging": {
        "LogLevel": {
          "Default": "Information",
          "Microsoft.AspNetCore": "Warning"
        }
      },
      "AllowedHosts": "*",
      "MongoDb": {
        "ConnectionString": "mongodb://localhost:27017",
        "DatabaseName": "ContentCraftDb"
      },
      "Gemini": {
        "ApiKey": "YOUR_GEMINI_API_KEY_HERE"
      },
      "Auth0": {
        "Domain": "YOUR_AUTH0_DOMAIN_HERE",
        "ClientId": "YOUR_AUTH0_CLIENT_ID_HERE",
        "ClientSecret": "YOUR_AUTH0_CLIENT_SECRET_HERE"
      }
    }
    ```

2.  **Update the following**:
    *   `MongoDb:ConnectionString`: Set this to your MongoDB connection string. If running locally, `mongodb://localhost:27017` is often the default.
    *   `MongoDb:DatabaseName`: The name of the database to use (e.g., `ContentCraftDb`).
    *   `Gemini:ApiKey`: Replace `YOUR_GEMINI_API_KEY_HERE` with your actual Google Gemini API key.
    *   `Auth0:Domain`, `Auth0:ClientId`, `Auth0:ClientSecret`: If using Auth0 for authentication, configure these values. Otherwise, you might need to remove or adjust the authentication setup in `Program.cs`.

## Usage 💡

To run the ContentCraft Studio application:

1.  **Start the application**:

    ```bash
    dotnet run
    ```

    The application will typically start on `https://localhost:7000` or `http://localhost:5000`. Check the console output for the exact URL.

2.  **Access in your browser**:
    Open your web browser and navigate to the URL provided in the console (e.g., `https://localhost:7000`).

    You will be greeted by the ContentCraft Studio homepage. From there, you can register an account, log in, and start using the various content generation tools.

## API Documentation 📖

ContentCraft Studio is a web application built with ASP.NET Core MVC, which includes server-side logic and API endpoints. While a dedicated API documentation portal is not currently available, you can explore the API endpoints by examining the controller files in the `Controllers/` directory.

Key controllers include:
*   `Controllers/GeminiController.cs`: Handles interactions with the Gemini API for content generation.
*   `Controllers/ToolsController.cs`: Manages requests for various content creation tools (e.g., image description, Instagram captions).
*   `Controllers/DashboardController.cs`: Provides data for the user dashboard.

## Contributing 🤝

We welcome contributions to ContentCraft Studio! To contribute, please follow these steps:

1.  **Fork the repository**.
2.  **Create a new branch** for your feature or bug fix:
    ```bash
    git checkout -b feature/your-feature-name
    ```
    or
    ```bash
    git checkout -b bugfix/issue-description
    ```
3.  **Make your changes** and ensure they adhere to the project's coding standards.
4.  **Test your changes** thoroughly.
5.  **Commit your changes** with a clear and concise commit message:
    ```bash
    git commit -m "feat: Add new feature X"
    ```
    or
    ```bash
    git commit -m "fix: Resolve issue Y"
    ```
6.  **Push your branch** to your forked repository:
    ```bash
    git push origin feature/your-feature-name
    ```
7.  **Open a Pull Request** to the `main` branch of the original repository, describing your changes in detail.


## Contact 📧

If you have any questions, suggestions, or need support, please open an issue on the GitHub repository or contact us at:

*   **Email**: bhargavthummar05@gmail.com (placeholder)
*   **GitHub Issues**: [https://github.com/bhargavtz/ContentCraft_studio/issues](https://github.com/bhargavtz/ContentCraft_studio/issues)

