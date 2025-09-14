# ContentGenerator.API

A comprehensive .NET 9.0 Web API for content generation and management, built with clean architecture principles and modern development practices.
<video src="https://github.com/user-attachments/assets/24f7cafb-5b33-446f-928a-51206e3f2613" autoplay loop muted playsinline width="600"></video>
## 🚀 Features

- **User Management**: Authentication, authorization, and user profile management
- **Project Management**: Create, update, and manage content generation projects
- **Template System**: Customizable templates for different content types
- **File Management**: Upload, store, and manage generated content files
- **History Tracking**: Complete audit trail of all user actions
- **Subscription Management**: Multi-tier subscription system with Stripe integration
- **Rate Limiting**: Built-in rate limiting for API protection
- **External Integrations**: 
  - Supabase for database and authentication
  - Stripe for payment processing
  - Unsplash for image services

## 🏗️ Architecture

The project follows Clean Architecture principles with the following layers:

- **API Layer** (`ContentGenerator.API`): Controllers, middleware, and API configuration
- **Core Layer** (`ContentGenerator.Core`): Business entities, DTOs, interfaces, and enums
- **Infrastructure Layer** (`ContentGenerator.Infrastructure`): Data access, external services, and repositories
- **Tests** (`ContentGenerator.Tests`): Unit and integration tests

## 🛠️ Technology Stack

- **.NET 9.0** - Latest .NET framework
- **Entity Framework Core** - ORM for data access
- **JWT Authentication** - Secure token-based authentication
- **Swagger/OpenAPI** - API documentation
- **Serilog** - Structured logging
- **CORS** - Cross-origin resource sharing
- **Rate Limiting** - API protection

## 📋 Prerequisites

- [.NET 9.0 SDK](https://dotnet.microsoft.com/download/dotnet/9.0)
- [Visual Studio 2022](https://visualstudio.microsoft.com/) or [VS Code](https://code.visualstudio.com/)
- [SQL Server](https://www.microsoft.com/en-us/sql-server/sql-server-downloads) or [SQL Server LocalDB](https://docs.microsoft.com/en-us/sql/database-engine/configure-windows/sql-server-express-localdb)
- [Supabase Account](https://supabase.com/) (for database and authentication)
- [Stripe Account](https://stripe.com/) (for payment processing)
- [Unsplash API Key](https://unsplash.com/developers) (for image services)

## 🚀 Getting Started

### 1. Clone the Repository

```bash
git clone <repository-url>
cd ContentGenerator.API
```

### 2. Environment Configuration

Create a `.env` file in the root directory with the following variables:

```env
# Database Configuration
CONNECTION_STRING=your_supabase_connection_string

# JWT Configuration
JWT_SECRET=your_jwt_secret_key
JWT_ISSUER=ContentGenerator.API
JWT_AUDIENCE=ContentGenerator.API
JWT_EXPIRY_MINUTES=60

# Supabase Configuration
SUPABASE_URL=your_supabase_url
SUPABASE_ANON_KEY=your_supabase_anon_key
SUPABASE_SERVICE_ROLE_KEY=your_supabase_service_role_key

# Stripe Configuration
STRIPE_PUBLISHABLE_KEY=your_stripe_publishable_key
STRIPE_SECRET_KEY=your_stripe_secret_key
STRIPE_WEBHOOK_SECRET=your_stripe_webhook_secret

# Unsplash Configuration
UNSPLASH_ACCESS_KEY=your_unsplash_access_key

# Email Configuration
SMTP_HOST=your_smtp_host
SMTP_PORT=587
SMTP_USERNAME=your_smtp_username
SMTP_PASSWORD=your_smtp_password
SMTP_FROM_EMAIL=noreply@contentgenerator.com

# File Storage
STORAGE_PATH=./uploads
MAX_FILE_SIZE_MB=10
```

### 3. Database Setup

```bash
# Navigate to the API project
cd src/ContentGenerator.API

# Add Entity Framework migrations
dotnet ef migrations add InitialCreate

# Update the database
dotnet ef database update
```

### 4. Build and Run

```bash
# Restore packages
dotnet restore

# Build the solution
dotnet build

# Run the API
dotnet run --project src/ContentGenerator.API
```

The API will be available at `https://localhost:7000` (HTTPS) or `http://localhost:5000` (HTTP).

### 5. API Documentation

Once the application is running, visit:
- **Swagger UI**: `https://localhost:7000/swagger`
- **API Documentation**: `https://localhost:7000/api-docs`

## 📚 API Endpoints

### Authentication
- `POST /api/auth/register` - User registration
- `POST /api/auth/login` - User login
- `POST /api/auth/refresh` - Refresh JWT token
- `POST /api/auth/logout` - User logout

### Users
- `GET /api/users/profile` - Get user profile
- `PUT /api/users/profile` - Update user profile
- `DELETE /api/users/account` - Delete user account

### Projects
- `GET /api/projects` - Get user projects
- `POST /api/projects` - Create new project
- `GET /api/projects/{id}` - Get project details
- `PUT /api/projects/{id}` - Update project
- `DELETE /api/projects/{id}` - Delete project

### Templates
- `GET /api/templates` - Get available templates
- `POST /api/templates` - Create new template
- `GET /api/templates/{id}` - Get template details
- `PUT /api/templates/{id}` - Update template
- `DELETE /api/templates/{id}` - Delete template

### Files
- `POST /api/files/upload` - Upload file
- `GET /api/files/{id}` - Download file
- `DELETE /api/files/{id}` - Delete file

### History
- `GET /api/history` - Get user activity history
- `GET /api/history/{id}` - Get specific history entry

## 🧪 Testing

```bash
# Run all tests
dotnet test

# Run tests with coverage
dotnet test --collect:"XPlat Code Coverage"

# Run specific test project
dotnet test src/ContentGenerator.Tests
```

## 🔧 Configuration

### Rate Limiting
Configure rate limiting in `appsettings.json`:

```json
{
  "RateLimit": {
    "MaxRequests": 100,
    "WindowMinutes": 1
  }
}
```

### Logging
The application uses Serilog for structured logging. Configure log levels in `appsettings.json`:

```json
{
  "Logging": {
    "LogLevel": {
      "Default": "Information",
      "Microsoft.AspNetCore": "Warning"
    }
  }
}
```

## 🐳 Docker Deployment

The project is fully containerized with Docker and Docker Compose for easy deployment and development.

### Prerequisites for Docker

- [Docker](https://www.docker.com/get-started) (version 20.10+)
- [Docker Compose](https://docs.docker.com/compose/install/) (version 2.0+)

### Quick Start with Docker

1. **Clone and navigate to the project:**
   ```bash
   git clone <repository-url>
   cd ContentGenerator.API
   ```

2. **Create environment file:**
   ```bash
   cp .env.example .env
   # Edit .env with your configuration values
   ```

3. **Start the development environment:**
   ```bash
   # Using the helper script (recommended)
   chmod +x scripts/docker-run.sh
   ./scripts/docker-run.sh dev
   
   # Or using docker-compose directly
   docker-compose up -d
   ```

4. **Access the application:**
   - API: http://localhost:5000
   - Swagger UI: http://localhost:5000/swagger
   - PostgreSQL: localhost:5432
   - Redis: localhost:6379

### Docker Commands

The project includes helper scripts for easy Docker management:

```bash
# Development commands
./scripts/docker-run.sh dev          # Start development environment
./scripts/docker-run.sh stop         # Stop all services
./scripts/docker-run.sh restart      # Restart all services
./scripts/docker-run.sh logs         # View all logs
./scripts/docker-run.sh logs-api     # View API logs only

# Production commands
./scripts/docker-run.sh prod         # Start production environment
./scripts/docker-run.sh build        # Build all services

# Maintenance commands
./scripts/docker-run.sh migrate      # Run database migrations
./scripts/docker-run.sh shell        # Open shell in API container
./scripts/docker-run.sh status       # Show service status
./scripts/docker-run.sh clean        # Clean up containers and volumes
```

### Docker Compose Files

- **`docker-compose.yml`** - Base configuration with all services
- **`docker-compose.override.yml`** - Development overrides (auto-loaded)
- **`docker-compose.prod.yml`** - Production configuration

### Services Included

- **API Service** - ContentGenerator.API (.NET 9.0)
- **PostgreSQL** - Database with persistent storage
- **Redis** - Caching and session storage
- **Nginx** - Reverse proxy (production profile)

### Environment Configuration

Create a `.env` file in the root directory:

```env
# Database Configuration
POSTGRES_PASSWORD=your_secure_password
CONNECTION_STRING=Host=postgres;Port=5432;Database=contentgenerator;Username=postgres;Password=your_secure_password

# JWT Configuration
JWT_SECRET=your-super-secret-jwt-key-change-this-in-production
JWT_ISSUER=ContentGenerator.API
JWT_AUDIENCE=ContentGenerator.API
JWT_EXPIRY_MINUTES=60

# External Services
SUPABASE_URL=your_supabase_url
SUPABASE_ANON_KEY=your_supabase_anon_key
SUPABASE_SERVICE_ROLE_KEY=your_supabase_service_role_key
STRIPE_PUBLISHABLE_KEY=your_stripe_publishable_key
STRIPE_SECRET_KEY=your_stripe_secret_key
STRIPE_WEBHOOK_SECRET=your_stripe_webhook_secret
UNSPLASH_ACCESS_KEY=your_unsplash_access_key

# Email Configuration
SMTP_HOST=your_smtp_host
SMTP_PORT=587
SMTP_USERNAME=your_smtp_username
SMTP_PASSWORD=your_smtp_password
SMTP_FROM_EMAIL=noreply@contentgenerator.com
```

### Production Deployment

1. **Build and run production environment:**
   ```bash
   ./scripts/docker-run.sh prod
   ```

2. **Or use docker-compose directly:**
   ```bash
   docker-compose -f docker-compose.yml -f docker-compose.prod.yml up -d
   ```

3. **Run database migrations:**
   ```bash
   ./scripts/docker-run.sh migrate
   ```

### Docker Image Details

- **Base Image**: `mcr.microsoft.com/dotnet/aspnet:9.0`
- **Build Image**: `mcr.microsoft.com/dotnet/sdk:9.0`
- **Multi-stage Build**: Optimized for production with minimal runtime image
- **Security**: Runs as non-root user
- **Health Checks**: Built-in health monitoring
- **Ports**: 80 (HTTP), 443 (HTTPS)

### Volume Mounts

- **Uploads**: `/app/uploads` - User uploaded files
- **Logs**: `/app/logs` - Application logs
- **Database**: Persistent PostgreSQL data
- **Redis**: Persistent Redis data

### Network Configuration

All services run on a custom Docker network (`contentgenerator-network`) for secure inter-service communication.

### Monitoring and Logs

```bash
# View real-time logs
docker-compose logs -f

# View specific service logs
docker-compose logs -f api
docker-compose logs -f postgres

# Check service health
docker-compose ps
```

### Troubleshooting

1. **Port conflicts**: Ensure ports 5000, 5432, and 6379 are available
2. **Permission issues**: Make sure Docker has proper permissions
3. **Database connection**: Wait for PostgreSQL to be ready before starting API
4. **Environment variables**: Verify all required environment variables are set

### Azure Container Deployment

1. **Build and push to Azure Container Registry:**
   ```bash
   # Build with Azure registry
   ./scripts/docker-build.sh latest your-registry.azurecr.io --push
   
   # Deploy to Azure Container Instances or App Service
   az container create --resource-group myResourceGroup \
     --name contentgenerator-api \
     --image your-registry.azurecr.io/contentgenerator-api:latest \
     --ports 80 443
   ```

2. **Or use Azure App Service with Docker:**
   - Create App Service with Docker container
   - Configure environment variables
   - Deploy using Azure DevOps or GitHub Actions

## 🤝 Contributing

1. Fork the repository
2. Create a feature branch (`git checkout -b feature/amazing-feature`)
3. Commit your changes (`git commit -m 'Add some amazing feature'`)
4. Push to the branch (`git push origin feature/amazing-feature`)
5. Open a Pull Request

## 📄 License

This project is licensed under the MIT License - see the [LICENSE](LICENSE) file for details.

## 🆘 Support

For support and questions:
- Create an issue in the repository
- Check the [API Documentation](docs/api-documentation.md)
- Review the [Database Schema](docs/database-schema.md)

## 🔄 Version History

- **v1.0.0** - Initial release with core functionality
- **v1.1.0** - Added subscription management and Stripe integration
- **v1.2.0** - Enhanced file management and Unsplash integration
