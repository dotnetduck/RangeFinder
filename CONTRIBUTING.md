# Contributing to RangeFinder

> **⚠️ DRAFT**: These contributing guidelines are a work in progress. CI/CD automation and NuGet publishing workflows are not yet implemented.

Thank you for your interest in contributing to RangeFinder! This document provides guidelines for contributing to this high-performance range query library.

## Getting Started

1. **Fork the repository** on GitHub
2. **Clone your fork** locally:
   ```bash
   git clone https://github.com/your-username/RangeFinder.git
   cd RangeFinder
   ```
3. **Build and test** to ensure everything works:
   ```bash
   dotnet build
   dotnet test
   ```

## Development Workflow

### Before Making Changes

1. **Create a feature branch** from the main branch:
   ```bash
   git checkout -b feature/your-feature-name
   ```
2. **Understand the performance requirements** - RangeFinder is a performance-first library
3. **Review existing tests** to understand expected behavior

### Making Changes

#### Code Guidelines

- **Follow existing code style** and patterns in the codebase
- **Use meaningful variable and method names**
- **Add XML documentation** for public APIs
- **Maintain O(log N + K) query complexity** for core algorithms
- **Preserve backward compatibility** unless explicitly breaking change

#### Testing Requirements

All contributions must include appropriate tests:

```bash
# Run all tests
dotnet test

# Run specific test project
dotnet test RangeFinderTests/
```

**Test Categories:**
- **Unit tests**: Core functionality and edge cases
- **Compatibility tests**: Ensure RangeTree API compatibility
- **Performance validation**: Verify no performance regressions

#### Performance Considerations

RangeFinder prioritizes performance above convenience. When contributing:

1. **Benchmark significant changes**:
   ```bash
   dotnet run --project RangeFinder.Benchmark -c Release
   ```
2. **Run performance guardian** to detect regressions:
   ```bash
   dotnet run --project RangeFinder.Validator -c Release -- --guardian
   ```
3. **Avoid allocations** in hot paths
4. **Prefer arrays over lists** for performance-critical code
5. **Consider cache locality** in data structure design

### Submitting Changes

#### Before Submitting

1. **Ensure all tests pass**:
   ```bash
   dotnet test --verbosity normal
   ```
2. **Build in Release mode**:
   ```bash
   dotnet build -c Release
   ```
3. **Run benchmarks** if you modified core algorithms
4. **Update documentation** for new features or API changes

#### Pull Request Guidelines

**PR Title Format:**
- `feat: Add new feature description`
- `fix: Fix specific issue description`
- `perf: Performance improvement description`
- `docs: Documentation update description`
- `test: Test improvement description`

**PR Description Should Include:**
- Clear description of changes made
- Reason for the change
- Any performance impact (positive or negative)
- Test coverage for new code
- Breaking changes (if any)

**Example PR Description:**
```markdown
## Summary
Add support for custom comparison operators in NumericRange.

## Changes
- Add IComparer<T> support to RangeFinder constructor
- Update binary search to use custom comparer
- Add compatibility tests for custom comparers

## Performance Impact
- No performance regression (verified with benchmarks)
- Same O(log N + K) complexity maintained

## Testing
- Added 15 new unit tests covering custom comparer scenarios
- All existing tests pass
- Compatibility with RangeTree maintained
```

## Types of Contributions

### 🐛 Bug Fixes
- **Include test cases** that reproduce the bug
- **Verify the fix** doesn't break existing functionality
- **Document any edge cases** discovered

### ✨ New Features
- **Discuss large features** in an issue first
- **Maintain API compatibility** with RangeTree where possible
- **Include comprehensive tests** and documentation
- **Benchmark performance impact**

### 📊 Performance Improvements
- **Provide benchmark evidence** of improvement
- **Ensure no regression** in other scenarios
- **Document algorithmic complexity** if changed
- **Test across different dataset characteristics** (Dense, Sparse, Uniform)

### 📝 Documentation
- **Update relevant markdown files**
- **Include code examples** for new features
- **Verify examples compile and run**
- **Update benchmark tables** if performance changed

### 🧪 Tests
- **Follow existing test patterns**
- **Include edge cases and boundary conditions**
- **Use descriptive test names**
- **Test both RangeTree compatibility and native APIs**

## Code Review Process

> **Note**: Automated CI/CD workflows are in development. Currently manual verification is required.

**Planned automation:**
1. **CI Pipeline**: Automated build, test, and performance guardian checks on PRs
2. **Performance validation**: Automated benchmarks on significant changes  
3. **NuGet publishing**: Automatic package publishing on version tags
4. **Code review** by maintainers
5. **Documentation review** for public API changes

**Current manual process:**
1. Verify all tests pass locally: `dotnet test`
2. Run performance guardian: `dotnet run --project RangeFinder.Validator -c Release -- --guardian`
3. Manual code review by maintainers
4. Manual merge after approval

## Performance Standards

RangeFinder maintains strict performance requirements:

- **Query performance**: Must remain O(log N + K)
- **Construction performance**: Should scale linearly or better
- **Memory efficiency**: Minimize allocations in query paths
- **Regression detection**: Use RangeFinder.Validator for validation

## Getting Help

- **Create an issue** for questions or proposals
- **Review existing issues** before creating new ones
- **Check CLAUDE.md** for project-specific guidance
- **Look at recent pull requests** for examples

## Project Structure

```
RangeFinder/
├── RangeFinder.Core/      # Core library
├── RangeFinderTests/      # Unit and integration tests
├── RangeFinder.Benchmark/ # Performance benchmarks
├── RangeFinder.Validator/ # Performance regression detection and validation
└── RangeGenerator/        # Test data generation utilities
```

## Recognition

Contributors will be recognized in:
- **Git commit history** with proper attribution
- **Release notes** for significant contributions
- **Documentation** for major features

Thank you for helping make RangeFinder better! 🚀