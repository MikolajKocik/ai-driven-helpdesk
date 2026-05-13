# AI-Driven Helpdesk (ADH)

**A modern, intelligent support platform that combines AI-powered automation with enterprise security and infrastructure autonomy.**

ADH is an advanced helpdesk solution designed to transform IT operations through intelligent automation, privacy-first AI integration, and proactive system management. Built for enterprise environments, it delivers immediate value through AI-assisted support while maintaining strict data sovereignty and compliance standards.

---

## The Problem ADH Solves

Modern IT departments face critical operational challenges that traditional helpdesk solutions cannot address:

### Operational Inefficiency
- **Repetitive Manual Tasks:** IT teams spend significant time on routine ticket handling, password resets, and basic troubleshooting that could be automated.
- **Reactive Infrastructure:** Systems are monitored reactively, leading to outages and service degradation instead of proactive prevention.
- **Asset Visibility Gap:** Manual asset tracking results in inaccurate CMDB records, leaving organizations blind to their infrastructure.

### Security & Compliance Risks
- **Data Privacy Concerns:** Integrating external AI services exposes sensitive corporate data and employee PII to third-party vendors.
- **Regulatory Compliance:** Organizations cannot meet data residency and sovereignty requirements with cloud-dependent solutions.
- **Prompt Injection Vulnerabilities:** AI-driven systems are susceptible to manipulation through malicious user input.

### User Experience Limitations
- **Slow Support Response:** Manual processes delay problem resolution, impacting user productivity.
- **Inconsistent Support Quality:** Helpdesk quality depends on staff availability and expertise levels.

### ADH's Solution

ADH addresses these challenges through:
- **AI-Powered Automation:** Semantic Kernel orchestration reduces manual workload by intelligently triaging and responding to support requests.
- **Privacy-by-Design:** Built-in PII scrubber removes sensitive information before AI processing; full support for sovereign AI deployments via Ollama.
- **Infrastructure Autonomy:** Proactive self-healing monitors disk space, restarts failing services, and performs preventive maintenance automatically.
- **Automated Asset Discovery:** LDAP/Active Directory integration and network scanning maintain an accurate, up-to-date CMDB.
- **Real-Time Support:** SignalR-powered chat interface with streaming responses provides immediate assistance to end users.

---

## Key Features

- **Intelligent Chat Assistant** — AI-powered support interface with real-time streaming and live status indicators
- **Privacy-First AI** — Automatic PII scrubbing and support for air-gapped deployments with local LLMs
- **Self-Healing Infrastructure** — Proactive monitoring and automated remediation of common issues
- **Asset Intelligence** — Automatic discovery and inventory management via LDAP, Active Directory, and network scanning
- **Jira Integration** — Seamless ticket management and workflow orchestration
- **Prompt Injection Protection** — Security-hardened AI prompts resistant to manipulation attacks
- **Real-Time Notifications** — Live updates and status streaming via SignalR
- **Sovereign Deployment** — Full support for private, air-gapped environments with no external dependencies

---

## Technology Stack

### Backend
- **.NET 10** (C#) — Modern, high-performance application framework
- **Clean Architecture** — Maintainable, testable, and scalable design patterns
- **Entity Framework Core** — Data access and ORM
- **Microsoft Semantic Kernel** — AI orchestration and LLM integration
- **SignalR** — Real-time bidirectional communication

### Frontend
- **React 19** with **TypeScript** — Modern, type-safe UI framework
- **Vite** — Next-generation build tool for fast development
- **Tailwind CSS** — Utility-first styling framework
- **Shadcn UI** — High-quality, accessible component library

### Data & Infrastructure
- **PostgreSQL** — Robust relational database
- **pgvector** — Vector similarity search for semantic search capabilities
- **Docker & Docker Compose** — Containerized deployment and orchestration
- **LDAP/Active Directory Integration** — Enterprise authentication and asset discovery

### AI & Integrations
- **Microsoft Semantic Kernel** — LLM orchestration and prompt management
- **Ollama** — Local LLM deployment for sovereign AI
- **Jira API** — Enterprise ticket system integration

---

## Getting Started

### Prerequisites
- Docker and Docker Compose
- .NET 10 SDK (for local development)
- Node.js 18+ and npm (for frontend development)
- PostgreSQL 14+ (if running without Docker)

### Quick Start

1. **Clone the repository:**
   ```bash
   git clone <repository-url>
   cd ADH
   ```

2. **Configure environment variables:**
   ```bash
   cp .env.example .env
   # Edit .env with your configuration
   ```

3. **Start the application:**
   ```bash
   docker-compose up --build
   ```

4. **Access the application:**
   - Frontend: `http://localhost:3000`
   - API Documentation: `http://localhost:5000/swagger`

---

## Architecture Overview

ADH follows a layered architecture pattern:

- **API Layer** — RESTful endpoints with comprehensive error handling and validation
- **Application Layer** — Business logic, DTOs, and validators following CQRS patterns
- **Domain Layer** — Core entities and business rules
- **Infrastructure Layer** — Data persistence, external integrations, and background services

---

## Security & Compliance

ADH implements security as a core design principle:

- **Prompt Injection Protection** — Hardened prompt templates and input validation
- **Automated Data Sanitization** — PII scrubber prevents sensitive information from reaching AI models
- **Air-Gapped Deployment** — Full support for offline environments with local LLMs and no external API calls
- **LDAP/AD Integration** — Enterprise authentication and authorization
- **Role-Based Access Control** — Granular permission management for multi-tenant environments
- **Audit Logging** — Complete activity tracking for compliance audits
