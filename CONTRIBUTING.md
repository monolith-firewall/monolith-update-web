# Contributing to Monolith Firewall Update Site

Thank you for considering contributing to this project!

## Getting Started

1. **Fork the repository** on GitHub
2. **Clone your fork** locally:
   ```bash
   git clone https://github.com/your-username/Monolith-Firewall-UpdateSite.git
   cd Monolith-Firewall-UpdateSite
   ```
3. **Set up development environment**:
   ```bash
   ./scripts/build.sh
   ```

## Development Workflow

### Building the Project

```bash
# Using helper script
./scripts/build.sh

# Or manually
cd src
dotnet build
```

### Running in Development Mode

```bash
# Using helper script with hot reload
./scripts/run-dev.sh

# Or manually
cd src
dotnet watch run
```

### Running Tests

```bash
cd src
dotnet test
```

## Project Structure

- **src/** - Application source code
  - **Controllers/** - MVC and API controllers
  - **Models/** - Data models and view models
  - **Services/** - Business logic
  - **Views/** - Razor views
  - **Data/** - Database context and configurations
- **docs/** - Documentation files
- **scripts/** - Helper scripts for common tasks
- **.github/workflows/** - GitHub Actions CI/CD

## Making Changes

1. **Create a feature branch**:
   ```bash
   git checkout -b feature/your-feature-name
   ```

2. **Make your changes** following the coding standards below

3. **Test your changes** thoroughly

4. **Commit your changes**:
   ```bash
   git add .
   git commit -m "Add feature: description of your changes"
   ```

5. **Push to your fork**:
   ```bash
   git push origin feature/your-feature-name
   ```

6. **Create a Pull Request** on GitHub

## Coding Standards

### C# Code Style

- Follow Microsoft's [C# Coding Conventions](https://docs.microsoft.com/en-us/dotnet/csharp/fundamentals/coding-style/coding-conventions)
- Use meaningful variable and method names
- Add XML documentation comments for public APIs
- Keep methods focused and single-purpose
- Use async/await for I/O operations

### File Organization

- Controllers go in `src/Controllers/Admin/` or `src/Controllers/Api/`
- Domain models in `src/Models/Domain/`
- View models in `src/Models/ViewModels/`
- API responses in `src/Models/ApiResponses/`
- Business logic in `src/Services/`

### Naming Conventions

- **Controllers**: End with `Controller` (e.g., `FirewallUpdatesController`)
- **Services**: End with `Service` (e.g., `UpdateService`)
- **ViewModels**: End with `ViewModel` (e.g., `FirewallUpdateViewModel`)
- **API Responses**: End with `Response` (e.g., `FirewallUpdateResponse`)

## Database Changes

When adding or modifying database entities:

1. **Update the model** in `src/Models/Domain/`
2. **Update DbContext** if needed in `src/Data/ApplicationDbContext.cs`
3. **Create migration**:
   ```bash
   cd src
   dotnet ef migrations add YourMigrationName
   ```
4. **Test the migration**:
   ```bash
   dotnet ef database update
   ```

## Adding New Features

### Adding a New Admin Page

1. Create controller in `src/Controllers/Admin/`
2. Create view model in `src/Models/ViewModels/`
3. Create views in `src/Views/[ControllerName]/`
4. Update navigation in `src/Views/Shared/_Layout.cshtml`

### Adding a New API Endpoint

1. Create/update controller in `src/Controllers/Api/`
2. Create response model in `src/Models/ApiResponses/`
3. Add business logic to appropriate service
4. Update API documentation in README.md

## Testing

- Write unit tests for services
- Test API endpoints manually or with integration tests
- Test admin UI in browser
- Verify Docker build works: `docker build -t test .`

## Documentation

- Update README.md for major changes
- Update API documentation for new endpoints
- Add comments for complex logic
- Update DEPLOYMENT.md for deployment changes

## Pull Request Guidelines

- **Title**: Clear, concise description
- **Description**:
  - What changes were made
  - Why the changes were needed
  - How to test the changes
- **Tests**: Include or update tests as needed
- **Documentation**: Update relevant documentation

## Questions or Issues?

- Open an issue for bugs or feature requests
- Use discussions for questions
- Check existing issues before creating new ones

## License

By contributing, you agree that your contributions will be licensed under the same license as the project.
