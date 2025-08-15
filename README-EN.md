# CryptoWallet

**English** | [Русский](README-RU.md)

## Overview

CryptoWallet is a secure, multi-currency cryptocurrency wallet application that allows users to store, send, and receive various cryptocurrencies with enterprise-grade security features.

## Features

### User Management
- User registration and authentication
- Role-based access control (RBAC)
- Email verification
- Password reset functionality
- User profile management

### Wallet Management
- Multi-currency wallet support (BTC, ETH, USDT)
- Generate new wallet addresses
- View wallet balances and transaction history
- Detailed wallet information
- Wallet labeling and organization

### Transactions
- Send and receive cryptocurrencies
- Internal transfers between wallets
- Transaction history with advanced filtering
- Transaction details and status tracking
- Transaction queuing and processing

### Security Features
- Secure key management
- Two-factor authentication (2FA)
- IP whitelisting
- Device management
- Real-time transaction notifications
- Suspicious activity monitoring

### Administration
- User management dashboard
- System monitoring and analytics
- System configuration
- Automated backups
- Audit logging

## Technology Stack

### Backend
- .NET 8.0
- ASP.NET Core Web API
- Entity Framework Core 8.0
- PostgreSQL 13
- xUnit / Moq / FluentAssertions
- AutoMapper
- FluentValidation
- Serilog

### Security
- JWT Authentication
- BCrypt for password hashing
- Data encryption at rest
- Rate limiting
- Request validation

### Infrastructure
- Docker
- Docker Compose
- GitHub Actions for CI/CD
- Nginx as reverse proxy
- Redis for caching

## Getting Started

### Prerequisites
- .NET 8.0 SDK
- Docker and Docker Compose
- PostgreSQL 13+
- Node.js 16+ (for frontend)

### Installation

1. Clone the repository:
   ```bash
   git clone https://github.com/yourusername/CryptoWallet.git
   cd CryptoWallet
   ```

2. Set up the environment:
   ```bash
   cp .env.example .env
   # Update environment variables in .env file
   ```

3. Start the infrastructure:
   ```bash
   docker-compose up -d postgres redis
   ```

4. Apply database migrations:
   ```bash
   cd src/API
   dotnet ef database update
   ```

5. Run the application:
   ```bash
   dotnet run --project src/API
   ```

6. Access the application:
   - API: http://localhost:5000
   - Swagger UI: http://localhost:5000/swagger
   - Admin Panel: http://localhost:8080

## Testing

Run unit tests:
```bash
dotnet test
```

Run integration tests:
```bash
cd tests/IntegrationTests
dotnet test
```

## Security Best Practices

1. Always use HTTPS in production
2. Enable 2FA for all admin accounts
3. Regularly rotate API keys and secrets
4. Monitor system logs for suspicious activities
5. Keep all dependencies up to date

## Contributing

1. Fork the repository
2. Create a feature branch
3. Commit your changes
4. Push to the branch
5. Create a new Pull Request

## License

This project is licensed under the MIT License - see the [LICENSE](LICENSE) file for details.

## Contact

For questions and suggestions, please contact: [dennilen@gmail.com](mailto:dennile@ngmai.com)
