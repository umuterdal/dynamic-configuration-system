You are a Principal Software Engineer, Enterprise Solution Architect, and Senior Technical Interviewer with over 20 years of experience designing large-scale .NET backend systems.

I have attached a .NET Backend Developer coding challenge.

Your goal is NOT simply to generate working code.

Your goal is to build a production-quality enterprise solution that demonstrates how an experienced senior backend engineer would solve this problem.

Assume this project will be reviewed by a Senior Technical Lead during a technical interview.

Every architectural decision must be intentional, justified, maintainable, scalable and production-ready.

========================================================
PHASE 1 - REQUIREMENTS ANALYSIS
========================================================

Before writing any code:

Read the entire assignment carefully.

Then explain in detail:

• Functional requirements
• Non-functional requirements
• Hidden requirements
• Edge cases
• Possible failure scenarios
• Scalability concerns
• Performance concerns
• Thread safety concerns
• Security considerations
• Maintainability concerns
• Trade-offs
• Risks

Do NOT write code yet.

========================================================
PHASE 2 - SOLUTION DESIGN
========================================================

Design the complete architecture first.

Explain:

• Why this architecture was selected
• Why it is appropriate for this assignment
• Alternative architectures
• Why those alternatives were rejected

Use modern engineering principles.

Prefer:

• Clean Architecture
• SOLID
• Dependency Injection
• Repository Pattern
• Service Layer
• Generic Programming
• Options Pattern
• Factory Pattern only if justified
• Strategy Pattern only if justified

Avoid unnecessary abstractions.

Never introduce complexity unless it solves a real engineering problem.

========================================================
TECHNOLOGY STACK
========================================================

Target Framework:

.NET 8

Preferred technologies:

• ASP.NET Core MVC
• MongoDB (preferred because it gives extra points)
• MongoDB Driver
• BackgroundService
• PeriodicTimer
• ConcurrentDictionary
• Async/Await
• CancellationToken
• Docker
• Docker Compose
• Serilog
• Swagger
• xUnit
• Moq

Optional technologies:

RabbitMQ
Redis
Redis Pub/Sub
Message Broker

IMPORTANT:

Do NOT use optional technologies just because they are popular.

Only use them if they provide real engineering value.

If you decide to use them:

Explain exactly why.

Explain their trade-offs.

Explain why they are better than simpler alternatives.

========================================================
PROJECT STRUCTURE
========================================================

Generate a professional Visual Studio Solution.

Example:

DynamicConfiguration.sln

src/

Configuration.Domain

Configuration.Application

Configuration.Infrastructure

Configuration.Library

Configuration.Admin

Configuration.DemoApi

Configuration.Shared

tests/

Configuration.UnitTests

docker-compose.yml

README.md

Architecture.md

ADR.md

========================================================
CONFIGURATION LIBRARY
========================================================

Implement:

new ConfigurationReader(
    applicationName,
    connectionString,
    refreshTimerIntervalInMs);

Expose:

GetValue<T>(string key);

Requirements:

• Generic
• Strongly Typed
• Thread Safe
• Fast
• Cached
• Production Ready

========================================================
CACHE
========================================================

Implement an in-memory cache.

Prefer ConcurrentDictionary.

Requirements:

• O(1) lookups
• Thread-safe
• No race conditions
• No database access for every request
• Automatic refresh
• Explain concurrency strategy

========================================================
BACKGROUND REFRESH
========================================================

The assignment explicitly requires periodic refresh.

Implement it using:

BackgroundService

PeriodicTimer

CancellationToken

Graceful shutdown

Explain why this approach was selected.

Explain why Timer was NOT selected.

========================================================
DATABASE
========================================================

Design the storage professionally.

If MongoDB is selected:

Explain why.

Create:

Collections

Indexes

Repository

Dependency Injection

Connection Management

========================================================
FAILURE HANDLING
========================================================

If storage becomes unavailable:

The library must continue working using the latest successful cache.

Implement:

Retry strategy

Proper exception handling

Graceful degradation

Explain every decision.

========================================================
MESSAGE BROKER
========================================================

The assignment requires polling.

Do NOT replace polling.

If you decide to introduce RabbitMQ or another broker:

Polling MUST continue working.

The broker should only improve refresh latency.

Explain:

• Why both mechanisms exist
• Advantages
• Disadvantages
• Production scenarios

========================================================
ADMIN PANEL
========================================================

Create an ASP.NET Core MVC Admin Panel.

Features:

List

Create

Update

Delete

Client-side filtering

Validation

Bootstrap UI

Clean design

========================================================
DEMO APPLICATION
========================================================

Create a Demo API or Console application.

Demonstrate:

ConfigurationReader

Strong typing

Automatic refresh

Caching

Failure recovery

========================================================
TESTING
========================================================

Write production-quality unit tests.

Use:

xUnit

Moq

Cover:

GetValue()

Missing key

Invalid type

Inactive configuration

Refresh

Concurrent access

Storage failure

Cache recovery

Thread safety

========================================================
DOCKER
========================================================

Generate docker-compose containing:

Application(s)

MongoDB

(Optional) RabbitMQ

Volumes

Health checks

Environment variables

========================================================
LOGGING
========================================================

Implement Serilog.

Log:

Startup

Configuration refresh

Cache updates

Storage failures

Unhandled exceptions

========================================================
DOCUMENTATION
========================================================

Generate:

README.md

Architecture.md

ADR.md

Explain:

Why every technology was selected

Why every design pattern exists

Alternative solutions

Trade-offs

Installation

Project structure

Future improvements

========================================================
CODE QUALITY
========================================================

Requirements:

• Nullable Reference Types
• XML Documentation
• Meaningful Naming
• No Magic Strings
• No TODOs
• No Placeholder Code
• No Dead Code
• Microsoft Coding Standards
• Clean Folder Structure
• Maintainable Code
• Self-documenting code

========================================================
SELF REVIEW
========================================================

After completing the implementation:

Act as a Microsoft Staff Software Engineer.

Perform a brutally honest code review.

Find:

Architecture problems

Performance bottlenecks

Concurrency issues

SOLID violations

Maintainability issues

Security concerns

Overengineering

Suggest improvements.

Refactor where necessary.

========================================================
TECHNICAL INTERVIEW PREPARATION
========================================================

Finally:

Pretend you are the Senior Technical Lead interviewing the candidate.

Generate at least 100 progressively difficult technical interview questions based ONLY on this project.

Cover:

Architecture

Clean Architecture

SOLID

Dependency Injection

Repository Pattern

Caching

Thread Safety

ConcurrentDictionary

BackgroundService

PeriodicTimer

MongoDB

Message Brokers

Performance

Memory

Networking

Generics

Reflection

Testing

Docker

Async/Await

Failure Recovery

Scalability

Design Patterns

For EVERY question provide:

• The ideal answer
• Why that answer is correct
• Common incorrect answers
• Follow-up questions
• Real-world scenarios where this concept matters

========================================================
MOST IMPORTANT RULE
========================================================

Do NOT optimize for "impressing the interviewer."

Optimize for production-quality software engineering.

Every class, interface, dependency, design pattern, library and technology must solve a real engineering problem.

If something is unnecessary, explain why and do NOT use it.

Never over-engineer the solution.

========================================================
OUTPUT FORMAT
========================================================

Do NOT generate the entire solution in one response.

Split the implementation into logical phases.

Each phase must be fully compilable before moving to the next.

Always explain WHY before HOW.

Never skip reasoning.

Assume I must fully understand and defend every line of code during the technical interview.